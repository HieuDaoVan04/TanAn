using Microsoft.EntityFrameworkCore;
using Service.TanAn.Domain.Entities;
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

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong.TamVang;

public partial class Index
{
    [Inject] private IPopulationService PopulationService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private IQueryable<BienDongDto>? bienDongQuery;
    private PaginationState pagination = new PaginationState { ItemsPerPage = 10 };

    private LoaiBienDongEnum? ChangeType => LoaiBienDongEnum.TamVang;

    private string TypeLabel => ChangeType.HasValue ? BusinessCreateDialog.Label(ChangeType.Value.ToString()) : "Biến động dân cư";
    private string Title => ChangeType.HasValue ? $"Danh sách {TypeLabel.ToLowerInvariant()}" : "Danh sách biến động dân cư";
    private int loadVersion;

    [Inject] private IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] private IUserService Users { get; set; } = default!;
    private List<ApThon> villages = new();
    private Guid? selectedVillageId;
    private string searchKeyword = "";
    private int? FilterType => (int)LoaiBienDongEnum.TamVang;
    private DateTime? fromDate;
    private DateTime? toDate;
    private string? filterError;

    private async Task LoadVillages()
    {
        var user = await Users.GetCurrentUserAsync();
        if (!user.IsAuthenticated) return;
        using var scope = Scopes.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ITanAnDbContext>().ApThons.AsNoTracking()
            .Where(x => x.DangHoatDong);
        if (user.Role != "Admin" && user.Role != "CanBoXa" && user.Role != "ChuTichXa")
            query = query.Where(x => user.VillageIds.Contains(x.Id));
        villages = await query.OrderBy(x => x.Ten).ToListAsync();
    }
    protected override async Task OnInitializedAsync()
    {
        await LoadVillages();
        await LoadData();
    }

    private BienDongDto? selectedRecord;
    private void OpenView(BienDongDto record) { showEdit = false; selectedRecord = record; }
    private void CloseView() => selectedRecord = null;

    private bool showEdit;
    private void OpenEdit() { selectedRecord = null; showEdit = true; }
    private void CloseEdit() => showEdit = false;
    private async Task OnSaved()
    {
        showEdit = false;
        await LoadData();
    }

    private async Task LoadData()
    {
        var version = ++loadVersion;
        bienDongQuery = Array.Empty<BienDongDto>().AsQueryable();
        filterError = null;
        var res = await PopulationService.GetBienDongsAsync(searchKeyword, 1, int.MaxValue, FilterType, fromDate, toDate, selectedVillageId);
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
        selectedVillageId = null;
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
                DenNgay = toDate,
                ApThonId = selectedVillageId
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
