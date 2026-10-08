using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.QuanLyPhien;

public partial class Index
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private IReadOnlyList<LoginSession> rows = Array.Empty<LoginSession>();
    private LoginSession? pending;
    private LoginSession? details;
    private PaginationState pagination = new() { ItemsPerPage = 10 };
    private string appliedSearch = "";
    private string search = "";
    private string? error;
    private bool busy;

    private Task ApplySearch()
    {
        appliedSearch = search;
        return pagination.SetCurrentPageIndexAsync(0);
    }

    private string Remaining(LoginSession session) => Math.Max(0, (int)(session.ExpiresAt - DateTimeOffset.UtcNow).TotalMinutes) + " phút";

    private IQueryable<LoginSession> Filtered => rows.Where(x => x.Username.Contains(appliedSearch, StringComparison.OrdinalIgnoreCase)).AsQueryable();

    protected override Task OnInitializedAsync() => Reload();

    private async Task Reload()
    {
        busy = true;
        error = null;
        try
        {
            rows = await Sessions.ListAsync();
            await pagination.SetCurrentPageIndexAsync(0);
        }
        catch
        {
            rows = Array.Empty<LoginSession>();
            error = "Không tải được phiên. Kiểm tra quyền truy cập và kết nối Redis.";
        }
        finally
        {
            busy = false;
        }
    }

    private async Task RefreshData(int pageSize)
    {
        await Reload();
    }

    private async Task Revoke()
    {
        if (pending == null || busy) return;
        busy = true;
        try
        {
            await Sessions.RevokeAsync(pending.Id);
            pending = null;
            await Reload();
        }
        catch
        {
            error = "Không thể thu hồi phiên. Hãy tải lại để kiểm tra trạng thái.";
        }
        finally
        {
            busy = false;
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
                Endpoint = "/QuanLyPhien/xuat-excel"
            };

            var baseQuery = new BaseQuery
            {
                draw = 1,
                SearchIn = new List<string> { "Username" },
                Keyword = appliedSearch?.ToLower()
            };

            var result = await CallService.PostForFile(apiRequest, baseQuery);
            if (result.Status != StatusCode.OK || result.Data == null)
            {
                ToastService.ShowError(result.Message ?? "Đã xảy ra lỗi khi xuất Excel");
                return;
            }

            var fileName = $"DanhSachPhienDangNhap_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
