using Microsoft.AspNetCore.Components;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Enums;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong.TamVang;

public partial class Edit
{
    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] public IUserService Users { get; set; } = default!;
    [Parameter] public EventCallback Saved { get; set; }
    [Parameter] public EventCallback Cancel { get; set; }
    private const LoaiBienDongEnum ScreenType = LoaiBienDongEnum.TamVang;
    private const string Title = "Đăng ký tạm vắng";
    private bool busy;
    private string error = "", keyword = "";
    private Guid personId;
    private List<NhanKhauDto> people = new();
    private readonly CreateBienDongForm change = new() { LoaiBienDong = ScreenType, NgayPhatSinh = DateTime.Today };

    private async Task FindPeople()
    {
        if (busy) return;
        busy = true;
        error = "";
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
        busy = true;
        error = "";
        try
        {
            var user = await Users.GetCurrentUserAsync();
            if (!user.IsAuthenticated || (user.Role != "Admin" && !user.MenusActive.Any(m => m.Path.TrimEnd('/') == "/bien-dong/tam-vang")))
                throw new InvalidOperationException("Bạn không còn quyền thao tác trên trang này. Hãy đăng nhập lại.");
            if (personId == Guid.Empty) throw new InvalidOperationException("Vui lòng tìm và chọn nhân khẩu.");
            if (string.IsNullOrWhiteSpace(change.LyDo)) throw new InvalidOperationException("Vui lòng nhập lý do biến động.");
            change.LoaiBienDong = ScreenType;
            change.NhanKhauId = personId;
            using var scope = Scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IPopulationService>().CreateBienDongAsync(change, user.UserName);
            if (!result.Success) throw new InvalidOperationException(result.Message);
            await Saved.InvokeAsync();
        }
        catch (InvalidOperationException ex) { error = ex.Message; }
        catch { error = "Không lưu được dữ liệu. Hãy tải lại danh sách để kiểm tra trước khi thử lại."; }
        finally { busy = false; }
    }
}
