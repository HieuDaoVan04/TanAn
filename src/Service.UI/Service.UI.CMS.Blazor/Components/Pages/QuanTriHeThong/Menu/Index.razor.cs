using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Enums;
using Service.UI.CMS.Blazor.Applications;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using System.Text.Json;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.Menu;

public partial class Index
{
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [CascadingParameter] public CurrentUserDto CurrentUser { get; set; } = new();
    private bool viewOnly;
    private Guid? selectedId;
    private PaginationState pagination = new() { ItemsPerPage = 10 };
    private const string GridColumns = "";
    private const string NameColumnTitle = "Menu";
    private bool Match(AdminRecord record) => (status == "all" || record.Active == (status == "active"))
        && (string.IsNullOrWhiteSpace(search) || $"{record.Name} {record.Code} {record.Path} {record.Email} {record.Description}".Contains(search, StringComparison.OrdinalIgnoreCase));
    private IQueryable<AdminRecord> FilteredRecords => records.Where(Match).AsQueryable();
    private Task ResetPage() => pagination.SetCurrentPageIndexAsync(0);
    private async Task View(AdminRecord record) { await Edit(record); viewOnly = true; }

    private bool loading, busy;
    private string search = "", status = "all", error = "", message = "";
    private AdminRecord? form, pendingDelete;
    private List<AdminRecord> records = new(), modules = new();
    private HashSet<Guid> expanded = new();
    private const string Title = "Quản lý menu";
    protected override Task OnInitializedAsync() => Reload();
    private async Task Reload()
    {
        loading = true; error = "";
        try {
            records = (await CallService.Get<List<AdminRecord>>(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/administration/menus" })).RequireData();

            await ResetPage();
        }
        catch (Exception ex) { error = ex.Message; records = new(); }
        finally { loading = false; }
    }
    private IEnumerable<(AdminRecord Item, int Depth)> VisibleRows
    {
        get
        {
            bool Match(AdminRecord r) => (status == "all" || r.Active == (status == "active")) && (string.IsNullOrWhiteSpace(search) || $"{r.Name} {r.Code} {r.Path}".Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(search) || status != "all") return records.Where(Match).Select(x => (x, 0));
            var result = new List<(AdminRecord, int)>();
            var seen = new HashSet<Guid>();
            void Walk(Guid? parent, int depth)
            {
                foreach (var r in records.Where(x => x.ParentId == parent).OrderBy(x => x.Order))
                    if (seen.Add(r.Id)) { result.Add((r, depth)); if (expanded.Contains(r.Id)) Walk(r.Id, depth + 1); }
            }
            Walk(null, 0);
            return result;
        }
    }
    private void Toggle(Guid id) { if (!expanded.Add(id)) expanded.Remove(id); }
    private void ExpandAll() => expanded = records.Select(x => x.Id).ToHashSet();
    private void CollapseAll() => expanded.Clear();
    private Task New() => Edit(new AdminRecord());
    private async Task Edit(AdminRecord record)
    {
        error = ""; viewOnly = false;
        try
        {
            modules = (await CallService.Get<List<AdminRecord>>(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/administration/modules" })).RequireData();

            form = JsonSerializer.Deserialize<AdminRecord>(JsonSerializer.Serialize(record))!;
        }
        catch (Exception ex) { error = ex.Message; }
    }
    private void Close() { if (busy) return; form = null; viewOnly = false; error = ""; }
    private async Task Save()
    {
        if (busy || viewOnly || form == null) return;
        busy = true; error = "";
        try { (await CallService.Post(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/administration/menus" }, form)).EnsureSuccess(); form = null; message = "Đã lưu thay đổi."; await Reload(); MenuState.SetState(Guid.NewGuid().ToString()); }
        catch (Exception ex) { error = ex.Message; }
        finally { busy = false; }
    }
    private async Task ChangeStatus(AdminRecord record)
    {
        if (busy) return;
        busy = true;
        var copy = JsonSerializer.Deserialize<AdminRecord>(JsonSerializer.Serialize(record))!; copy.Active = !copy.Active;
        try { (await CallService.Post(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/administration/menus" }, copy)).EnsureSuccess(); await Reload(); MenuState.SetState(Guid.NewGuid().ToString()); }
        catch (Exception ex) { error = ex.Message; }
        finally { busy = false; }
    }
    private async Task Delete()
    {
        if (busy || pendingDelete == null) return;
        busy = true;
        try { (await CallService.Delete(new ApiRequestModel { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = $"/administration/menus/{pendingDelete.Id}" })).EnsureSuccess(); pendingDelete = null; message = "Đã xóa bản ghi."; await Reload(); MenuState.SetState(Guid.NewGuid().ToString()); }
        catch (Exception ex) { error = ex.Message; }
        finally { busy = false; }
    }
}
