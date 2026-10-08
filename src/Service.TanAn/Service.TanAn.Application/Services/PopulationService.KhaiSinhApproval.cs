using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Application.Services;

public partial class PopulationService
{
    public async Task<ApiResult<PagedResult<KhaiSinhDraftDto>>> GetKhaiSinhApprovalNotificationsAsync(string username, int limit = 10)
    {
        var userId = await _db.Users.AsNoTracking().Where(x => x.UserName == username).Select(x => (Guid?)x.Id).SingleOrDefaultAsync();
        if (!userId.HasValue || !await BirthApprovalAuthorization.CanApproveAsync(_db, userId.Value))
            return ApiResult<PagedResult<KhaiSinhDraftDto>>.Fail(BirthApprovalAuthorization.DeniedMessage);
        return await GetKhaiSinhDraftsAsync(null, 1, limit, choDuyet: true);
    }

    public async Task<ApiResult<KhaiSinhDraftDto>> ApproveKhaiSinhAsync(Guid id, int phienBan, string username)
    {
        // Quyền được đọc lại từ DB mỗi lần; không tin cờ quyền hoặc vai trò do client gửi.
        var chairman = await _db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.UserName == username);
        if (chairman == null || !await BirthApprovalAuthorization.CanApproveAsync(_db, chairman.Id))
            return DraftFailure(BirthApprovalAuthorization.DeniedMessage);
        if (id == Guid.Empty || phienBan < 1) return DraftFailure("Mã hồ sơ hoặc phiên bản không hợp lệ.");
        var record = await _db.HoSoKhaiSinhs.SingleOrDefaultAsync(x => x.Id == id);
        if (record == null) return DraftFailure("Hồ sơ không tồn tại hoặc ngoài phạm vi quản lý.");
        var wasApproved = record.ModerationStatus == ModerationStatus.Approved;
        if (wasApproved && await _db.BienDongDanCus.AnyAsync(x => x.Id == id && x.LoaiBienDong == LoaiBienDongEnum.KhaiSinh))
            return ApiResult<KhaiSinhDraftDto>.Ok(DraftDto(record, true, true), "Hồ sơ đã được duyệt và ghi nhận khai sinh.");
        if (record.PhienBan != phienBan) return DraftFailure("Hồ sơ vừa được sửa. Tải lại và xem nội dung mới trước khi duyệt.");
        KhaiSinhForm? form;
        try { form = JsonSerializer.Deserialize<KhaiSinhForm>(record.NoiDungJson); }
        catch (JsonException) { return DraftFailure("Nội dung hồ sơ khai sinh không hợp lệ."); }
        if (form == null) return DraftFailure("Thiếu nội dung hồ sơ khai sinh.");
        form.RequestId = record.Id;
        form.HoGiaDinhId = record.HoGiaDinhId ?? Guid.Empty;
        var oldValues = JsonSerializer.Serialize(new { record.ModerationStatus, record.PhienBan });
        var context = (DbContext)_db;
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Chiếm phiên bản trước khi tạo nhân khẩu, tránh hai phiên duyệt cùng hồ sơ tạo trùng.
            if (!wasApproved)
            {
                record.ModerationStatus = ModerationStatus.Approved;
                record.NguoiDuyetId = chairman.Id;
                record.NguoiDuyet = chairman.UserName;
                record.NgayDuyet = DateTime.UtcNow;
            }
            // Cán bộ không nhập ngày đăng ký trên form nháp; ngày duyệt là ngày đăng ký mặc định.
            form.NgayDangKy ??= UtcDate((record.NgayDuyet ?? DateTime.UtcNow).ToLocalTime());
            record.PhienBan++;
            await _db.SaveChangesAsync();
            var birth = await CreateKhaiSinhAsync(form, username);
            if (!birth.Success || birth.Data == null)
            {
                await transaction.RollbackAsync();
                context.ChangeTracker.Clear();
                return DraftFailure(birth.Message);
            }
            record.NoiDungJson = JsonSerializer.Serialize(form);
            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(username, wasApproved ? "Ghi nhận khai sinh đã duyệt" : "Duyệt hồ sơ khai sinh", "HoSoKhaiSinh", record.Id.ToString(), oldValues,
                JsonSerializer.Serialize(new { record.ModerationStatus, record.PhienBan, record.NguoiDuyetId, record.NguoiDuyet, record.NgayDuyet, form.NgayDangKy }));
            await transaction.CommitAsync();
            return ApiResult<KhaiSinhDraftDto>.Ok(DraftDto(record, true, true), "Đã tạo nhân khẩu trong hộ và ghi nhận khai sinh.");
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            context.ChangeTracker.Clear();
            var saved = await _db.HoSoKhaiSinhs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
            return saved?.ModerationStatus == ModerationStatus.Approved && await _db.BienDongDanCus.AnyAsync(x => x.Id == id && x.LoaiBienDong == LoaiBienDongEnum.KhaiSinh)
                ? ApiResult<KhaiSinhDraftDto>.Ok(DraftDto(saved, true, true), "Hồ sơ đã được duyệt và ghi nhận khai sinh.")
                : DraftFailure("Hồ sơ vừa được sửa. Tải lại và xem nội dung mới trước khi duyệt.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(); context.ChangeTracker.Clear();
            return DraftFailure("Không ghi nhận được khai sinh do dữ liệu vừa thay đổi hoặc số định danh bị trùng. Hồ sơ chưa được cập nhật.");
        }
        catch { await transaction.RollbackAsync(); context.ChangeTracker.Clear(); throw; }
    }
}
