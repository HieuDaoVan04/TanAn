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
using Service.UI.CMS.Blazor.Components.Layout.Component;

namespace Service.UI.CMS.Blazor.Components.Layout;

public partial class Header
{
    [Inject] private Service.UI.CMS.Blazor.Applications.SystemConfigurationState Configuration { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        Configuration.Changed += RefreshConfiguration;
        await Configuration.EnsureLoadedAsync();
    }
    private void RefreshConfiguration() => _ = InvokeAsync(StateHasChanged);
    public void Dispose() => Configuration.Changed -= RefreshConfiguration;
    [Inject] IDialogService DialogService { get; set; } = default!;
    [Inject] NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter]
    protected CurrentUserDto CurrentUser { set; get; } = new();

    private async Task OpenUserDialog(Guid Id)
    {
        try
        {
            var dialog = await DialogService.ShowDialogAsync<InfoUser>(Id, new DialogParameters()
            {
                Title = "Thông tin người dùng",
                PreventDismissOnOverlayClick = true,
                PreventScroll = true,
                Modal = true,
                PrimaryAction = null,
                SecondaryAction = null
            });
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync(ex.Message);
        }
    }

    private async Task LogoutAction()
    {
        try
        {
            NavigationManager.NavigateTo("/account/logout", true);
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync(ex.Message);
        }
    }
}
