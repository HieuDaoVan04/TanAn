using Service.Shared.Commons.Model.SQL;
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

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong.KhaiSinh;

public partial class Index
{
    [SupplyParameterFromQuery(Name = "hoSoId")] public Guid? RequestedDraftId { get; set; }
    private Guid? openedQueryDraftId;

    protected override async Task OnParametersSetAsync()
    {
        if (!RequestedDraftId.HasValue) { openedQueryDraftId = null; return; }
        if (openedQueryDraftId == RequestedDraftId) return;
        openedQueryDraftId = RequestedDraftId;
        showDrafts = true; activeTabId = "drafts";
        searchKeyword = ""; selectedVillageId = null; fromDate = null; toDate = null;
        await LoadData();
        await ViewDraft(new() { Id = RequestedDraftId.Value });
    }
    [Inject] private IPopulationService PopulationService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private IQueryable<BienDongDto>? bienDongQuery;
    private PaginationState pagination = new PaginationState { ItemsPerPage = 10 };

    private LoaiBienDongEnum? ChangeType => LoaiBienDongEnum.KhaiSinh;

    private string TypeLabel => ChangeType.HasValue ? BusinessCreateDialog.Label(ChangeType.Value.ToString()) : "Biến động dân cư";
    private string Title => ChangeType.HasValue ? $"Danh sách {TypeLabel.ToLowerInvariant()}" : "Danh sách biến động dân cư";
    private int loadVersion;

