using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications;
using Service.UI.CMS.Blazor.Components.Shared;

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong;

public partial class Index
{
    [Inject] private IPopulationService PopulationService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private IQueryable<BienDongDto>? bienDongQuery;
    private PaginationState pagination = new PaginationState { ItemsPerPage = 10 };

    private bool showCreate;
    private string searchKeyword = "";
    private BienDongDto? detail;

    protected override Task OnInitializedAsync() => LoadData();

    private async Task LoadData()
    {
        var res = await PopulationService.GetBienDongsAsync(searchKeyword, 1, 200);
        if (res.Success && res.Data != null)
        {
            bienDongQuery = res.Data.Items.AsQueryable();
            await pagination.SetCurrentPageIndexAsync(0);
        }
    }

    private async Task ClearSearch()
    {
        searchKeyword = "";
        await LoadData();
    }

    private async Task RefreshData(int pageSize)
    {
        await LoadData();
    }

    private async Task Created()
    {
        showCreate = false;
        await LoadData();
        await pagination.SetCurrentPageIndexAsync(0);
    }

    private async Task ExportExcel() => await XuatExcel();

    private async Task XuatExcel()
    {
        try
        {
            var apiRequest = new ApiRequestModel
            {
                ApiService = Service.Shared.Commons.Enums.ServicesRegistryEnum.ServiceAIM,
                Endpoint = "/BienDong/xuat-excel"
            };

            var baseQuery = new BaseQuery
            {
                draw = 1,
                SearchIn = new List<string> { "HoTenNhanKhau", "CCCDNhanKhau" },
                Keyword = searchKeyword?.ToLower()
            };

            var result = await CallService.PostForFile(apiRequest, baseQuery);
            if (result.Status != StatusCode.OK || result.Data == null)
            {
                ToastService.ShowError(result.Message ?? "Đã xảy ra lỗi khi xuất Excel");
                return;
            }

            var fileName = $"BienDongDanCu_XaTanAn_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
