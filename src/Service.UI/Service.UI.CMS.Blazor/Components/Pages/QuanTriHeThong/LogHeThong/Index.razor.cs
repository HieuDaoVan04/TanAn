// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Extensions;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.LogHeThong
{
    public partial class Index : ComponentBase
    {
        [CascadingParameter]
        public CurrentUserDto CurrentUser { get; set; } = new();

        [Inject] private IToastService ToastService { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ICallServiceRegistry CallService { get; set; } = default!;

        protected FluentDataGrid<LogHeThongDto> Grid { get; set; } = default!;
        protected PaginationState pagination = new PaginationState { ItemsPerPage = 15 };
        protected bool IsLoadingSync { get; set; } = false;

        protected DateTime? TuNgay { get; set; }
        protected DateTime? DenNgay { get; set; }
        protected string SearchKeyword { get; set; } = string.Empty;

        private LogHeThongQuery BaseQuery { get; set; } = new LogHeThongQuery();

        protected CultureInfo customCulture = DateTimeHepler.CreateCustomCultureForDatePicker();

        protected bool DisableDatesForDenNgay(DateTime date)
        {
            if (TuNgay.HasValue && date < TuNgay.Value)
            {
                return true;
            }
            return false;
        }

        protected async ValueTask<GridItemsProviderResult<LogHeThongDto>> LoadData(GridItemsProviderRequest<LogHeThongDto> request)
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/LogsHeThong/GetPagedLogHeThong"
                };

                BaseQuery.Keyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim();
                BaseQuery.TimKiemTuNgay = TuNgay;
                BaseQuery.TimKiemDenNgay = DenNgay;
                BaseQuery.PageIndex = request.StartIndex / pagination.ItemsPerPage + 1;
                BaseQuery.PageSize = pagination.ItemsPerPage;
                BaseQuery.gridRequest = new GridRequest
                {
                    filter = new Filter(),
                    page = BaseQuery.PageIndex,
                    pageSize = pagination.ItemsPerPage,
                };

                ResultAPI<DataTableJson<LogHeThongDto>> result = await CallService.Post<DataTableJson<LogHeThongDto>>(apiRequest, BaseQuery);

                if (result.Status == StatusCode.OK && result.Data is DataTableJson<LogHeThongDto> dataTable)
                {
                    var indexedItems = dataTable.Data.Select((item, idx) =>
                    {
                        item.STT = request.StartIndex + idx + 1;
                        return item;
                    }).ToList();

                    var totalRecords = dataTable.RecordsTotal;
                    return GridItemsProviderResult.From(indexedItems, totalRecords);
                }

                ToastService.ShowError($"[{result.Status}]=>{result.Message}");
                return GridItemsProviderResult.From(new List<LogHeThongDto>(), 0);
            }
            catch (Exception ex)
            {
                ToastService.ShowError(ex.Message);
                return GridItemsProviderResult.From(new List<LogHeThongDto>(), 0);
            }
        }

        protected async Task OpenViewModal(string Id)
        {
            try
            {
                var parameters = new EditOrUpdateParametersDto
                {
                    Parameter = Id,
                };
                await DialogService.ShowDialogAsync<View>(parameters, new DialogParameters
                {
                    Title = "Chi tiết bản ghi log",
                    Width = "1200px",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    PrimaryAction = null,
                    SecondaryAction = null,
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal chi tiết: {ex.Message}");
            }
        }

        protected async Task RefreshData(int value)
        {
            try
            {
                pagination.ItemsPerPage = value;
                await pagination.SetCurrentPageIndexAsync(0);
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách: {ex.Message}");
            }
        }

        protected async Task RefreshGrid()
        {
            try
            {
                if (Grid != null)
                {
                    await pagination.SetCurrentPageIndexAsync(0);
                    await Grid.RefreshDataAsync();
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi làm mới dữ liệu: {ex.Message}");
            }
        }

        protected async Task ResetFilters()
        {
            SearchKeyword = string.Empty;
            TuNgay = null;
            DenNgay = null;
            await RefreshGrid();
        }

        protected static string DisplayValue(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "—" : value;

        protected static string ActionStyle(string? action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return "background:#f3f4f6;color:#374151;";

            var value = action.ToLowerInvariant();
            if (value.Contains("xóa") || value.Contains("delete") || value.Contains("hủy"))
                return "background:#fee2e2;color:#991b1b;";
            if (value.Contains("tạo") || value.Contains("create") || value.Contains("thêm"))
                return "background:#dcfce7;color:#166534;";
            if (value.Contains("sửa") || value.Contains("update") || value.Contains("cập nhật"))
                return "background:#fef3c7;color:#92400e;";

            return "background:#e0f2fe;color:#075985;";
        }

        public class SelectOption
        {
            public int Value { get; set; }
            public string Text { get; set; } = string.Empty;
        }
    }
}
