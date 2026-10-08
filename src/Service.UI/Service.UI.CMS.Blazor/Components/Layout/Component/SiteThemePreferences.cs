using Microsoft.FluentUI.AspNetCore.Components;

namespace Service.UI.CMS.Blazor.Components.Layout.Component;

public sealed class SiteThemePreferences
{
    public DesignThemeModes Mode { get; set; } = DesignThemeModes.System;
    public OfficeColor? OfficeColor { get; set; }
    public LocalizationDirection? Direction { get; set; } = LocalizationDirection.LeftToRight;
    public Func<Task> ApplyAsync { get; set; } = () => Task.CompletedTask;
    public Func<Task> ResetAsync { get; set; } = () => Task.CompletedTask;
}
