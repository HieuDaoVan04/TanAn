using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Services.Core;

namespace Service.UI.CMS.Blazor.Applications;

/// <summary>Trạng thái giao diện theo circuit; mỗi lần lưu sẽ tải lại từ DB trong scope riêng.</summary>
public sealed class SystemConfigurationState(IServiceScopeFactory scopes, ModuleTypeState menuState,
    ILogger<SystemConfigurationState> logger) : IDisposable
{
    public PublicSystemConfigurationDto Value { get; private set; } = new()
    {
        AppName = SystemParameterCatalog.Find("AppName")!.DefaultValue,
        AppVersion = SystemParameterCatalog.Find("AppVersion")!.DefaultValue,
        FooterEnabled = true, FooterContent = SystemParameterCatalog.Find("FooterContent")!.DefaultValue
    };
    public event Action? Changed;
    private Task? initialLoad;
    public Task EnsureLoadedAsync()
    {
        if (initialLoad != null) return initialLoad;
        menuState.Changed += Reload;
        return initialLoad = LoadAsync();
    }
    private void Reload() => _ = LoadAsync();
    private async Task LoadAsync()
    {
        try
        {
            using var scope = scopes.CreateScope();
            Value = await scope.ServiceProvider.GetRequiredService<SystemConfigurationService>().GetPublicAsync();
            Changed?.Invoke();
        }
        catch (Exception ex) { logger.LogWarning(ex, "Không tải được thông tin hiển thị hệ thống."); }
    }
    public void Dispose() => menuState.Changed -= Reload;
}
