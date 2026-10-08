using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Pages.DichVuCong;

public partial class Index
{
    [Inject] public Service.UI.CMS.Blazor.Applications.IUserService Users { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;

    private string searchKeyword = "", error = "";
    private bool busy;
    private YeuCauDto? detail;
    private Guid? pendingId;
    private TrangThaiHoSoEnum pendingStatus;

    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    private List<Service.TanAn.Domain.Entities.ApThon> villages = new();
    private async Task OpenCreate()
    {
        error = "";
        using var scope = Scopes.CreateScope();
        var user = await Users.GetCurrentUserAsync();
        var query = scope.ServiceProvider.GetRequiredService<ITanAnDbContext>().ApThons.Where(x=>x.DangHoatDong);
        if(user.Role == "CanBoThon") query = query.Where(x=>user.VillageIds.Contains(x.Id));
        villages = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query.OrderBy(x=>x.Ten));
        form = new() { LoaiYeuCau = loaiList[0], ApThonId = villages.Count == 1 ? villages[0].Id : null };
        showModal = true;
    }

    private void ConfirmStatus(Guid id, TrangThaiHoSoEnum status)
    {
        pendingId = id;
        pendingStatus = status;
    }

    private async Task ApplyStatus()
    {
        if (pendingId.HasValue)
        {
            await UpdateStatus(pendingId.Value, pendingStatus);
            pendingId = null;
        }
    }

    private static string StatusLabel(TrangThaiHoSoEnum status) => status switch
    {
        TrangThaiHoSoEnum.MoiTiepNhan => "Mới tiếp nhận",
        TrangThaiHoSoEnum.DangXuLy => "Đang xử lý",
        TrangThaiHoSoEnum.YeuCauBoSung => "Yêu cầu bổ sung",
        TrangThaiHoSoEnum.DaPheDuyet => "Đã phê duyệt",
        TrangThaiHoSoEnum.TuChoi => "Từ chối",
        _ => "Không xác định"
    };

    private IQueryable<YeuCauDto>? requestsQuery;
    private PaginationState pagination = new PaginationState { ItemsPerPage = 10 };
    private bool showModal = false;
    private CreateYeuCauForm form = new CreateYeuCauForm { LoaiYeuCau = "Đăng ký Tạm trú" };
    private List<string> loaiList = new() { "Đăng ký Tạm trú", "Đăng ký Khai sinh", "Xác nhận Hộ nghèo", "Cấp giấy xác nhận cư trú" };

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        var res = await RequestService.GetYeuCausAsync(searchKeyword, null, 1, 200);
        if (res.Success && res.Data != null)
        {
            requestsQuery = res.Data.Items.AsQueryable();
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

    private async Task ExportExcel() => await XuatExcel();

    private async Task XuatExcel()
    {
        try
        {
            var apiRequest = new ApiRequestModel
            {
                ApiService = Service.Shared.Commons.Enums.ServicesRegistryEnum.ServiceAIM,
                Endpoint = "/DichVu/xuat-excel/danh-sach-du-lieu"
            };

            var baseQuery = new BaseQuery
            {
                draw = 1,
                SearchIn = new List<string> { "MaYeuCau", "HoTenNguoiYeuCau", "CCCDNguoiYeuCau" },
                Keyword = searchKeyword?.ToLower()
            };

            var result = await CallService.PostForFile(apiRequest, baseQuery);
            if (result.Status != StatusCode.OK || result.Data == null)
            {
                ToastService.ShowError(result.Message ?? "Đã xảy ra lỗi khi xuất Excel");
                return;
            }

            var fileName = $"HoSoDichVuCong_XaTanAn_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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

    private async Task SubmitForm()
    {
        if (busy) return;
        if (new[] { form.HoTenNguoiYeuCau, form.CCCDNguoiYeuCau, form.SoDienThoai, form.NoiDung }.Any(string.IsNullOrWhiteSpace))
        {
            error = "Vui lòng nhập đủ họ tên, CCCD, số điện thoại và nội dung.";
            return;
        }
        busy = true;
        error = "";
        try
        {
            var res = await RequestService.CreateYeuCauAsync(form);
            if (res.Success)
            {
                showModal = false;
                await LoadData();
                ToastService.ShowSuccess("Nộp hồ sơ trực tuyến thành công!");
            }
            else error = res.Message;
        }
        catch
        {
            error = "Không lưu được hồ sơ. Hãy tải lại danh sách để kiểm tra.";
        }
        finally
        {
            busy = false;
        }
    }

    private async Task UpdateStatus(Guid id, TrangThaiHoSoEnum status)
    {
        if (busy) return;
        busy = true;
        try
        {
            var user = await Users.GetCurrentUserAsync();
            if (!user.IsAuthenticated || (user.Role != "Admin" && !user.MenusActive.Any(m => m.Path == "/dich-vu-cong")))
            {
                ToastService.ShowError("Bạn không còn quyền xử lý hồ sơ.");
                return;
            }
            var res = await RequestService.UpdateYeuCauStatusAsync(new UpdateYeuCauStatusForm { YeuCauId = id, TrangThai = status, CanBoXuLy = user.UserName });
            if (res.Success)
            {
                await LoadData();
                ToastService.ShowSuccess("Đã cập nhật trạng thái hồ sơ!");
            }
            else ToastService.ShowError(res.Message);
        }
        catch
        {
            ToastService.ShowError("Không cập nhật được trạng thái hồ sơ.");
        }
        finally
        {
            busy = false;
        }
    }
}
