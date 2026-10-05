using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications;
using Service.UI.CMS.Blazor.Components.Shared;

namespace Service.UI.CMS.Blazor.Components.Pages.AnSinh;

public partial class WelfareList
{
    [Inject] private IWelfareService WelfareService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    [Parameter] public string? Nhom { get; set; }
    private int _loadVersion;
    private bool showCreate;
    private string searchKeyword = "";
    private Task ReloadData() => OnParametersSetAsync();
    private DoiTuongAnSinhDto? detail, payoutTarget;
    private DoiTuongAnSinhEnum? Category => Nhom switch
    {
        "ho-ngheo" => DoiTuongAnSinhEnum.HoNgheo,
        "ho-can-ngheo" => DoiTuongAnSinhEnum.HoCanNgheo,
        "nguoi-cao-tuoi" => DoiTuongAnSinhEnum.NguoiCaoTuoi,
        _ => null
    };

    private async Task Created()
    {
        showCreate = false;
        payoutTarget = null;
        await OnParametersSetAsync();
    }

    private string Title => Nhom switch
    {
        "ho-ngheo" => "Quản lý hộ nghèo",
        "ho-can-ngheo" => "Quản lý hộ cận nghèo",
        "nguoi-cao-tuoi" => "Quản lý người cao tuổi",
        null => "Quản lý an sinh xã hội",
        _ => "Nhóm an sinh không tồn tại"
    };

    private IQueryable<DoiTuongAnSinhDto>? welfareQuery;
    private PaginationState pagination = new PaginationState { ItemsPerPage = 10 };

    protected override async Task OnParametersSetAsync()
    {
        var version = ++_loadVersion;
        showCreate = false;
        detail = null;
        payoutTarget = null;
        welfareQuery = Array.Empty<DoiTuongAnSinhDto>().AsQueryable();
        pagination = new PaginationState { ItemsPerPage = 10 };
        int? category = Nhom switch
        {
            "ho-ngheo" => (int)DoiTuongAnSinhEnum.HoNgheo,
            "ho-can-ngheo" => (int)DoiTuongAnSinhEnum.HoCanNgheo,
            "nguoi-cao-tuoi" => (int)DoiTuongAnSinhEnum.NguoiCaoTuoi,
            _ => null
        };
        if (Nhom != null && category == null) return;
        var res = await WelfareService.GetDoiTuongAnSinhsAsync(searchKeyword, category, 1, 200);
        if (version != _loadVersion) return;
        if (res.Success && res.Data != null)
        {
            welfareQuery = res.Data.Items.AsQueryable();
        }
    }

    private async Task ClearSearch()
    {
        searchKeyword = "";
        await OnParametersSetAsync();
    }

    private async Task RefreshData(int pageSize)
    {
        await OnParametersSetAsync();
    }

    private async Task ExportExcel() => await XuatExcel();

    private async Task XuatExcel()
    {
        try
        {
            var apiRequest = new ApiRequestModel
            {
                ApiService = Service.Shared.Commons.Enums.ServicesRegistryEnum.ServiceAIM,
                Endpoint = "/AnSinh/xuat-excel"
            };

            var baseQuery = new BaseQuery
            {
                draw = 1,
                SearchIn = new List<string> { "HoTen", "CCCD" },
                Keyword = searchKeyword?.ToLower()
            };

            int? category = Nhom switch
            {
                "ho-ngheo" => (int)DoiTuongAnSinhEnum.HoNgheo,
                "ho-can-ngheo" => (int)DoiTuongAnSinhEnum.HoCanNgheo,
                "nguoi-cao-tuoi" => (int)DoiTuongAnSinhEnum.NguoiCaoTuoi,
                _ => null
            };

            var result = await CallService.PostForFile(apiRequest, new
            {
                baseQuery.draw,
                baseQuery.SearchIn,
                baseQuery.Keyword,
                LoaiDoiTuong = category
            });

            if (result.Status != StatusCode.OK || result.Data == null)
            {
                ToastService.ShowError(result.Message ?? "Đã xảy ra lỗi khi xuất Excel");
                return;
            }

            var fileName = $"AnSinhXaHoi_XaTanAn_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
