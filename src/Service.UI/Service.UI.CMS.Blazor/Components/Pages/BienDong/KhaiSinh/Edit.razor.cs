using Service.Shared.Commons.Model.SQL;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Domain.Entities;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Enums;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong.KhaiSinh;

public partial class Edit
{
    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] public IUserService Users { get; set; } = default!;
    [Parameter] public EventCallback Saved { get; set; }
    [Parameter] public EventCallback Cancel { get; set; }
    [Parameter] public Guid? DraftId { get; set; }
    private KhaiSinhForm form = new() { NgayDangKy = null, DanToc = "", QuocTich = "", TinhTrangMe = TinhTrangThongTinChaMe.CoThongTin, TinhTrangCha = TinhTrangThongTinChaMe.CoThongTin };
    private KhaiSinhParentForm mother = new() { DanToc = "", QuocTich = "" }, father = new() { DanToc = "", QuocTich = "" };
    private bool hasMother = true, hasFather = true, busy;
    private string error = "", houseKeyword = "";
    private Guid chosenHouseId;
    private Guid? selectedVillageId;
    private int draftVersion;
    private string draftCode = "";
    private bool loadFailed;
    private List<ApThon> villages = [];
    private HoGiaDinhDto? selectedHouse;
    private List<HoGiaDinhDto> houseOptions = [];
    private IReadOnlyList<NhanKhauDto> Members => selectedHouse?.ThanhVien ?? [];
    private static string Today => DateTime.Today.ToString("yyyy-MM-dd");
    internal static string GenderLabel(GioiTinhEnum gender) => gender switch { GioiTinhEnum.Nam => "Nam", GioiTinhEnum.Nu => "Nữ", _ => "Khác" };

    protected override async Task OnInitializedAsync()
    {
        busy = true;
        try
        {
            var user = await Users.GetCurrentUserAsync();
            using var scope = Scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ITanAnDbContext>();
            var query = db.ApThons.AsNoTracking().Where(x => x.DangHoatDong);
            if (user.Role != "Admin" && user.Role != "CanBoXa" && user.Role != "ChuTichXa") query = query.Where(x => user.VillageIds.Contains(x.Id));
            villages = await query.OrderBy(x => x.Ten).ToListAsync();
            if (DraftId.HasValue)
            {
                var population = scope.ServiceProvider.GetRequiredService<IPopulationService>();
                var result = await population.GetKhaiSinhDraftAsync(DraftId.Value);
                if (!result.Success || result.Data?.HoSo == null) { error = result.Message; loadFailed = true; return; }
                if ((result.Data.ModerationStatus == ModerationStatus.Approved)) { error = "Hồ sơ đã duyệt, không được chỉnh sửa. Mở Xem để đọc nội dung."; loadFailed = true; return; }
                form = result.Data.HoSo; draftVersion = result.Data.PhienBan; draftCode = result.Data.MaHoSo;
                selectedVillageId = result.Data.ApThonId;
                mother = form.Me ?? new(); father = form.Cha ?? new();
                hasMother = form.TinhTrangMe == TinhTrangThongTinChaMe.CoThongTin;
                hasFather = form.TinhTrangCha == TinhTrangThongTinChaMe.CoThongTin;
                if (form.HoGiaDinhId != Guid.Empty)
                {
                    var house = await population.GetHoGiaDinhByIdAsync(form.HoGiaDinhId);
                    if (house.Success && house.Data != null) { selectedHouse = house.Data; chosenHouseId = house.Data.Id; houseKeyword = house.Data.MaSoHo; houseOptions = [house.Data]; }
                    else error = "Hộ liên kết không còn đọc được. Bạn có thể chuyển sang tự nhập để bỏ liên kết hộ.";
                }
            }
        }
        catch { error = "Không tải được hồ sơ hoặc danh mục thôn. Đóng form và mở lại."; loadFailed = true; }
        finally { busy = false; }
    }

    private void ChangeEntryMode()
    {
        if (form.KieuNhap != KieuNhapKhaiSinh.TuNhap) return;
        selectedHouse = null; chosenHouseId = Guid.Empty; houseOptions = []; houseKeyword = "";
        form.HoGiaDinhId = Guid.Empty; form.MaSoHo = ""; form.TenChuHo = "";
        form.NguoiYeuCauId = null; mother.NhanKhauId = null; father.NhanKhauId = null;
    }
    private void ChangeMotherState() => hasMother = form.TinhTrangMe == TinhTrangThongTinChaMe.CoThongTin;
    private void ChangeFatherState() => hasFather = form.TinhTrangCha == TinhTrangThongTinChaMe.CoThongTin;
    private void ChangeApplicantRole()
    {
        if (form.VaiTroNguoiYeuCau == VaiTroNguoiKhaiSinh.Cha) form.QuanHeVoiTre = "Cha";
        else if (form.VaiTroNguoiYeuCau == VaiTroNguoiKhaiSinh.Me) form.QuanHeVoiTre = "Mẹ";
        else form.QuanHeVoiTre = "";
    }

    private async Task FindHousehold()
    {
        if (busy) return;
        busy = true; error = "";
        ResetHousehold();
        houseOptions = [];
        try
        {
            if (string.IsNullOrWhiteSpace(houseKeyword)) { error = "Nhập mã hộ hoặc tên chủ hộ để tìm."; return; }
            using var scope = Scopes.CreateScope();
            var population = scope.ServiceProvider.GetRequiredService<IPopulationService>();
            var exact = await population.GetHoGiaDinhByCodeAsync(houseKeyword);
            if (exact.Success && exact.Data != null)
            {
                houseOptions = [exact.Data];
                ApplyHousehold(exact.Data);
                return;
            }
            var matches = await population.GetHoGiaDinhsAsync(houseKeyword.Trim(), null, 1, 25);
            if (!matches.Success) { error = matches.Message; return; }
            houseOptions = matches.Data?.Items.ToList() ?? [];
            if (houseOptions.Count == 0) error = "Không tìm thấy hộ trong phạm vi quản lý. Kiểm tra mã hộ hoặc tìm theo tên chủ hộ.";
        }
        catch { error = "Không tra cứu được hộ. Vui lòng thử lại."; }
        finally { busy = false; }
    }

    private async Task ChooseHousehold()
    {
        if (busy) return;
        var id = chosenHouseId;
        ResetHousehold();
        if (id == Guid.Empty) return;
        busy = true; error = "";
        try
        {
            using var scope = Scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IPopulationService>().GetHoGiaDinhByIdAsync(id);
            if (result.Success && result.Data != null) ApplyHousehold(result.Data);
            else error = result.Message;
        }
        catch { error = "Không tải được hộ. Vui lòng tra cứu lại."; }
        finally { busy = false; }
    }

    private void ResetHousehold()
    {
        var hadLinkedHouse = selectedHouse != null || form.HoGiaDinhId != Guid.Empty;
        selectedHouse = null; chosenHouseId = Guid.Empty;
        form.HoGiaDinhId = Guid.Empty; form.MaSoHo = ""; form.TenChuHo = "";
        if (!hadLinkedHouse) return;
        form.ThuongTru = "";
        form.QuanHeVoiChuHo = "";
        form.NguoiYeuCauId = null; form.HoTenNguoiYeuCau = ""; form.SoGiayTo = "";
        form.NgayCapGiayTo = null; form.NoiCapGiayTo = ""; form.NoiCuTruNguoiYeuCau = "";
        form.QuanHeVoiTre = "";
        mother = new() { DanToc = "", QuocTich = "" }; father = new() { DanToc = "", QuocTich = "" };
    }

    private void ApplyHousehold(HoGiaDinhDto house)
    {
        selectedHouse = house; chosenHouseId = house.Id;
        form.HoGiaDinhId = house.Id; form.MaSoHo = house.MaSoHo; form.TenChuHo = house.TenChuHo;
        form.ThuongTru = house.DiaChi;
        form.NoiCuTruNguoiYeuCau = house.DiaChi;
        mother.NoiCuTru = house.DiaChi; father.NoiCuTru = house.DiaChi;
    }

    private void FillApplicant()
    {
        var person = Members.SingleOrDefault(x => x.Id == form.NguoiYeuCauId);
        form.HoTenNguoiYeuCau = person?.HoTen ?? "";
        form.SoGiayTo = person?.CCCD ?? "";
        form.LoaiGiayTo = "CCCD";
        form.QuanHeVoiTre = "";
        form.NgayCapGiayTo = null; form.NoiCapGiayTo = "";
        form.NoiCuTruNguoiYeuCau = person == null ? selectedHouse?.DiaChi ?? "" : !string.IsNullOrWhiteSpace(person.ThuongTru) ? person.ThuongTru : selectedHouse?.DiaChi ?? "";
    }

    private async Task Save(EditContext editContext)
    {
        if (busy || loadFailed) return;
        error = "";
        form.Me = hasMother ? mother : null;
        form.Cha = hasFather ? father : null;
        var errors = KhaiSinhDraftValidation.Errors(form);
        if (editContext.GetValidationMessages().Any()) { error = "Kiểm tra định dạng các ô đang nhập."; return; }
        if (errors.Count > 0) { error = string.Join(" ", errors); return; }
        busy = true;
        try
        {
            var user = await Users.GetCurrentUserAsync();
            if (!user.IsAuthenticated || (user.Role != "Admin" && !user.MenusActive.Any(m => m.Path.TrimEnd('/') == "/bien-dong/khai-sinh")))
            { error = "Bạn không còn quyền đăng ký khai sinh. Hãy đăng nhập lại."; return; }
            using var scope = Scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IPopulationService>().SaveKhaiSinhDraftAsync(new() { HoSo = form, ApThonId = selectedVillageId, PhienBan = draftVersion }, user.UserName);
            if (!result.Success) { error = result.Message; return; }
            draftVersion = result.Data!.PhienBan;
            await Saved.InvokeAsync();
        }
        catch (UnauthorizedAccessException) { error = "Hồ sơ hoặc hộ không thuộc phạm vi quản lý của bạn."; }
        catch { error = "Không lưu được hồ sơ nháp. Hãy thử lại trên form này."; }
        finally { busy = false; }
    }
}
