using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Application.Services;

public partial class PopulationService
{
    public async Task<ApiResult<KhaiSinhDraftDto>> SaveKhaiSinhDraftAsync(KhaiSinhDraftSaveForm request, string username)
    {
        if (request?.HoSo == null || request.PhienBan < 0) return DraftFailure("Hồ sơ hoặc phiên bản không hợp lệ.");
        var form = request.HoSo;
        TrimFields(form);
        if (form.Me != null) TrimFields(form.Me);
        if (form.Cha != null) TrimFields(form.Cha);
        var errors = KhaiSinhDraftValidation.Errors(form);
        if (errors.Count > 0) return DraftFailure(string.Join(" ", errors));
        var officer = await _db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.UserName == username && x.ModerationStatus == ModerationStatus.Approved);
        if (officer == null || officer.LockoutEnd > DateTime.UtcNow) return DraftFailure("Phiên cán bộ không hợp lệ. Hãy đăng nhập lại.");
        if (await _db.BienDongDanCus.AnyAsync(x => x.Id == form.RequestId)) return DraftFailure("Mã này đã được ghi nhận thành biến động, không thể dùng làm hồ sơ nháp.");

        var villageId = request.ApThonId;
        if (form.HoGiaDinhId != Guid.Empty)
        {
            var house = await _db.HoGiaDinhs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == form.HoGiaDinhId);
            if (house == null) return DraftFailure("Hộ không tồn tại hoặc ngoài phạm vi quản lý. Hãy tra cứu lại.");
            villageId = house.ApThonId;
            form.MaSoHo = house.MaSoHo; form.TenChuHo = house.TenChuHo;
        }
        else { form.MaSoHo = ""; form.TenChuHo = ""; }
        if (villageId.HasValue && !await _db.ApThons.AnyAsync(x => x.Id == villageId && x.DangHoatDong))
            return DraftFailure("Thôn không tồn tại hoặc chưa được duyệt.");
        if (villageId.HasValue && officer.Role != RoleEnum.Admin && officer.Role != RoleEnum.CanBoXa && officer.Role != RoleEnum.ChuTichXa
            && !await _db.PhuTrachThons.AnyAsync(x => x.UserId == officer.Id && x.ApThonId == villageId))
            return DraftFailure("Bạn không được lưu hồ sơ ngoài thôn được giao.");
        var selectedPeople = new[] { form.NguoiYeuCauId, form.Me?.NhanKhauId, form.Cha?.NhanKhauId }.OfType<Guid>().Distinct().ToArray();
        if (selectedPeople.Length > 0 && (form.HoGiaDinhId == Guid.Empty || await _db.NhanKhaus.CountAsync(x => Enumerable.Contains(selectedPeople, x.Id) && x.MaHoGiaDinh == form.HoGiaDinhId) != selectedPeople.Length))
            return DraftFailure("Người được chọn không còn thuộc hộ hoặc ngoài phạm vi quản lý.");
        if (form.VaiTroNguoiYeuCau == VaiTroNguoiKhaiSinh.Cha) form.QuanHeVoiTre = "Cha";
        if (form.VaiTroNguoiYeuCau == VaiTroNguoiKhaiSinh.Me) form.QuanHeVoiTre = "Mẹ";
        if (form.TinhTrangMe != TinhTrangThongTinChaMe.CoThongTin) form.Me = null;
        if (form.TinhTrangCha != TinhTrangThongTinChaMe.CoThongTin) form.Cha = null;
        var json = JsonSerializer.Serialize(form);
        var context = (DbContext)_db;
        var record = await _db.HoSoKhaiSinhs.SingleOrDefaultAsync(x => x.Id == form.RequestId);
        if (record == null)
        {
            if (request.PhienBan != 0) return DraftFailure("Hồ sơ không tồn tại hoặc ngoài phạm vi quản lý.");
            // Không ghi đè một ID không đọc được qua bộ lọc quyền.
            if (await _db.HoSoKhaiSinhs.IgnoreQueryFilters().AnyAsync(x => x.Id == form.RequestId))
                return DraftFailure("Không thể lưu hồ sơ với mã này.");
            record = new HoSoKhaiSinh { Id = form.RequestId, MaHoSo = $"KS-{form.RequestId:N}", NguoiTaoId = officer.Id };
            _db.HoSoKhaiSinhs.Add(record);
        }
        else
        {
            if (record.ModerationStatus == ModerationStatus.Approved) return DraftFailure("Hồ sơ đã duyệt, không được chỉnh sửa.");
            if (record.NoiDungJson == json && record.ApThonId == villageId)
                return ApiResult<KhaiSinhDraftDto>.Ok(DraftDto(record, true), "Hồ sơ nháp đã được lưu.");
            if (request.PhienBan != record.PhienBan) return DraftFailure("Hồ sơ đã được sửa ở phiên khác. Đóng form và mở lại trước khi sửa tiếp.");
            record.PhienBan++; record.NgaySua = DateTime.UtcNow; record.NguoiSuaId = officer.Id;
        }
        record.HoGiaDinhId = form.HoGiaDinhId == Guid.Empty ? null : form.HoGiaDinhId;
        record.ApThonId = villageId; record.MaSoHo = form.MaSoHo;
        record.HoTenTre = form.HoTen; record.HoTenNguoiYeuCau = form.HoTenNguoiYeuCau;
        record.NgaySinh = form.NgaySinh.HasValue ? UtcDate(form.NgaySinh.Value) : null;
        var oldJson = context.Entry(record).State == EntityState.Added ? null : context.Entry(record).Property(x => x.NoiDungJson).OriginalValue;
        record.NoiDungJson = json;
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(username, oldJson == null ? "Tạo hồ sơ khai sinh nháp" : "Sửa hồ sơ khai sinh nháp", "HoSoKhaiSinh", record.Id.ToString(), oldJson, json);
            await transaction.CommitAsync();
            return ApiResult<KhaiSinhDraftDto>.Ok(DraftDto(record, true), "Đã lưu hồ sơ nháp; chưa tạo nhân khẩu hoặc biến động.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(); context.ChangeTracker.Clear();
            var saved = await _db.HoSoKhaiSinhs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == form.RequestId);
            return saved != null && saved.ModerationStatus != ModerationStatus.Approved && saved.NoiDungJson == json && saved.ApThonId == villageId
                ? ApiResult<KhaiSinhDraftDto>.Ok(DraftDto(saved, true), "Hồ sơ nháp đã được lưu.")
                : DraftFailure("Hồ sơ vừa thay đổi. Đóng form và mở lại để lấy dữ liệu mới.");
        }
        catch { await transaction.RollbackAsync(); context.ChangeTracker.Clear(); throw; }
    }

    public async Task<ApiResult<KhaiSinhDraftDto>> GetKhaiSinhDraftAsync(Guid id)
    {
        var record = await _db.HoSoKhaiSinhs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (record == null) return DraftFailure("Hồ sơ không tồn tại hoặc ngoài phạm vi quản lý.");
        var recorded = await _db.BienDongDanCus.AnyAsync(x => x.Id == record.Id && x.LoaiBienDong == LoaiBienDongEnum.KhaiSinh);
        return ApiResult<KhaiSinhDraftDto>.Ok(DraftDto(record, true, recorded));
    }

    public async Task<ApiResult<PagedResult<KhaiSinhDraftDto>>> GetKhaiSinhDraftsAsync(string? keyword, int pageIndex, int pageSize, Guid? apThonId = null, DateTime? tuNgay = null, DateTime? denNgay = null, bool choDuyet = false)
    {
        if (pageIndex < 1 || pageSize < 1 || pageSize > 100) return ApiResult<PagedResult<KhaiSinhDraftDto>>.Fail("Phân trang không hợp lệ (tối đa 100 hồ sơ mỗi trang).");
        if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date) return ApiResult<PagedResult<KhaiSinhDraftDto>>.Fail("Từ ngày không được sau đến ngày.");
        var query = _db.HoSoKhaiSinhs.AsNoTracking().AsQueryable();
        if (choDuyet) query = query.Where(x => x.ModerationStatus == ModerationStatus.Pending);
        if (!string.IsNullOrWhiteSpace(keyword)) { keyword = keyword.Trim(); query = query.Where(x => x.MaHoSo.Contains(keyword) || x.HoTenTre.Contains(keyword) || x.HoTenNguoiYeuCau.Contains(keyword) || x.MaSoHo.Contains(keyword)); }
        if (apThonId.HasValue) query = query.Where(x => x.ApThonId == apThonId);
        if (tuNgay.HasValue) { var start = UtcDate(tuNgay.Value); query = query.Where(x => x.NgayTao >= start); }
        if (denNgay.HasValue) { var end = UtcDate(denNgay.Value).AddDays(1); query = query.Where(x => x.NgayTao < end); }
        var total = await query.CountAsync();
        var rows = await query.OrderByDescending(x => x.NgayTao).ThenBy(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize)
            .Select(x => new KhaiSinhDraftDto { Id = x.Id, MaHoSo = x.MaHoSo, HoTenTre = x.HoTenTre, HoTenNguoiYeuCau = x.HoTenNguoiYeuCau,
                MaSoHo = x.MaSoHo, ApThonId = x.ApThonId, NgaySinh = x.NgaySinh, NgayTao = x.NgayTao, NgaySua = x.NgaySua, PhienBan = x.PhienBan,
                ModerationStatus = x.ModerationStatus, NguoiDuyet = x.NguoiDuyet, NgayDuyet = x.NgayDuyet,
                DaGhiNhan = _db.BienDongDanCus.Any(b => b.Id == x.Id && b.LoaiBienDong == LoaiBienDongEnum.KhaiSinh) }).ToListAsync();
        return ApiResult<PagedResult<KhaiSinhDraftDto>>.Ok(new(rows, total, pageIndex, pageSize));
    }

    private static ApiResult<KhaiSinhDraftDto> DraftFailure(string message) => ApiResult<KhaiSinhDraftDto>.Fail(message);
    private static KhaiSinhDraftDto DraftDto(HoSoKhaiSinh x, bool includeForm, bool recorded = false) => new()
    {
        Id = x.Id, MaHoSo = x.MaHoSo, HoTenTre = x.HoTenTre, HoTenNguoiYeuCau = x.HoTenNguoiYeuCau,
        MaSoHo = x.MaSoHo, ApThonId = x.ApThonId, NgaySinh = x.NgaySinh, NgayTao = x.NgayTao, NgaySua = x.NgaySua,
        PhienBan = x.PhienBan, ModerationStatus = x.ModerationStatus, DaGhiNhan = recorded, NguoiDuyet = x.NguoiDuyet, NgayDuyet = x.NgayDuyet,
        HoSo = includeForm ? JsonSerializer.Deserialize<KhaiSinhForm>(x.NoiDungJson) : null
    };
}
