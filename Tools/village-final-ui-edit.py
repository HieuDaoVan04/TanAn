from pathlib import Path
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/QuanTriHeThong/DanhMuc/Index.razor');s=p.read_text(encoding='utf-8').replace('<label><input type="checkbox" checked="@form.VillageIds','<label class="village-assignment"><input type="checkbox" checked="@form.VillageIds');p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/QuanTriHeThong/DanhMuc/Index.razor.css');s=p.read_text(encoding='utf-8')+'\n.editor ::deep label.village-assignment { flex-direction:row; align-items:center; gap:10px; margin:8px 0; }\n.editor ::deep .village-assignment input[type=checkbox] { width:16px; height:16px; flex:0 0 16px; margin:0; }\n';p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/QuanTriHeThong/DanhMuc/AdminRowActions.razor');s=p.read_text(encoding='utf-8').replace('!Item.Active && Kind is (AdminCatalog.Users or AdminCatalog.Roles)','Kind == AdminCatalog.Users || (!Item.Active && Kind == AdminCatalog.Roles)');p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Layout/Component/NotificationCenter.razor.cs');s=p.read_text(encoding='utf-8-sig').replace('    private IDialogReference? _dialog;','''    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
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
    private IDialogReference? _dialog;''').replace('DialogResult result = await _dialog.Result;','DialogResult result = await _dialog.Result;\n        await RefreshUnread();').replace('        MessageService.OnMessageItemsUpdated -= UpdateCount;','        stopPolling.Cancel();\n        MessageService.OnMessageItemsUpdated -= UpdateCount;');p.write_text(s,encoding='utf-8')
p=p.with_suffix('');s=p.read_text(encoding='utf-8-sig').replace('MessageService.Count(AppRoutes.MESSAGES_NOTIFICATION_CENTER)','(villageUnread + MessageService.Count(AppRoutes.MESSAGES_NOTIFICATION_CENTER))');p.write_text(s,encoding='utf-8')
