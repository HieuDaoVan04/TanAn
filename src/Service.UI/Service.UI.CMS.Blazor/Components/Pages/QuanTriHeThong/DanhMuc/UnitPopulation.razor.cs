using Microsoft.AspNetCore.Components;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.DanhMuc;

public partial class UnitPopulation
{
    [Parameter, EditorRequired] public AdminRecord Unit { get; set; } = default!;
    [Parameter] public EventCallback Close { get; set; }
    [CascadingParameter] public CurrentUserDto CurrentUser { get; set; } = new();
    [Inject] public IServiceScopeFactory ScopeFactory { get; set; } = default!;
    private bool people, busy;
    private int page = 1, total;
    private string keyword = "";
    private string? error;
    private Guid? loadedUnit;
    private List<HoGiaDinhDto> households = new();
    private List<NhanKhauDto> residents = new();

    protected override async Task OnParametersSetAsync()
    {
        if (loadedUnit != Unit.Id) { loadedUnit = Unit.Id; page = 1; keyword = ""; await Load(); }
    }
    private async Task Switch(bool value) { people = value; await Search(); }
    private async Task Search() { page = 1; await Load(); }
    private async Task Move(int delta) { page += delta; await Load(); }
    private async Task Load()
    {
        households.Clear(); residents.Clear(); total = 0; error = null;
        if (!CurrentUser.IsAuthenticated || CurrentUser.Role != "Admin") { error = "Bạn chưa có quyền quản trị."; return; }
        busy = true;
        try
        {
            using var scope = ScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IPopulationService>();
            if (people)
            {
                var result = await service.GetNhanKhausAsync(keyword, null, page, 20, Unit.Id);
                if (!result.Success || result.Data == null) { error = result.Message; return; }
                residents = result.Data.Items; total = result.Data.TotalCount;
            }
            else
            {
                var result = await service.GetHoGiaDinhsAsync(keyword, null, page, 20, Unit.Id);
                if (!result.Success || result.Data == null) { error = result.Message; return; }
                households = result.Data.Items; total = result.Data.TotalCount;
            }
        }
        catch { error = "Không tải được dân cư của đơn vị. Vui lòng thử lại."; }
        finally { busy = false; }
    }
}
