using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Service.UI.CMS.Blazor.Components.Layout.Component;

public partial class SiteSettingsPanel
{
    [Parameter] public SiteThemePreferences Content { get; set; } = default!;
    private string? _status;
    private static string ModeLabel(DesignThemeModes mode) => mode switch
    {
        DesignThemeModes.Light => "Sáng",
        DesignThemeModes.Dark => "Tối",
        _ => "Theo hệ thống"
    };
    private Task ApplyAsync() => Content.ApplyAsync();
    private Task HandleDirectionChanged(bool leftToRight)
    {
        Content.Direction = leftToRight ? LocalizationDirection.LeftToRight : LocalizationDirection.RightToLeft;
        return ApplyAsync();
    }
    private async Task ResetSiteAsync()
    {
        try
        {
            await Content.ResetAsync();
            _status = "Đã đặt lại cài đặt giao diện.";
        }
        catch (Microsoft.JSInterop.JSException)
        {
            _status = "Không thể đặt lại dữ liệu trình duyệt. Vui lòng thử lại.";
        }
    }
}
