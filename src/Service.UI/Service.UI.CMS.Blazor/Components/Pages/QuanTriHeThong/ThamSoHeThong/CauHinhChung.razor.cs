using Microsoft.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.ThamSoHeThong;

public partial class CauHinhChung
{
    [Inject] private ICallServiceRegistry Calls { get; set; } = default!;
    [Inject] private ModuleTypeState State { get; set; } = default!;
    [CascadingParameter] public CurrentUserDto CurrentUser { get; set; } = new();
    private List<SystemConfigurationField> fields = new();
    private Dictionary<string, string> saved = new();
    private bool loading, busy;
    private string error = "", message = "";
    private bool IsDirty => fields.Any(x => !saved.TryGetValue(x.Code, out var value) || value != x.Value);
    private static ApiRequestModel Request => new() { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/system-configuration/general" };
    protected override Task OnInitializedAsync() => Reload();
    private async Task Reload()
    {
        if (!CurrentUser.IsAuthenticated || CurrentUser.Role != "Admin") return;
        loading = true; error = "";
        try
        {
            fields = (await Calls.Get<List<SystemConfigurationField>>(Request)).RequireData();
            saved = fields.ToDictionary(x => x.Code, x => x.Value);
        }
        catch (Exception ex) { error = ex.Message; fields = new(); }
        finally { loading = false; }
    }
    private async Task Save()
    {
        if (busy || !IsDirty) return;
        busy = true; error = ""; message = "";
        try
        {
            var updates = fields.Where(x => saved[x.Code] != x.Value).Select(x => new SystemConfigurationUpdate
            { Code = x.Code, Value = x.Value, ExpectedValue = saved[x.Code] }).ToList();
            (await Calls.Put(Request, updates)).EnsureSuccess();
            message = "Đã lưu cấu hình chung.";
            await Reload();
            State.SetState(Guid.NewGuid().ToString());
        }
        catch (Exception ex) { error = ex.Message; }
        finally { busy = false; }
    }
}
