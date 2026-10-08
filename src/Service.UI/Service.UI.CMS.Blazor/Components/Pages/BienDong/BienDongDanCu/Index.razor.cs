using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications;
using Service.UI.CMS.Blazor.Components.Shared;

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong.BienDongDanCu;

public partial class Index
{
    [Inject] private IPopulationService PopulationService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private IQueryable<BienDongDto>? bienDongQuery;
    private PaginationState pagination = new PaginationState { ItemsPerPage = 10 };

    private LoaiBienDongEnum? ChangeType => null;

    private string TypeLabel => ChangeType.HasValue ? BusinessCreateDialog.Label(ChangeType.Value.ToString()) : "Biến động dân cư";
    private string Title => ChangeType.HasValue ? $"Danh sách {TypeLabel.ToLowerInvariant()}" : "Danh sách biến động dân cư";
    private int loadVersion;

    private string searchKeyword = "";
    private int? selectedType;
    private int? FilterType => ChangeType.HasValue ? (int)ChangeType.Value : selectedType;
    private DateTime? fromDate;
    private DateTime? toDate;
    private string? filterError;

    protected override async Task OnInitializedAsync()
    {
        selectedType = ChangeType.HasValue ? (int)ChangeType.Value : null;
        await LoadData();
    }

    private BienDongDto? selectedRecord;
    private void OpenView(BienDongDto record) => selectedRecord = record;
    private void CloseView() => selectedRecord = null;

    private async Task LoadData()
    {
        var version = ++loadVersion;
        bienDongQuery = Array.Empty<BienDongDto>().AsQueryable();
        filterError = null;
        var res = await PopulationService.GetBienDongsAsync(searchKeyword, 1, int.MaxValue, FilterType, fromDate, toDate);
        if (version != loadVersion) return;
        if (res.Success && res.Data != null)
        {
            bienDongQuery = res.Data.Items.AsQueryable();
            await pagination.SetCurrentPageIndexAsync(0);
        }
        else filterError = res.Message ?? "Không thể tải danh sách biến động.";
    }

    private async Task ClearSearch()
    {
        searchKeyword = "";
        selectedType = ChangeType.HasValue ? (int)ChangeType.Value : null;
        fromDate = null;
        toDate = null;
        await LoadData();
    }

    private async Task RefreshData(int pageSize)
    {
        await LoadData();
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

            var baseQuery = new BienDongQuery
            {
                draw = 1,
                SearchIn = new List<string> { "HoTenNhanKhau", "CCCDNhanKhau" },
                Keyword = searchKeyword,
                LoaiBienDong = FilterType,
                TuNgay = fromDate,
                DenNgay = toDate
            };

            var result = await CallService.PostForFile(apiRequest, baseQuery);
            if (result.Status != StatusCode.OK || result.Data == null)
            {
                ToastService.ShowError(result.Message ?? "Đã xảy ra lỗi khi xuất Excel");
                return;
            }

            var fileType = FilterType.HasValue ? ((LoaiBienDongEnum)FilterType.Value).ToString() : "TatCa";
            var fileName = $"BienDongDanCu_{fileType}_XaTanAn_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
