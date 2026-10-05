using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Pages.AuditLogs;

public partial class Index
{
    [Inject] private IAuditLogService AuditLogService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private AuditLog? detail;
    private string searchKeyword = "";
    private IQueryable<AuditLog>? auditLogsQuery;
    private PaginationState pagination = new PaginationState { ItemsPerPage = 15 };

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        var res = await AuditLogService.GetAuditLogsAsync(searchKeyword, 1, 200);
        if (res.Success && res.Data != null)
        {
            auditLogsQuery = res.Data.Items.AsQueryable();
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

    private static string DisplayValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value;

    private static string DisplayIp(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Chưa ghi nhận" : value;

    private static string DisplayNewValue(AuditLog log) =>
        string.IsNullOrWhiteSpace(log.NewValues)
            ? $"{log.Action} - {log.EntityName} ({log.EntityId})"
            : log.NewValues;

    private async Task ExportExcel() => await XuatExcel();

    private async Task XuatExcel()
    {
        try
        {
            var apiRequest = new ApiRequestModel
            {
                ApiService = Service.Shared.Commons.Enums.ServicesRegistryEnum.ServiceAIM,
                Endpoint = "/AuditLog/xuat-excel"
            };

            var baseQuery = new BaseQuery
            {
                draw = 1,
                SearchIn = new List<string> { "Username", "Action", "EntityName" },
                Keyword = searchKeyword?.ToLower()
            };

            var result = await CallService.PostForFile(apiRequest, baseQuery);
            if (result.Status != StatusCode.OK || result.Data == null)
            {
                ToastService.ShowError(result.Message ?? "Đã xảy ra lỗi khi xuất Excel");
                return;
            }

            var fileName = $"NhatKyThaoTac_XaTanAn_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
