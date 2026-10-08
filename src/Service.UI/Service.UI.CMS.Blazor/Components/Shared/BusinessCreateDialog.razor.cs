using Microsoft.AspNetCore.Components;
using Service.Shared.Contracts.DTOs;
using Service.Shared.Commons.Models;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Enums;
using Service.UI.CMS.Blazor.Applications;
namespace Service.UI.CMS.Blazor.Components.Shared;
public partial class BusinessCreateDialog
{
    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] public IUserService Users { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Parameter] public string Kind { get; set; } = "household";
    [Parameter] public DoiTuongAnSinhEnum? Category { get; set; }
    [Parameter] public LoaiBienDongEnum? ChangeType { get; set; }
    [Parameter] public Guid TargetId { get; set; }
    [Parameter] public string TargetName { get; set; } = "";
    [Parameter] public decimal Amount { get; set; }
    [Parameter] public EventCallback Saved { get; set; }
    [Parameter] public EventCallback Cancel { get; set; }
    private bool busy;
    private string error = "", keyword = "";
    private Guid personId;
    private List<NhanKhauDto> people = new();
    private CreateHoGiaDinhForm household = new();
    private CreateBienDongForm change = new() { LoaiBienDong = LoaiBienDongEnum.TamTru, NgayPhatSinh = DateTime.Today };
    private CreateAnSinhForm welfare = new() { LoaiDoiTuong = DoiTuongAnSinhEnum.HoNgheo, NgayBatDauHuong = DateTime.Today };
    private CreateTroCapForm payout = new();
    private object Model => Kind switch { "household" => household, "change" => change, "payout" => payout, _ => welfare };
    private string Title => Kind switch { "household" => "Thêm mới hộ gia đình", "change" => ChangeType.HasValue ? $"Đăng ký {Label(ChangeType.Value.ToString()).ToLowerInvariant()}" : "Đăng ký biến động dân cư", "payout" => "Ghi nhận chi trả trợ cấp", _ => "Thêm đối tượng an sinh" };
    private List<string> villageNames = new();
    protected override async Task OnInitializedAsync()
    {
        var user = await Users.GetCurrentUserAsync();
        using var scope = Scopes.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ITanAnDbContext>().ApThons.Where(x=>x.DangHoatDong);
        if(user.Role == "CanBoThon") query = query.Where(x=>user.VillageIds.Contains(x.Id));
        villageNames = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query.OrderBy(x=>x.Ten).Select(x=>x.Ten));
        if(villageNames.Count==1) household.ApThon = villageNames[0];
        welfare.LoaiDoiTuong = Category ?? DoiTuongAnSinhEnum.HoNgheo;
        if (ChangeType.HasValue) change.LoaiBienDong = ChangeType.Value;
        payout = new() { DoiTuongAnSinhId = TargetId, SoTien = Amount, ThangNam = DateTime.Today.ToString("MM/yyyy") };
    }
    private async Task FindPeople()
    {
        if (busy) return;
        busy = true; error = "";
        try
        {
            using var scope = Scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IPopulationService>().GetNhanKhausAsync(keyword, null, 1, 100);
            if (!result.Success) { error = result.Message; return; }
            people = result.Data?.Items.ToList() ?? new();
            if (people.Count == 0) error = "Không tìm thấy nhân khẩu. Hãy nhập từ khóa khác.";
        }
        catch { error = "Không tải được nhân khẩu. Vui lòng thử lại."; }
        finally { busy = false; }
    }
    private async Task Save()
    {
        if (busy) return;
        busy = true; error = "";
        try
        {
            var user = await Users.GetCurrentUserAsync();
            var path = "/" + Navigation.ToBaseRelativePath(Navigation.Uri).Split('?')[0].Trim('/');
            if (!user.IsAuthenticated || (user.Role != "Admin" && !user.MenusActive.Any(m => m.Path.TrimEnd('/') == path)))
                throw new InvalidOperationException("Bạn không còn quyền thao tác trên trang này. Hãy đăng nhập lại.");
            using var scope = Scopes.CreateScope();
            var population = scope.ServiceProvider.GetRequiredService<IPopulationService>();
            var benefits = scope.ServiceProvider.GetRequiredService<IWelfareService>();
            if (Kind == "household")
            {
                if (new[] { household.MaSoHo, household.TenChuHo, household.CCCDChuHo, household.DiaChi, household.ApThon }.Any(string.IsNullOrWhiteSpace)) throw new InvalidOperationException("Vui lòng nhập đủ các trường bắt buộc.");
                Ensure(await population.CreateHoGiaDinhAsync(household, user.UserName));
            }
            else if (Kind == "payout")
            {
                if (payout.SoTien <= 0 || !DateTime.TryParseExact(payout.ThangNam, "MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _)) throw new InvalidOperationException("Nhập số tiền dương và tháng theo định dạng MM/yyyy.");
                Ensure(await benefits.AddLichSuTroCapAsync(payout, user.UserName));
            }
            else
            {
                if (personId == Guid.Empty) throw new InvalidOperationException("Vui lòng tìm và chọn nhân khẩu.");
                if (Kind == "change")
                {
                    if (!ChangeType.HasValue || !Enum.IsDefined(ChangeType.Value)) throw new InvalidOperationException("Hãy chọn menu của loại biến động để thêm mới.");
                    change.LoaiBienDong = ChangeType.Value;
                    if (string.IsNullOrWhiteSpace(change.LyDo)) throw new InvalidOperationException("Vui lòng nhập lý do biến động.");
                    change.NhanKhauId = personId;
                    Ensure(await population.CreateBienDongAsync(change, user.UserName));
                }
                else
                {
                    if (welfare.MucTroCapHangThang < 0) throw new InvalidOperationException("Mức trợ cấp không được âm.");
                    welfare.NhanKhauId = personId;
                    Ensure(await benefits.CreateDoiTuongAnSinhAsync(welfare, user.UserName));
                }
            }
            await Saved.InvokeAsync();
        }
        catch (InvalidOperationException ex) { error = ex.Message; }
        catch { error = "Không lưu được dữ liệu. Hãy tải lại danh sách để kiểm tra trước khi thử lại."; }
        finally { busy = false; }
    }
    private static void Ensure<T>(ApiResult<T> result) { if (!result.Success) throw new InvalidOperationException(result.Message); }
    public static string Label(string value) => value switch
    {
        "KhaiSinh" => "Khai sinh", "KhaiTu" => "Khai tử", "TamTru" => "Tạm trú", "TamVang" => "Tạm vắng", "ChuyenDen" => "Chuyển đến", "ChuyenDi" => "Chuyển đi",
        "HoNgheo" => "Hộ nghèo", "HoCanNgheo" => "Hộ cận nghèo", "NguoiCaoTuoi" => "Người cao tuổi", "ThuongBinh" => "Thương binh", "BenhBinh" => "Bệnh binh", "ThanNhanLietSi" => "Thân nhân liệt sĩ", "MeVietNamAnhHung" => "Mẹ Việt Nam anh hùng", "BaoCongAnSinhKhac" => "Bảo trợ khác", _ => value
    };
}
