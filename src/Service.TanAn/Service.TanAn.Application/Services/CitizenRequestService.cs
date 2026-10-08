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
    public class CitizenRequestService : ICitizenRequestService
    {
        private readonly ITanAnDbContext _db;
        private readonly IAuditLogService _auditLog;

        public CitizenRequestService(ITanAnDbContext db, IAuditLogService auditLog)
        {
            _db = db;
            _auditLog = auditLog;
        }

        public async Task<ApiResult<PagedResult<YeuCauDto>>> GetYeuCausAsync(string? keyword, int? trangThai, int pageIndex, int pageSize)
        {
            var query = _db.YeuCauNguoiDans.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(r => r.MaYeuCau.Contains(keyword) || r.HoTenNguoiYeuCau.Contains(keyword) || r.CCCDNguoiYeuCau.Contains(keyword) || r.LoaiYeuCau.Contains(keyword));
            }

            if (trangThai.HasValue)
            {
                query = query.Where(r => (int)r.TrangThai == trangThai.Value);
            }

            int totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(r => r.NgayGui)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new YeuCauDto
                {
                    Id = r.Id,
                    MaYeuCau = r.MaYeuCau,
                    HoTenNguoiYeuCau = r.HoTenNguoiYeuCau,
                    CCCDNguoiYeuCau = r.CCCDNguoiYeuCau,
                    SoDienThoai = r.SoDienThoai,
                    LoaiYeuCau = r.LoaiYeuCau,
                    NoiDung = r.NoiDung,
                    TrangThai = r.TrangThai,
                    CanBoXuLy = r.CanBoXuLy,
                    GhiChuCanBo = r.GhiChuCanBo,
                    NgayGui = r.NgayGui,
                    NgayCapNhat = r.NgayCapNhat
                })
                .ToListAsync();

            return ApiResult<PagedResult<YeuCauDto>>.Ok(new PagedResult<YeuCauDto>(items, totalCount, pageIndex, pageSize));
        }

        public async Task<ApiResult<YeuCauDto>> CreateYeuCauAsync(CreateYeuCauForm form)
        {
            if (!form.ApThonId.HasValue || !await _db.ApThons.AnyAsync(x=>x.Id==form.ApThonId && x.DangHoatDong)) return ApiResult<YeuCauDto>.Fail("Hãy chọn thôn tiếp nhận hồ sơ.");
            var req = new YeuCauNguoiDan
            {
                ApThonId = form.ApThonId,
                MaYeuCau = $"YC{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}",
                HoTenNguoiYeuCau = form.HoTenNguoiYeuCau,
                CCCDNguoiYeuCau = form.CCCDNguoiYeuCau,
                SoDienThoai = form.SoDienThoai,
                LoaiYeuCau = form.LoaiYeuCau,
                NoiDung = form.NoiDung,
                NgayGui = DateTime.Now,
                NgayCapNhat = DateTime.Now,
                TrangThai = TrangThaiHoSoEnum.MoiTiepNhan
            };

            await _db.YeuCauNguoiDans.AddAsync(req);
            var recipients = await _db.PhuTrachThons.Where(p=>p.ApThonId==form.ApThonId && p.User.Role==RoleEnum.CanBoThon && p.User.ModerationStatus==Service.Shared.Commons.Model.SQL.ModerationStatus.Approved).Select(p=>p.UserId).ToListAsync();
            foreach(var recipient in recipients) _db.ThongBaoThons.Add(new ThongBaoThon {NguoiNhanId=recipient,ApThonId=form.ApThonId.Value,TieuDe="Hồ sơ mới cần xử lý",NoiDung=$"Hồ sơ {req.MaYeuCau}: {req.LoaiYeuCau}."});
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync(
                form.HoTenNguoiYeuCau,
                "Tạo mới yêu cầu",
                "YeuCauNguoiDan",
                req.Id.ToString(),
                null,
                JsonSerializer.Serialize(new
                {
                    req.MaYeuCau,
                    req.LoaiYeuCau,
                    req.TrangThai,
                    req.ApThonId
                }));

            return ApiResult<YeuCauDto>.Ok(new YeuCauDto
            {
                Id = req.Id,
                MaYeuCau = req.MaYeuCau,
                HoTenNguoiYeuCau = req.HoTenNguoiYeuCau,
                CCCDNguoiYeuCau = req.CCCDNguoiYeuCau,
                SoDienThoai = req.SoDienThoai,
                LoaiYeuCau = req.LoaiYeuCau,
                NoiDung = req.NoiDung,
                TrangThai = req.TrangThai,
                NgayGui = req.NgayGui,
                NgayCapNhat = req.NgayCapNhat
            });
        }

        public async Task<ApiResult<YeuCauDto>> UpdateYeuCauStatusAsync(UpdateYeuCauStatusForm form)
        {
            var req = await _db.YeuCauNguoiDans.FindAsync(form.YeuCauId);
            if (req == null) return ApiResult<YeuCauDto>.Fail("Yêu cầu không tồn tại.");

            var oldValues = JsonSerializer.Serialize(new
            {
                req.TrangThai,
                req.CanBoXuLy,
                req.GhiChuCanBo
            });

            req.TrangThai = form.TrangThai;
            req.GhiChuCanBo = form.GhiChuCanBo;
            req.CanBoXuLy = form.CanBoXuLy;
            req.NgayCapNhat = DateTime.Now;

            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(
                form.CanBoXuLy,
                "Cập nhật trạng thái hồ sơ",
                "YeuCauNguoiDan",
                req.Id.ToString(),
                oldValues,
                JsonSerializer.Serialize(new
                {
                    req.TrangThai,
                    req.CanBoXuLy,
                    req.GhiChuCanBo
                }));

            return ApiResult<YeuCauDto>.Ok(new YeuCauDto
            {
                Id = req.Id,
                MaYeuCau = req.MaYeuCau,
                HoTenNguoiYeuCau = req.HoTenNguoiYeuCau,
                CCCDNguoiYeuCau = req.CCCDNguoiYeuCau,
                SoDienThoai = req.SoDienThoai,
                LoaiYeuCau = req.LoaiYeuCau,
                NoiDung = req.NoiDung,
                TrangThai = req.TrangThai,
                CanBoXuLy = req.CanBoXuLy,
                GhiChuCanBo = req.GhiChuCanBo,
                NgayGui = req.NgayGui,
                NgayCapNhat = req.NgayCapNhat
            });
        }
    }
}


