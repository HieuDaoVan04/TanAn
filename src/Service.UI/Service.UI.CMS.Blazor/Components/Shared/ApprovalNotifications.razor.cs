using Microsoft.AspNetCore.Components;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Shared;

public partial class ApprovalNotifications
{
    [Inject] private IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] private IUserService Users { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Parameter] public EventCallback Opening { get; set; }
    private bool visible, busy;
    private int pendingCount;
    private string error = "";
    private List<KhaiSinhDraftDto> pending = [];

    protected override Task OnInitializedAsync() => Load();

    private async Task Load()
    {
        if (busy) return;
        busy = true;
        error = "";
        pending.Clear();
        pendingCount = 0;
        visible = false;
        try
        {
            var user = await Users.GetCurrentUserAsync();
            if (!HasAccess(user)) return;
            using var scope = Scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IPopulationService>()
                .GetKhaiSinhApprovalNotificationsAsync(user.UserName);
            if (!result.Success || result.Data == null) return;
            visible = true;
            pending = result.Data.Items;
            pendingCount = result.Data.TotalCount;
        }
        catch { visible = true; error = "Không tải được việc cần duyệt. Vui lòng làm mới."; }
        finally { busy = false; }
    }

    private static bool HasAccess(CurrentUserDto user) => user.IsAuthenticated
        && user.RoleCodes.Any(code => string.Equals(code.Trim(), Service.Shared.Commons.Enums.RoleCodes.ChuTichXa, StringComparison.OrdinalIgnoreCase))
        && user.MenusActive.Any(menu => menu.Path.TrimEnd('/') == "/bien-dong/khai-sinh");

    private async Task Open(Guid? id)
    {
        await Load();
        if (!visible || !string.IsNullOrEmpty(error)) return;
        await Opening.InvokeAsync();
        Navigation.NavigateTo("/bien-dong/khai-sinh" + (id.HasValue ? $"?hoSoId={id.Value:D}" : ""));
    }
}