    [Inject] private IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] private IUserService Users { get; set; } = default!;
    private List<ApThon> villages = new();
    private Guid? selectedVillageId;
    private string searchKeyword = "";
    private int? FilterType => (int)LoaiBienDongEnum.KhaiSinh;
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
        var user = await Users.GetCurrentUserAsync();
        canApproveBirth = HasApprovalAccess(user);
        await LoadVillages();
        await LoadData();
    }

    private BienDongDto? selectedRecord;
    private KhaiSinhDraftDto? selectedDraft;
    private Guid? editingDraftId;
    private bool showDrafts = true;
    private string activeTabId = "drafts";
    private FluentDataGrid<KhaiSinhDraftDto>? draftGrid;
    private readonly PaginationState draftPagination = new() { ItemsPerPage = 10 };
    private List<KhaiSinhDraftDto> drafts = [];
    private int draftStartIndex;
    private bool canApproveBirth, approvalBusy;
    private static bool HasApprovalAccess(CurrentUserDto user) => user.IsAuthenticated
        && user.RoleCodes.Any(code => string.Equals(code.Trim(), Service.Shared.Commons.Enums.RoleCodes.ChuTichXa, StringComparison.OrdinalIgnoreCase))
        && user.MenusActive.Any(menu => menu.Path.TrimEnd('/') == "/bien-dong/khai-sinh");
    private bool CanApprove(KhaiSinhDraftDto row) => canApproveBirth && !approvalBusy && row.ModerationStatus == ModerationStatus.Pending;
    private void OpenView(BienDongDto record) { showEdit = false; selectedDraft = null; selectedRecord = record; }
    private void CloseView() { selectedRecord = null; selectedDraft = null; }

    private bool showEdit;
    private void OpenEdit() { selectedRecord = null; selectedDraft = null; editingDraftId = null; showEdit = true; }
    private void EditDraft(KhaiSinhDraftDto row) { if ((row.ModerationStatus == ModerationStatus.Approved) || approvalBusy) return; selectedRecord = null; selectedDraft = null; editingDraftId = row.Id; showEdit = true; }
    private async Task ApproveDraft(KhaiSinhDraftDto row)
    {
        if (approvalBusy || row.ModerationStatus != ModerationStatus.Pending) return;
        approvalBusy = true;
        filterError = null;
        try
        {
            var user = await Users.GetCurrentUserAsync();
            canApproveBirth = HasApprovalAccess(user);
            if (!canApproveBirth)
            { filterError = Service.TanAn.Application.Services.BirthApprovalAuthorization.DeniedMessage; return; }
            using var scope = Scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IPopulationService>()
                .ApproveKhaiSinhAsync(row.Id, row.PhienBan, user.UserName);
            if (!result.Success) { filterError = result.Message; return; }
            if (selectedDraft?.Id == row.Id) selectedDraft = result.Data;
            showDrafts = false; activeTabId = "recorded"; draftGrid = null;
            searchKeyword = ""; selectedVillageId = null; fromDate = null; toDate = null;
            await LoadData();
            ToastService.ShowSuccess(result.Message);
        }
        catch { filterError = "Không duyệt được hồ sơ. Vui lòng tải lại và thử lại."; }
        finally { approvalBusy = false; }
    }
    private async Task ViewDraft(KhaiSinhDraftDto row)
    {
        try
        {
            using var scope = Scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IPopulationService>().GetKhaiSinhDraftAsync(row.Id);
            if (!result.Success) { filterError = result.Message; return; }
            if (result.Data?.ModerationStatus == ModerationStatus.Approved && showDrafts)
            {
                showDrafts = false; activeTabId = "recorded"; draftGrid = null;
                await LoadData();
            }
            showEdit = false; selectedRecord = null; selectedDraft = result.Data;
        }
        catch { filterError = "Không tải được hồ sơ. Vui lòng thử lại."; }
    }
    private Task ChangeTab() => activeTabId == "recorded" ? ShowRecorded() : ShowDrafts();
    private async Task ShowDrafts() { showDrafts = true; activeTabId = "drafts"; CloseView(); await LoadData(); }
    private async Task ShowRecorded() { showDrafts = false; activeTabId = "recorded"; draftGrid = null; CloseView(); await LoadData(); }
    private void CloseEdit() => showEdit = false;
    private async Task OnSaved()
    {
        showEdit = false;
        showDrafts = true; activeTabId = "drafts";
        searchKeyword = ""; fromDate = null; toDate = null; selectedVillageId = null;
        await LoadData();
    }

    private async Task LoadData()
    {
        var version = ++loadVersion;
        bienDongQuery = Array.Empty<BienDongDto>().AsQueryable();
        filterError = null;
        if (showDrafts)
        {
            await draftPagination.SetCurrentPageIndexAsync(0);
            if (draftGrid != null) await draftGrid.RefreshDataAsync();
            return;
        }
        var res = await PopulationService.GetBienDongsAsync(searchKeyword, 1, int.MaxValue, FilterType, fromDate, toDate, selectedVillageId);
        if (version != loadVersion) return;
        if (res.Success && res.Data != null)
        {
            bienDongQuery = res.Data.Items.AsQueryable();
            await pagination.SetCurrentPageIndexAsync(0);
        }
        else filterError = res.Message ?? "Không thể tải danh sách biến động.";
    }

    private async ValueTask<GridItemsProviderResult<KhaiSinhDraftDto>> LoadDrafts(GridItemsProviderRequest<KhaiSinhDraftDto> request)
    {
        var size = request.Count ?? draftPagination.ItemsPerPage;
        try
        {
            using var scope = Scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IPopulationService>()
                .GetKhaiSinhDraftsAsync(searchKeyword, request.StartIndex / size + 1, size, selectedVillageId, fromDate, toDate, choDuyet: true);
            request.CancellationToken.ThrowIfCancellationRequested();
            drafts = result.Data?.Items ?? [];
            draftStartIndex = request.StartIndex;
            filterError = result.Success ? null : result.Message;
            await InvokeAsync(StateHasChanged);
            return GridItemsProviderResult.From(drafts, result.Data?.TotalCount ?? 0);
        }
        catch (OperationCanceledException) when (request.CancellationToken.IsCancellationRequested) { throw; }
        catch
        {
            request.CancellationToken.ThrowIfCancellationRequested();
            drafts = [];
            filterError = "Không tải được hồ sơ nháp. Kiểm tra migration DB và thử lại.";
            await InvokeAsync(StateHasChanged);
            return GridItemsProviderResult.From(drafts, 0);
        }
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
