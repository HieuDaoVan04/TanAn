// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.SystemParameter
{
    public partial class Index : ComponentBase
    {
        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;

        protected string? SearchKeyword { get; set; } = string.Empty;

        protected GridSort<SystemParameterDto> _roleNameSort = GridSort<SystemParameterDto>.ByDescending(x => x.Created);
        protected FluentDataGrid<SystemParameterDto> Grid { get; set; } = default!;

        protected PaginationState pagination = new PaginationState { ItemsPerPage = 10 };

        protected bool IsLoadingSync { get; set; } = false;

        protected async Task<bool> RejectAction(Guid id)
        {
            IsLoadingSync = true;
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/SystemParameter/{id}/Reject"
                };

                ResultAPI<object> result = await CallService.Put<object>(apiRequest, new object());
                if (result.Status == StatusCode.OK)
                {
                    await RefreshGrid();
                    ToastService.ShowSuccess("Hủy duyệt thành công!");
                    return true;
                }
                else
                {
                    throw new Exception(result.Message ?? "Lỗi khi hủy duyệt.");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi hủy duyệt: {ex.Message}");
                return false;
            }
            finally
            {
                IsLoadingSync = false;
            }
        }

        protected async Task<bool> ApproveAction(Guid id)
        {
            IsLoadingSync = true;
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/SystemParameter/{id}/approve"
                };

                ResultAPI<object> result = await CallService.Put<object>(apiRequest, new object());
                if (result.Status == StatusCode.OK)
                {
                    await RefreshGrid();
                    ToastService.ShowSuccess("Duyệt thành công!");
                    return true;
                }
                else
                {
                    throw new Exception(result.Message ?? "Lỗi khi duyệt.");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi duyệt tham số hệ thống: {ex.Message}");
                return false;
            }
            finally
            {
                IsLoadingSync = false;
            }
        }

        protected async Task EditAction(Guid Id)
        {
            try
            {
                var parameters = new EditOrUpdateParametersDto
                {
                    Id = Id,
                    IsEditMode = true,
                    OnRefresh = EventCallback.Factory.Create(this, RefreshGrid)
                };
                await DialogService.ShowDialogAsync<Edit>(parameters, new DialogParameters
                {
                    Title = "Chỉnh sửa tham số hệ thống",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Width = "800px",
                    Modal = true,
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal chỉnh sửa: {ex.Message}");
            }
        }

        protected async Task OpenModalDelete(Guid id)
        {
            try
            {
                var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
                {
                    Content = new MessageBoxContent
                    {
                        Title = "Xác nhận xóa",
                        MarkupMessage = new MarkupString("Bạn có chắc chắn muốn xóa bản ghi tham số hệ thống này không?"),
                        Icon = new Icons.Regular.Size24.Warning(),
                        IconColor = Color.Warning,
                    },
                    PrimaryAction = "Xóa",
                    SecondaryAction = "Hủy",
                });
                var resultDialog = await dialog.Result;
                if (!resultDialog.Cancelled)
                {
                    ApiRequestModel apiRequest = new ApiRequestModel()
                    {
                        ApiService = ServicesRegistryEnum.ServicePortal,
                        Endpoint = $"/SystemParameter/{id}"
                    };
                    var result = await CallService.Delete(apiRequest);
                    if (result.Status == StatusCode.OK)
                    {
                        await RefreshGrid();
                        ToastService.ShowSuccess("Xóa bản ghi thành công!");
                    }
                    else
                    {
                        ToastService.ShowError(result.Message ?? "Lỗi khi xóa bản ghi.");
                    }
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi xóa bản ghi: {ex.Message}");
            }
        }

        protected async Task OpenAddModal()
        {
            try
            {
                var parameters = new EditOrUpdateParametersDto
                {
                    Id = Guid.Empty,
                    IsEditMode = false,
                    OnRefresh = EventCallback.Factory.Create(this, RefreshGrid)
                };

                await DialogService.ShowDialogAsync<Edit>(parameters, new DialogParameters
                {
                    Title = "Thêm mới tham số hệ thống",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "800px"
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal thêm mới: {ex.Message}");
            }
        }

        protected async Task OpenDetailsModal(Guid Id)
        {
            try
            {
                var parameters = new EditOrUpdateParametersDto
                {
                    Id = Id,
                };
                await DialogService.ShowDialogAsync<View>(parameters, new DialogParameters
                {
                    Title = "Thông tin chi tiết tham số hệ thống",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    PrimaryAction = null,
                    Width = "800px",
                    SecondaryAction = null,
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal chi tiết: {ex.Message}");
            }
        }

        protected async Task SyncRole()
        {
            IsLoadingSync = true;
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/SystemParameter/syncrsystemparameterfromenum"
                };
                ResultAPI<string> result = await CallService.Get<string>(apiRequest);

                if (result.Status != StatusCode.OK)
                {
                    ToastService.ShowWarning($"Lỗi: {result.Message ?? "Đã xảy ra lỗi khi thực hiện thao tác"}");
                }
                else
                {
                    ToastService.ShowSuccess("Đồng bộ tham số hệ thống thành công!");
                    await RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi đồng bộ tham số hệ thống: {ex.Message}");
            }
            finally
            {
                IsLoadingSync = false;
            }
        }

        protected async ValueTask<GridItemsProviderResult<SystemParameterDto>> LoadDatas(GridItemsProviderRequest<SystemParameterDto> request)
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/SystemParameter/GetPaged"
                };
                var sortProp = request.GetSortByProperties().FirstOrDefault();
                var baseQuery = new BaseQuery
                {
                    draw = 1,
                    SearchIn = new List<string> { "Code", "Value", "Description" },
                    Keyword = SearchKeyword,
                    SortBy = sortProp.PropertyName,
                    IsAscending = sortProp.Direction == SortDirection.Ascending,
                    gridRequest = new GridRequest
                    {
                        filter = new Filter(),
                        page = request.StartIndex / pagination.ItemsPerPage + 1,
                        pageSize = pagination.ItemsPerPage,
                    }
                };
                ResultAPI<DataTableJson<SystemParameterDto>> result = await CallService.Post<DataTableJson<SystemParameterDto>>(apiRequest, baseQuery);

                if (result.Status == StatusCode.OK && result.Data is DataTableJson<SystemParameterDto> dataTable)
                {
                    var indexedItems = dataTable.Data.Select((item, idx) =>
                    {
                        item.STT = request.StartIndex + idx + 1;
                        return item;
                    }).ToList();

                    var totalRecords = dataTable.RecordsTotal;
                    return GridItemsProviderResult.From(indexedItems, totalRecords);
                }

                return GridItemsProviderResult.From(new List<SystemParameterDto>(), 0);
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách tham số hệ thống: {ex.Message}");
                return GridItemsProviderResult.From(new List<SystemParameterDto>(), 0);
            }
        }

        protected async Task HandleSearchChanged()
        {
            try
            {
                await Grid.RefreshDataAsync();
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tìm kiếm: {ex.Message}");
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
            await Grid.RefreshDataAsync();
        }
    }
}
