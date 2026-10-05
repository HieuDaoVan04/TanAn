// "Một sản phẩm của HieuDV"

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Services
{
    public class PopulationService : IPopulationService
    {
        private readonly ITanAnDbContext _db;
        private readonly IAuditLogService _auditLog;

        public PopulationService(ITanAnDbContext db, IAuditLogService auditLog)
        {
            _db = db;
            _auditLog = auditLog;
        }

        public async Task<ApiResult<PagedResult<HoGiaDinhDto>>> GetHoGiaDinhsAsync(string? keyword, string? apThon, int pageIndex, int pageSize, Guid? groupId = null)
        {
            var query = _db.HoGiaDinhs.AsNoTracking().AsQueryable();
            if (groupId.HasValue) query = query.Where(h => h.DiaBan != null && h.DiaBan.GroupId == groupId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(h => h.MaSoHo.Contains(keyword) || h.TenChuHo.Contains(keyword) || h.CCCDChuHo.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(apThon))
            {
                query = query.Where(h => h.ApThon == apThon);
            }

            int totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(h => h.NgayTao).ThenBy(h => h.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HoGiaDinhDto
                {
                    Id = h.Id,
                    MaSoHo = h.MaSoHo,
                    TenChuHo = h.TenChuHo,
                    CCCDChuHo = h.CCCDChuHo,
                    DiaChi = h.DiaChi,
                    ApThon = h.ApThon,
                    SoThanhVien = h.ThanhVien != null ? h.ThanhVien.Count : 0,
                    NgayTao = h.NgayTao,
                    GhiChu = h.GhiChu
                })
                .ToListAsync();

            return ApiResult<PagedResult<HoGiaDinhDto>>.Ok(new PagedResult<HoGiaDinhDto>(items, totalCount, pageIndex, pageSize));
        }

        public async Task SetChuHoAsync(Guid householdId, Guid residentId, string username)
        {
            var context = (DbContext)_db;
            await using var transaction = await context.Database.BeginTransactionAsync();
            var house = await _db.HoGiaDinhs.SingleOrDefaultAsync(x=>x.Id==householdId) ?? throw new UnauthorizedAccessException("Không có quyền quản lý hộ này.");
            var resident = await _db.NhanKhaus.SingleOrDefaultAsync(x=>x.Id==residentId && x.MaHoGiaDinh==householdId) ?? throw new ArgumentException("Chủ hộ phải là thành viên của hộ.");
            var active = await _db.ThanhVienHos.Where(x=>x.HoGiaDinhId==householdId && x.DenNgay==null && (x.LaChuHo || x.NhanKhauId==residentId)).ToListAsync();
            if(active.Any(x=>x.LaChuHo && x.NhanKhauId==residentId)) return;
            var oldHead = active.FirstOrDefault(x => x.LaChuHo);
            var oldHeadName = oldHead == null
                ? null
                : await _db.NhanKhaus.Where(x => x.Id == oldHead.NhanKhauId).Select(x => x.HoTen).SingleOrDefaultAsync();
            var oldValues = oldHead == null
                ? null
                : JsonSerializer.Serialize(new { ChuHoId = oldHead.NhanKhauId, HoTen = oldHeadName });
            var today = DateOnly.FromDateTime(DateTime.Today);
            if(active.Any(x=>x.TuNgay>today)) throw new InvalidOperationException("Ngày hiệu lực thành viên đang nằm trong tương lai.");
            foreach(var item in active) item.DenNgay=today;
            await _db.SaveChangesAsync(); // close old head before inserting the new unique active head
            foreach(var old in active.Where(x=>x.LaChuHo && x.NhanKhauId!=residentId))
            {
                var prior = await _db.NhanKhaus.SingleAsync(x=>x.Id==old.NhanKhauId);
                prior.QuanHeVoiChuHo="Thành viên";
                _db.ThanhVienHos.Add(new ThanhVienHo {HoGiaDinhId=householdId,NhanKhauId=prior.Id,QuanHeVoiChuHo="Thành viên",TuNgay=today});
            }
            resident.QuanHeVoiChuHo="Chủ hộ";
            house.TenChuHo=resident.HoTen; house.CCCDChuHo=resident.CCCD;
            _db.ThanhVienHos.Add(new ThanhVienHo {HoGiaDinhId=householdId,NhanKhauId=residentId,QuanHeVoiChuHo="Chủ hộ",LaChuHo=true,TuNgay=today});
            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(
                username,
                "Thay đổi chủ hộ",
                "HoGiaDinh",
                householdId.ToString(),
                oldValues,
                JsonSerializer.Serialize(new { ChuHoId = residentId, HoTen = resident.HoTen }));
            await transaction.CommitAsync();
        }

        public async Task<ApiResult<HoGiaDinhDto>> GetHoGiaDinhByIdAsync(Guid id)
        {
            var h = await _db.HoGiaDinhs.Include(x => x.ThanhVien).FirstOrDefaultAsync(x => x.Id == id);
            if (h == null) return ApiResult<HoGiaDinhDto>.Fail("Hộ gia đình không tồn tại.");

            var dto = new HoGiaDinhDto
            {
                Id = h.Id,
                MaSoHo = h.MaSoHo,
                TenChuHo = h.TenChuHo,
                CCCDChuHo = h.CCCDChuHo,
                DiaChi = h.DiaChi,
                ApThon = h.ApThon,
                SoThanhVien = h.ThanhVien?.Count ?? 0,
                NgayTao = h.NgayTao,
                GhiChu = h.GhiChu,
                ThanhVien = h.ThanhVien?.Select(n => new NhanKhauDto
                {
                    Id = n.Id,
                    HoTen = n.HoTen,
                    CCCD = n.CCCD,
                    NgaySinh = n.NgaySinh,
                    GioiTinh = n.GioiTinh,
                    QuanHeVoiChuHo = n.QuanHeVoiChuHo,
                    NgheNghiep = n.NgheNghiep,
                    TamTru = n.TamTru,
                    MaHoGiaDinh = n.MaHoGiaDinh
                }).ToList() ?? new()
            };

            return ApiResult<HoGiaDinhDto>.Ok(dto);
        }

        public async Task<ApiResult<HoGiaDinhDto>> CreateHoGiaDinhAsync(CreateHoGiaDinhForm form, string username)
        {
            if (await _db.HoGiaDinhs.AnyAsync(h => h.MaSoHo == form.MaSoHo))
            {
                return ApiResult<HoGiaDinhDto>.Fail($"Mã sổ hộ {form.MaSoHo} đã tồn tại.");
            }

            if (form.NgaySinhChuHo == null || form.NgaySinhChuHo > DateTime.Today || form.NgaySinhChuHo < new DateTime(1900,1,1)) return ApiResult<HoGiaDinhDto>.Fail("Nhập ngày sinh hợp lệ của chủ hộ.");
            var villageId = await _db.ApThons.Where(x=>x.Ten==form.ApThon && x.DangHoatDong).Select(x=>(Guid?)x.Id).SingleOrDefaultAsync();
            if(villageId == null) return ApiResult<HoGiaDinhDto>.Fail("Hãy chọn thôn trong danh mục.");
            var ho = new HoGiaDinh
            {
                MaSoHo = form.MaSoHo,
                TenChuHo = form.TenChuHo,
                CCCDChuHo = form.CCCDChuHo,
                DiaChi = form.DiaChi,
                ApThon = form.ApThon,
                ApThonId = villageId,
                NgayTao = DateTime.UtcNow,
                GhiChu = form.GhiChu
            };

            var head = new NhanKhau {HoGiaDinh=ho,HoTen=form.TenChuHo,CCCD=form.CCCDChuHo,NgaySinh=DateTime.SpecifyKind(form.NgaySinhChuHo.Value,DateTimeKind.Utc),GioiTinh=form.GioiTinhChuHo,QuanHeVoiChuHo="Chủ hộ",ThuongTru=form.DiaChi};
            _db.NhanKhaus.Add(head);
            _db.ThanhVienHos.Add(new ThanhVienHo {HoGiaDinh=ho,NhanKhau=head,LaChuHo=true,QuanHeVoiChuHo="Chủ hộ",TuNgay=DateOnly.FromDateTime(DateTime.Today)});
            await _db.HoGiaDinhs.AddAsync(ho);
            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(
                username,
                "Tạo mới hộ gia đình",
                "HoGiaDinh",
                ho.Id.ToString(),
                null,
                JsonSerializer.Serialize(new { ho.MaSoHo, ho.TenChuHo, ho.ApThon, ho.DiaChi }));

            return ApiResult<HoGiaDinhDto>.Ok(new HoGiaDinhDto
            {
                Id = ho.Id,
                MaSoHo = ho.MaSoHo,
                TenChuHo = ho.TenChuHo,
                CCCDChuHo = ho.CCCDChuHo,
                DiaChi = ho.DiaChi,
                ApThon = ho.ApThon,
                NgayTao = ho.NgayTao,
                GhiChu = ho.GhiChu
            });
        }

        public async Task<ApiResult<PagedResult<NhanKhauDto>>> GetNhanKhausAsync(string? keyword, string? apThon, int pageIndex, int pageSize, Guid? groupId = null)
        {
            var query = _db.NhanKhaus.AsNoTracking().AsQueryable();
            if (groupId.HasValue) query = query.Where(n => n.HoGiaDinh != null && n.HoGiaDinh.DiaBan != null && n.HoGiaDinh.DiaBan.GroupId == groupId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(n => n.HoTen.Contains(keyword) || n.CCCD.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(apThon))
            {
                query = query.Where(n => n.HoGiaDinh != null && n.HoGiaDinh.ApThon == apThon);
            }

            int totalCount = await query.CountAsync();
            var items = await query.OrderBy(n => n.HoTen).ThenBy(n => n.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NhanKhauDto
                {
                    Id = n.Id,
                    MaHoGiaDinh = n.MaHoGiaDinh,
                    MaSoHo = n.HoGiaDinh != null ? n.HoGiaDinh.MaSoHo : string.Empty,
                    TenChuHo = n.HoGiaDinh != null ? n.HoGiaDinh.TenChuHo : string.Empty,
                    HoTen = n.HoTen,
                    CCCD = n.CCCD,
                    NgaySinh = n.NgaySinh,
                    GioiTinh = n.GioiTinh,
                    QuanHeVoiChuHo = n.QuanHeVoiChuHo,
                    DanToc = n.DanToc,
                    TonGiao = n.TonGiao,
                    NgheNghiep = n.NgheNghiep,
                    TamTru = n.TamTru,
                    TrangThai = n.TrangThai,
                    NgayTao = n.NgayTao
                })
                .ToListAsync();

            return ApiResult<PagedResult<NhanKhauDto>>.Ok(new PagedResult<NhanKhauDto>(items, totalCount, pageIndex, pageSize));
        }

        public async Task<ApiResult<NhanKhauDto>> GetNhanKhauByIdAsync(Guid id)
        {
            var n = await _db.NhanKhaus.Include(x => x.HoGiaDinh).FirstOrDefaultAsync(x => x.Id == id);
            if (n == null) return ApiResult<NhanKhauDto>.Fail("Nhân khẩu không tồn tại.");

            return ApiResult<NhanKhauDto>.Ok(new NhanKhauDto
            {
                Id = n.Id,
                MaHoGiaDinh = n.MaHoGiaDinh,
                MaSoHo = n.HoGiaDinh?.MaSoHo,
                TenChuHo = n.HoGiaDinh?.TenChuHo,
                HoTen = n.HoTen,
                CCCD = n.CCCD,
                NgaySinh = n.NgaySinh,
                GioiTinh = n.GioiTinh,
                DanToc = n.DanToc,
                TonGiao = n.TonGiao,
                NgheNghiep = n.NgheNghiep,
                QuanHeVoiChuHo = n.QuanHeVoiChuHo,
                ThuongTru = n.ThuongTru, QueQuan = n.QueQuan, TrinhDoHocVan = n.TrinhDoHocVan, GhiChu = n.GhiChu,
                TamTru = n.TamTru,
                TrangThai = n.TrangThai,
                NgayTao = n.NgayTao
            });
        }

        public async Task<ApiResult<NhanKhauDto>> CreateNhanKhauAsync(CreateNhanKhauForm form, string username)
        {
            if(!await _db.HoGiaDinhs.AnyAsync(x=>x.Id==form.MaHoGiaDinh)) return ApiResult<NhanKhauDto>.Fail("Hộ không tồn tại hoặc ngoài thôn được giao.");
            if(form.QuanHeVoiChuHo=="Chủ hộ") return ApiResult<NhanKhauDto>.Fail("Thêm thành viên trước rồi dùng chức năng chọn chủ hộ.");
            var nk = new NhanKhau
            {
                MaHoGiaDinh = form.MaHoGiaDinh,
                HoTen = form.HoTen,
                CCCD = form.CCCD,
                NgaySinh = form.NgaySinh,
                GioiTinh = form.GioiTinh,
                QuanHeVoiChuHo = form.QuanHeVoiChuHo,
                DanToc = form.DanToc,
                TonGiao = form.TonGiao,
                NgheNghiep = form.NgheNghiep,
                ThuongTru = form.ThuongTru,
                TamTru = form.TamTru,
                TrinhDoHocVan = form.TrinhDoHocVan,
                QueQuan = form.QueQuan,
                GhiChu = form.GhiChu
            };

            await _db.NhanKhaus.AddAsync(nk);
            _db.ThanhVienHos.Add(new ThanhVienHo {HoGiaDinhId=form.MaHoGiaDinh,NhanKhau=nk,QuanHeVoiChuHo=form.QuanHeVoiChuHo,TuNgay=DateOnly.FromDateTime(DateTime.Today)});
            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(
                username,
                "Tạo nhân khẩu",
                "NhanKhau",
                nk.Id.ToString(),
                null,
                JsonSerializer.Serialize(new
                {
                    nk.HoTen,
                    nk.MaHoGiaDinh,
                    nk.QuanHeVoiChuHo,
                    nk.NgaySinh,
                    nk.GioiTinh,
                    nk.TrangThai
                }));

            return ApiResult<NhanKhauDto>.Ok(new NhanKhauDto
            {
                Id = nk.Id,
                MaHoGiaDinh = nk.MaHoGiaDinh,
                HoTen = nk.HoTen,
                CCCD = nk.CCCD,
                NgaySinh = nk.NgaySinh,
                GioiTinh = nk.GioiTinh,
                QuanHeVoiChuHo = nk.QuanHeVoiChuHo,
                DanToc = nk.DanToc,
                TonGiao = nk.TonGiao,
                NgheNghiep = nk.NgheNghiep,
                TamTru = nk.TamTru,
                TrangThai = nk.TrangThai,
                NgayTao = nk.NgayTao
            });
        }

        public async Task<ApiResult<NhanKhauDto>> UpdateNhanKhauAsync(Guid id, CreateNhanKhauForm form, string username)
        {
            var nk = await _db.NhanKhaus.FindAsync(id);
            if (nk == null) return ApiResult<NhanKhauDto>.Fail("Nhân khẩu không tồn tại.");

            var oldValues = JsonSerializer.Serialize(new
            {
                nk.HoTen,
                nk.MaHoGiaDinh,
                nk.QuanHeVoiChuHo,
                nk.NgaySinh,
                nk.GioiTinh,
                nk.DanToc,
                nk.TonGiao,
                nk.NgheNghiep,
                nk.ThuongTru,
                nk.TamTru,
                nk.TrinhDoHocVan,
                nk.GhiChu,
                nk.TrangThai
            });

            if(nk.QuanHeVoiChuHo != form.QuanHeVoiChuHo && (nk.QuanHeVoiChuHo=="Chủ hộ" || form.QuanHeVoiChuHo=="Chủ hộ")) return ApiResult<NhanKhauDto>.Fail("Dùng chức năng chọn chủ hộ để thay đổi chủ hộ.");
            var house = await _db.HoGiaDinhs.SingleAsync(x=>x.Id==nk.MaHoGiaDinh);
            if(await _db.ThanhVienHos.AnyAsync(x=>x.NhanKhauId==nk.Id && x.DenNgay==null && x.LaChuHo)) { house.TenChuHo=form.HoTen;house.CCCDChuHo=form.CCCD; }
            nk.HoTen = form.HoTen;
            nk.CCCD = form.CCCD;
            nk.NgaySinh = form.NgaySinh;
            nk.GioiTinh = form.GioiTinh;
            nk.QuanHeVoiChuHo = form.QuanHeVoiChuHo;
            nk.DanToc = form.DanToc;
            nk.TonGiao = form.TonGiao;
            nk.NgheNghiep = form.NgheNghiep;
            nk.ThuongTru = form.ThuongTru;
            nk.TamTru = form.TamTru;
            nk.TrinhDoHocVan = form.TrinhDoHocVan;
            nk.GhiChu = form.GhiChu;

            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(
                username,
                "Cập nhật nhân khẩu",
                "NhanKhau",
                id.ToString(),
                oldValues,
                JsonSerializer.Serialize(new
                {
                    nk.HoTen,
                    nk.MaHoGiaDinh,
                    nk.QuanHeVoiChuHo,
                    nk.NgaySinh,
                    nk.GioiTinh,
                    nk.DanToc,
                    nk.TonGiao,
                    nk.NgheNghiep,
                    nk.ThuongTru,
                    nk.TamTru,
                    nk.TrinhDoHocVan,
                    nk.GhiChu,
                    nk.TrangThai
                }));

            return ApiResult<NhanKhauDto>.Ok(new NhanKhauDto
            {
                Id = nk.Id,
                MaHoGiaDinh = nk.MaHoGiaDinh,
                HoTen = nk.HoTen,
                CCCD = nk.CCCD,
                NgaySinh = nk.NgaySinh,
                GioiTinh = nk.GioiTinh,
                QuanHeVoiChuHo = nk.QuanHeVoiChuHo,
                DanToc = nk.DanToc,
                TonGiao = nk.TonGiao,
                NgheNghiep = nk.NgheNghiep,
                TamTru = nk.TamTru,
                TrangThai = nk.TrangThai,
                NgayTao = nk.NgayTao
            });
        }

        public async Task<ApiResult<PagedResult<BienDongDto>>> GetBienDongsAsync(string? keyword, int pageIndex, int pageSize)
        {
            var query = _db.BienDongDanCus.Include(b => b.NhanKhau).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(b => b.LyDo.Contains(keyword) || (b.NhanKhau != null && b.NhanKhau.HoTen.Contains(keyword)));
            }

            int totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(b => b.NgayTao)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BienDongDto
                {
                    Id = b.Id,
                    LoaiBienDong = b.LoaiBienDong,
                    NhanKhauId = b.NhanKhauId,
                    HoTenNhanKhau = b.NhanKhau != null ? b.NhanKhau.HoTen : string.Empty,
                    CCCDNhanKhau = b.NhanKhau != null ? b.NhanKhau.CCCD : string.Empty,
                    NgayPhatSinh = b.NgayPhatSinh,
                    NoiDenOrDi = b.NoiDenOrDi,
                    LyDo = b.LyDo,
                    CanBoGhiNhan = b.CanBoGhiNhan,
                    NgayTao = b.NgayTao
                })
                .ToListAsync();

            return ApiResult<PagedResult<BienDongDto>>.Ok(new PagedResult<BienDongDto>(items, totalCount, pageIndex, pageSize));
        }

        public async Task<ApiResult<BienDongDto>> CreateBienDongAsync(CreateBienDongForm form, string username)
        {
            var bd = new BienDongDanCu
            {
                NhanKhauId = form.NhanKhauId,
                LoaiBienDong = form.LoaiBienDong,
                NgayPhatSinh = form.NgayPhatSinh,
                NoiDenOrDi = form.NoiDenOrDi,
                LyDo = form.LyDo,
                CanBoGhiNhan = username,
                NgayTao = DateTime.UtcNow
            };

            await _db.BienDongDanCus.AddAsync(bd);

            if (form.LoaiBienDong == LoaiBienDongEnum.TamVang || form.LoaiBienDong == LoaiBienDongEnum.ChuyenDi)
            {
                var nk = await _db.NhanKhaus.FindAsync(form.NhanKhauId);
                if (nk != null) nk.TrangThai = form.LoaiBienDong == LoaiBienDongEnum.TamVang ? TrangThaiNhanKhauEnum.TamVang : TrangThaiNhanKhauEnum.DaChuyenDi;
            }

            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(
                username,
                "Đăng ký biến động dân cư",
                "BienDongDanCu",
                bd.Id.ToString(),
                null,
                JsonSerializer.Serialize(new
                {
                    bd.NhanKhauId,
                    bd.LoaiBienDong,
                    bd.NgayPhatSinh,
                    bd.NoiDenOrDi,
                    bd.LyDo
                }));

            return ApiResult<BienDongDto>.Ok(new BienDongDto
            {
                Id = bd.Id,
                LoaiBienDong = form.LoaiBienDong,
                NhanKhauId = bd.NhanKhauId,
                NgayPhatSinh = bd.NgayPhatSinh,
                NoiDenOrDi = bd.NoiDenOrDi,
                LyDo = bd.LyDo,
                CanBoGhiNhan = bd.CanBoGhiNhan,
                NgayTao = bd.NgayTao
            });
        }
    }
}


