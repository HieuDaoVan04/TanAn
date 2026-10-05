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
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using Service.UI.CMS.Blazor.Applications;
using Service.Shared.Commons.Extensions;

namespace Service.UI.CMS.Blazor.Components.Layout;

public partial class MainLayout
{
    private bool CanAccessPage => CurrentUser.IsAuthenticated && (CurrentUser.Role == "Admin"
        || Navigation.ToBaseRelativePath(Navigation.Uri).Split('?')[0].Trim('/') is "" or "ban-lam-viec"
        || CurrentUser.MenusActive.Any(m => m.Path.TrimEnd('/') == "/" + Navigation.ToBaseRelativePath(Navigation.Uri).Split('?')[0].TrimEnd('/')));
    protected override async Task OnParametersSetAsync()
        => CurrentUser = await UserService.GetCurrentUserAsync();
    private void RefreshUser() => _ = InvokeAsync(async () => { CurrentUser = await UserService.GetCurrentUserAsync(); StateHasChanged(); });
    private void RefreshConfiguration() => _ = InvokeAsync(StateHasChanged);
    public void Dispose()
    {
        MenuState.Changed -= RefreshUser;
        Configuration.Changed -= RefreshConfiguration;
    }
    private static bool _isFirstLoad = true;
    private bool IsLoading = _isFirstLoad;
    private int Progress = _isFirstLoad ? 0 : 100;
    private CurrentUserDto CurrentUser = new();

    protected override async Task OnInitializedAsync()
    {
        MenuState.Changed += RefreshUser;
        Configuration.Changed += RefreshConfiguration;
        await Configuration.EnsureLoadedAsync();
        if (!_isFirstLoad)
        {
            CurrentUser = await UserService.GetCurrentUserAsync();
            IsLoading = false;
            return;
        }

        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(20));
        var loadTask = UserService.GetCurrentUserAsync();

        while (Progress < 90 && await timer.WaitForNextTickAsync())
        {
            Progress += 30;
            StateHasChanged();
        }

        CurrentUser = await loadTask;
        Progress = 100;
        _isFirstLoad = false;
        IsLoading = false;
        StateHasChanged();
    }
}
