using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Application.Services;

public partial class PopulationService
{
    public async Task<ApiResult<HoGiaDinhDto>> GetHoGiaDinhByCodeAsync(string maSoHo)
    {
        if (string.IsNullOrWhiteSpace(maSoHo)) return ApiResult<HoGiaDinhDto>.Fail("Nhập mã hộ để tra cứu.");
        var id = await _db.HoGiaDinhs.Where(h => h.MaSoHo == maSoHo.Trim()).Select(h => (Guid?)h.Id).SingleOrDefaultAsync();
        return id.HasValue ? await GetHoGiaDinhByIdAsync(id.Value)
            : ApiResult<HoGiaDinhDto>.Fail("Không tìm thấy hộ hoặc hộ ngoài thôn được giao.");
    }

    public async Task<ApiResult<BienDongDto>> CreateKhaiSinhAsync(KhaiSinhForm form, string username)
    {
        TrimFields(form);
        if (form.Me != null) TrimFields(form.Me);
        if (form.Cha != null) TrimFields(form.Cha);
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), errors, true);
        if (form.Me != null) Validator.TryValidateObject(form.Me, new ValidationContext(form.Me), errors, true);
        if (form.Cha != null) Validator.TryValidateObject(form.Cha, new ValidationContext(form.Cha), errors, true);
        if (errors.Count > 0) return BirthFailure(string.Join(" ", errors.Select(x => x.ErrorMessage)));
        if (form.RequestId == Guid.Empty) return BirthFailure("Mã yêu cầu không hợp lệ. Hãy mở lại form khai sinh.");
        if (form.HoGiaDinhId == Guid.Empty) return BirthFailure("Tìm và chọn hộ gia đình cho trẻ trước khi lưu.");
        if (!Enum.IsDefined(form.GioiTinh)) return BirthFailure("Giới tính không hợp lệ.");
        if (form.QuanHeVoiChuHo == "Chủ hộ") return BirthFailure("Trẻ được khai sinh không thể được chọn làm chủ hộ.");
        if (form.NgaySinh!.Value.Date > DateTime.Today || form.NgaySinh.Value.Year < 1900)
            return BirthFailure("Ngày sinh của trẻ phải từ năm 1900 và không nằm trong tương lai.");
        if (form.NgayDangKy!.Value.Date < form.NgaySinh.Value.Date || form.NgayDangKy.Value.Date > DateTime.Today)
            return BirthFailure("Ngày đăng ký phải từ ngày sinh của trẻ đến hôm nay.");
        if (form.NgayCapGiayTo.HasValue && (form.NgayCapGiayTo.Value.Year < 1900 || form.NgayCapGiayTo.Value.Date > form.NgayDangKy.Value.Date))
            return BirthFailure("Ngày cấp giấy tờ phải hợp lệ và không sau ngày đăng ký.");
        foreach (var parent in new[] { form.Me, form.Cha }.OfType<KhaiSinhParentForm>())
            if (parent.NgaySinh.HasValue && (parent.NgaySinh.Value.Year < 1900 || parent.NgaySinh.Value.Date >= form.NgaySinh.Value.Date))
                return BirthFailure("Ngày sinh cha/mẹ phải hợp lệ và trước ngày sinh của trẻ.");
        if (form.Me?.NhanKhauId.HasValue == true && form.Me.NhanKhauId == form.Cha?.NhanKhauId)
            return BirthFailure("Cha và mẹ không thể là cùng một nhân khẩu.");

        var house = await _db.HoGiaDinhs.AsNoTracking().SingleOrDefaultAsync(h => h.Id == form.HoGiaDinhId);
        if (house == null) return BirthFailure("Hộ không tồn tại hoặc ngoài thôn được giao.");
        var selectedPeople = new[] { form.NguoiYeuCauId, form.Me?.NhanKhauId, form.Cha?.NhanKhauId }.OfType<Guid>().Distinct().ToArray();
        if (await _db.NhanKhaus.CountAsync(n => Enumerable.Contains(selectedPeople, n.Id) && n.MaHoGiaDinh == house.Id) != selectedPeople.Length)
            return BirthFailure("Người được chọn không còn thuộc hộ hoặc ngoài phạm vi quản lý. Hãy tra cứu lại hộ.");
        form.MaSoHo = house.MaSoHo;
        form.TenChuHo = house.TenChuHo;
        form.NgaySinh = UtcDate(form.NgaySinh.Value);
        form.NgayDangKy = UtcDate(form.NgayDangKy.Value);
        if (form.NgayCapGiayTo.HasValue) form.NgayCapGiayTo = UtcDate(form.NgayCapGiayTo.Value);
        if (form.Me?.NgaySinh.HasValue == true) form.Me.NgaySinh = UtcDate(form.Me.NgaySinh.Value);
        if (form.Cha?.NgaySinh.HasValue == true) form.Cha.NgaySinh = UtcDate(form.Cha.NgaySinh.Value);
        var snapshot = JsonSerializer.Serialize(form);
        var prior = await _db.BienDongDanCus.Include(b => b.NhanKhau).AsNoTracking().SingleOrDefaultAsync(b => b.Id == form.RequestId);
        if (prior != null) return ExistingBirth(prior, snapshot);
        if (form.SoDinhDanh.Length > 0 && await _db.NhanKhaus.AnyAsync(n => n.CCCD == form.SoDinhDanh))
            return BirthFailure("Số định danh đã tồn tại trong hệ thống.");

        var context = (DbContext)_db;
        // Khi duyệt hồ sơ, dùng giao dịch của thao tác duyệt để tất cả dữ liệu cùng commit/rollback.
        var ownsTransaction = context.Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction ? await context.Database.BeginTransactionAsync() : null;
        try
        {
            var personResult = await CreateNhanKhauAsync(new CreateNhanKhauForm
            {
                MaHoGiaDinh = house.Id, HoTen = form.HoTen, CCCD = form.SoDinhDanh,
                NgaySinh = form.NgaySinh.Value, GioiTinh = form.GioiTinh, DanToc = form.DanToc,
                QueQuan = form.QueQuan, ThuongTru = form.ThuongTru, QuanHeVoiChuHo = form.QuanHeVoiChuHo
            }, username);
            if (!personResult.Success || personResult.Data == null) return BirthFailure(personResult.Message);
            var membership = await _db.ThanhVienHos.SingleAsync(m => m.NhanKhauId == personResult.Data.Id && m.DenNgay == null);
            membership.TuNgay = DateOnly.FromDateTime(form.NgayDangKy.Value);
            var birth = new BienDongDanCu
            {
                Id = form.RequestId, NhanKhauId = personResult.Data.Id, LoaiBienDong = LoaiBienDongEnum.KhaiSinh,
                NgayPhatSinh = form.NgaySinh.Value, NoiDenOrDi = form.NoiSinh,
                LyDo = "Đăng ký khai sinh", CanBoGhiNhan = username, HoSoKhaiSinhJson = snapshot
            };
            _db.BienDongDanCus.Add(birth);
            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(username, "Đăng ký khai sinh", "BienDongDanCu", birth.Id.ToString(), null,
                JsonSerializer.Serialize(new { birth.NhanKhauId, HoGiaDinhId = house.Id, form.HoTen, form.NgaySinh, form.NgayDangKy }));
            if (transaction != null) await transaction.CommitAsync();
            return ApiResult<BienDongDto>.Ok(BirthDto(birth, form.HoTen, form.SoDinhDanh), "Đã tạo nhân khẩu và ghi nhận khai sinh.");
        }
        catch (DbUpdateException)
        {
            if (transaction == null) throw;
            await transaction.RollbackAsync();
            context.ChangeTracker.Clear();
            prior = await _db.BienDongDanCus.Include(b => b.NhanKhau).AsNoTracking().SingleOrDefaultAsync(b => b.Id == form.RequestId);
            return prior != null ? ExistingBirth(prior, snapshot) : BirthFailure("Dữ liệu vừa thay đổi hoặc số định danh bị trùng. Hãy kiểm tra lại trước khi lưu.");
        }
        catch
        {
            if (transaction != null) { await transaction.RollbackAsync(); context.ChangeTracker.Clear(); }
            throw;
        }
    }

    private static ApiResult<BienDongDto> BirthFailure(string message) => ApiResult<BienDongDto>.Fail(message);
    private static DateTime UtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    private static void TrimFields(object value)
    {
        foreach (var prop in value.GetType().GetProperties().Where(p => p.PropertyType == typeof(string) && p.CanWrite))
            prop.SetValue(value, (prop.GetValue(value) as string ?? "").Trim());
    }
    private static KhaiSinhForm? ReadBirthSnapshot(string json) => JsonSerializer.Deserialize<KhaiSinhForm>(json);
    private static BienDongDto BirthDto(BienDongDanCu birth, string name, string identity) => new()
    {
        Id = birth.Id, LoaiBienDong = birth.LoaiBienDong, NhanKhauId = birth.NhanKhauId,
        HoTenNhanKhau = name, CCCDNhanKhau = identity, NgayPhatSinh = birth.NgayPhatSinh,
        NoiDenOrDi = birth.NoiDenOrDi, LyDo = birth.LyDo, CanBoGhiNhan = birth.CanBoGhiNhan,
        NgayTao = birth.NgayTao, HoSoKhaiSinh = birth.HoSoKhaiSinhJson == null ? null : ReadBirthSnapshot(birth.HoSoKhaiSinhJson)
    };
    private static ApiResult<BienDongDto> ExistingBirth(BienDongDanCu prior, string snapshot)
        => prior.LoaiBienDong == LoaiBienDongEnum.KhaiSinh && prior.HoSoKhaiSinhJson == snapshot
            ? ApiResult<BienDongDto>.Ok(BirthDto(prior, prior.NhanKhau?.HoTen ?? "", prior.NhanKhau?.CCCD ?? ""), "Hồ sơ này đã được lưu.")
            : BirthFailure("Mã yêu cầu này đã được lưu với nội dung khác. Hãy kiểm tra hồ sơ đã ghi nhận.");
}
