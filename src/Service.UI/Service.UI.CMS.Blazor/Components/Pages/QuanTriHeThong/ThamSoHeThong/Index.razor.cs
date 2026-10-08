using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Enums;
using Service.UI.CMS.Blazor.Applications;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using System.Text.Json;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.ThamSoHeThong;

public partial class Index
{
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [CascadingParameter] public CurrentUserDto CurrentUser { get; set; } = new();
    private bool viewOnly;
    private PaginationState pagination = new() { ItemsPerPage = 10 };
    private const string GridColumns = "40px minmax(160px,1fr) 130px minmax(160px,1fr) minmax(160px,1fr) 110px 155px";
    private const string NameColumnTitle = "Tham số hệ thống";
    private bool Match(AdminRecord record) => (status == "all" || record.Active == (status == "active"))
        && (string.IsNullOrWhiteSpace(search) || $"{record.Name} {record.Code} {record.Path} {record.Email} {record.Description}".Contains(search, StringComparison.OrdinalIgnoreCase));
    private IQueryable<AdminRecord> FilteredRecords => records.Where(Match).AsQueryable();
    private Task ResetPage() => pagination.SetCurrentPageIndexAsync(0);
    private async Task View(AdminRecord record) { await Edit(record); viewOnly = true; }

    private bool loading, busy;
    private string search = "", status = "all", error = "", message = "";
    private AdminRecord? form, pendingDelete;
    private List<AdminRecord> records = new();
    private const string Title = "Tham số hệ thống";
    protected override Task OnInitializedAsync() => Reload();
    private async Task Reload()
    {
        if (!CurrentUser.IsAuthenticated || CurrentUser.Role != "Admin") return;
        loading = true; error = "";
        try {
            records = (await CallService.Get<List<AdminRecord>>(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/administration/parameters" })).RequireData();

            await ResetPage();
        }
        catch (Exception ex) { error = ex.Message; records = new(); }
        finally { loading = false; }
    }
    private Task New() => Edit(new AdminRecord());
    private async Task Sync()
    {
        if (busy) return;
        busy = true; error = "";
        try
        {
            (await CallService.Post(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/system-configuration/sync" }, new { })).EnsureSuccess();
            message = "Đã bổ sung các tham số còn thiếu, giữ nguyên cấu hình đã có.";
            await Reload(); MenuState.SetState(Guid.NewGuid().ToString());
        }
        catch (Exception ex) { error = ex.Message; }
        finally { busy = false; }
    }
    private async Task Edit(AdminRecord record)
    {
        error = ""; viewOnly = false;
        try
        {

            form = JsonSerializer.Deserialize<AdminRecord>(JsonSerializer.Serialize(record))!;
        }
        catch (Exception ex) { error = ex.Message; }
    }
    private void Close() { if (busy) return; form = null; viewOnly = false; error = ""; }
    private async Task Save()
    {
        if (busy || viewOnly || form == null) return;
        busy = true; error = "";
        try { (await CallService.Post(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/administration/parameters" }, form)).EnsureSuccess(); form = null; message = "Đã lưu thay đổi."; await Reload(); MenuState.SetState(Guid.NewGuid().ToString()); }
        catch (Exception ex) { error = ex.Message; }
        finally { busy = false; }
    }
    private async Task ChangeStatus(AdminRecord record)
    {
        if (busy) return;
        busy = true;
        var copy = JsonSerializer.Deserialize<AdminRecord>(JsonSerializer.Serialize(record))!; copy.Active = !copy.Active;
        try { (await CallService.Post(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/administration/parameters" }, copy)).EnsureSuccess(); await Reload(); MenuState.SetState(Guid.NewGuid().ToString()); }
        catch (Exception ex) { error = ex.Message; }
        finally { busy = false; }
    }
    private async Task Delete()
    {
        if (busy || pendingDelete == null) return;
        busy = true;
        try { (await CallService.Delete(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = $"/administration/parameters/{pendingDelete.Id}" })).EnsureSuccess(); pendingDelete = null; message = "Đã xóa bản ghi."; await Reload(); MenuState.SetState(Guid.NewGuid().ToString()); }
        catch (Exception ex) { error = ex.Message; }
        finally { busy = false; }
    }
}
