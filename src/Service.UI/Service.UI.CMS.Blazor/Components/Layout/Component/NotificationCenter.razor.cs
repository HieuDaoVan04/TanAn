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
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Layout.Component;

public partial class NotificationCenter
{
    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] public IUserService Users { get; set; } = default!;
    private int villageUnread;
    private readonly CancellationTokenSource stopPolling = new();
    protected override async Task OnInitializedAsync() { await RefreshUnread(); _ = PollUnread(); }
    private async Task PollUnread()
    {
        try { using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            while(await timer.WaitForNextTickAsync(stopPolling.Token)) await InvokeAsync(async () => {await RefreshUnread();StateHasChanged();});
        } catch(OperationCanceledException) { }
    }
    private async Task RefreshUnread()
    {
        try {
            var user = await Users.GetCurrentUserAsync();
            if(!user.IsAuthenticated) {villageUnread=0;return;}
            using var scope=Scopes.CreateScope();
            villageUnread=await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(scope.ServiceProvider.GetRequiredService<ITanAnDbContext>().ThongBaoThons.Where(x=>x.DaDocLuc==null));
        } catch { villageUnread=0; }
    }
    private IDialogReference? _dialog;
   
    protected override void OnInitialized()
    {
        MessageService.OnMessageItemsUpdated += UpdateCount;    
    }

    private void UpdateCount()
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task OpenNotificationCenterAsync()
    {
        _dialog = await DialogService.ShowPanelAsync<NotificationCenterPanel>(new DialogParameters<GlobalState>()
            {
                Alignment = HorizontalAlignment.Right,
                Title = "Thông báo",
                PrimaryAction = null,
                SecondaryAction = null,
                ShowDismiss = true
            });
        DialogResult result = await _dialog.Result;
        await RefreshUnread();
    }

    public void Dispose()
    {
        stopPolling.Cancel();
        MessageService.OnMessageItemsUpdated -= UpdateCount;
    }
}
