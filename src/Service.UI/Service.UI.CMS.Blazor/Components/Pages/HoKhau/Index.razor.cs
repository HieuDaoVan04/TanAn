using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Pages.HoKhau;

public partial class Index
{
    [Inject] private IPopulationService PopulationService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private string searchKeyword = "";
    private string selectedApThon = TanAnLocalities.All;
    private List<string> apThonList = new[] { TanAnLocalities.All }.Concat(TanAnLocalities.Villages).ToList();
    private IQueryable<HoGiaDinhDto>? householdsQuery;
    private PaginationState pagination = new PaginationState { ItemsPerPage = 10 };
    private bool showCreate;
    private HoGiaDinhDto? detail;
    private Guid selectedHead;
    private bool headBusy;
    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] public IUserService Users { get; set; } = default!;
    private async Task SaveHead()
    {
        if(detail == null || selectedHead == Guid.Empty || headBusy) return;
        headBusy=true;
        try {
            var user=await Users.GetCurrentUserAsync();
            if(!user.IsAuthenticated) throw new UnauthorizedAccessException("Hãy đăng nhập lại.");
            using var scope=Scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IPopulationService>().SetChuHoAsync(detail.Id,selectedHead,user.UserName);
            await View(detail); await LoadData();
        } catch(Exception ex) { await DialogService.ShowErrorAsync(ex.Message); }
        finally {headBusy=false;}
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private FluentDataGrid<HoGiaDinhDto>? grid;
    private async ValueTask<GridItemsProviderResult<HoGiaDinhDto>> LoadHouseholds(GridItemsProviderRequest<HoGiaDinhDto> request)
    {
        var size=request.Count ?? pagination.ItemsPerPage;
        string? ap = selectedApThon == TanAnLocalities.All || string.IsNullOrWhiteSpace(selectedApThon) ? null : selectedApThon;
        using var scope=Scopes.CreateScope();
        var result=await scope.ServiceProvider.GetRequiredService<IPopulationService>().GetHoGiaDinhsAsync(searchKeyword,ap,request.StartIndex/size+1,size);
        var items=result.Data?.Items ?? new(); householdsQuery=items.AsQueryable();
        return GridItemsProviderResult.From(items, result.Data?.TotalCount ?? 0);
    }
    private async Task LoadData()
    {
        await pagination.SetCurrentPageIndexAsync(0);
        if(grid!=null) await grid.RefreshDataAsync();
    }

    private async Task ClearSearch()
    {
        searchKeyword = "";
        selectedApThon = TanAnLocalities.All;
        await LoadData();
    }

    private async Task RefreshData(int pageSize)
    {
        await LoadData();
    }

    private void OpenAddModal()
    {
        showCreate = true;
    }

    private async Task Created()
    {
        showCreate = false;
        await LoadData();
        await pagination.SetCurrentPageIndexAsync(0);
    }

    private async Task View(HoGiaDinhDto row)
    {
        try
        {
            var result = await PopulationService.GetHoGiaDinhByIdAsync(row.Id);
            if (result.Success) { detail = result.Data; selectedHead = detail?.ThanhVien?.FirstOrDefault(x=>x.QuanHeVoiChuHo=="Chủ hộ")?.Id ?? Guid.Empty; }
            else await DialogService.ShowErrorAsync(result.Message);
        }
        catch
        {
            await DialogService.ShowErrorAsync("Không tải được thông tin hộ gia đình.");
        }
    }

    private async Task ExportExcel() => await XuatExcel();

    private async Task XuatExcel()
    {
        try
        {
            var apiRequest = new ApiRequestModel
            {
                ApiService = Service.Shared.Commons.Enums.ServicesRegistryEnum.ServiceAIM,
                Endpoint = "/HoGiaDinh/xuat-excel"
            };

            var baseQuery = new BaseQuery
            {
                draw = 1,
                SearchIn = new List<string> { "TenChuHo", "CCCDChuHo", "MaSoHo" },
                Keyword = searchKeyword?.ToLower()
            };

            var result = await CallService.PostForFile(apiRequest, new
            {
                baseQuery.draw,
                baseQuery.SearchIn,
                baseQuery.Keyword,
                ApThon = (selectedApThon == TanAnLocalities.All || string.IsNullOrWhiteSpace(selectedApThon)) ? null : selectedApThon
            });

            if (result.Status != StatusCode.OK || result.Data == null)
            {
                ToastService.ShowError(result.Message ?? "Đã xảy ra lỗi khi xuất Excel");
                return;
            }

            var fileName = $"DanhSachHoGiaDinh_XaTanAn_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            await DownloadFileFromBytes(result.Data, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            ToastService.ShowSuccess("Xuất Excel thành công!");
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Lỗi khi xuất Excel: {ex.Message}");
        }
    }

    private async Task DownloadFileFromBytes(byte[] fileBytes, string fileName, string contentType)
    {
        var stream = new MemoryStream(fileBytes);
        using var streamRef = new DotNetStreamReference(stream: stream);
        await JSRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}
