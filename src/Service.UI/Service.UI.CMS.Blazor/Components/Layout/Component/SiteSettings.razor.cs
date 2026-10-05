using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Orientation = Microsoft.FluentUI.AspNetCore.Components.Orientation;
using Align = Microsoft.FluentUI.AspNetCore.Components.Align;
using Color = Microsoft.FluentUI.AspNetCore.Components.Color;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;
using ApexCharts;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor;
using Service.UI.CMS.Blazor.Components;
using Service.UI.CMS.Blazor.Components.Shared;
using static Microsoft.AspNetCore.Components.Web.RenderMode;
using Service.UI.CMS.Blazor.Components.Layout;
using Service.UI.CMS.Blazor.Components.Shares.Cards;
using Service.UI.CMS.Blazor.Components.Shares.Filter;

namespace Service.UI.CMS.Blazor.Components.Layout.Component;

public partial class SiteSettings
{
    private FluentDesignTheme? _theme;
    private readonly SiteThemePreferences _preferences = new();
    private bool _panelOpen;

    private async Task OpenSiteSettingsAsync()
    {
        if (_panelOpen) return;
        _panelOpen = true;
        _preferences.ApplyAsync = () => InvokeAsync(StateHasChanged);
        _preferences.ResetAsync = async () =>
        {
            if (_theme != null) await _theme.ClearLocalStorageAsync();
            _preferences.Mode = DesignThemeModes.System;
            _preferences.OfficeColor = OfficeColorUtilities.GetRandom();
            await InvokeAsync(StateHasChanged);
        };
        try
        {
            var dialog = await DialogService.ShowPanelAsync<SiteSettingsPanel>(_preferences, new DialogParameters
            {
                ShowTitle = true, Title = "Cài đặt giao diện",
                Alignment = HorizontalAlignment.Right, PrimaryAction = "Đóng",
                SecondaryAction = null, ShowDismiss = true
            });
            await dialog.Result;
        }
        finally { _panelOpen = false; }
    }
}
