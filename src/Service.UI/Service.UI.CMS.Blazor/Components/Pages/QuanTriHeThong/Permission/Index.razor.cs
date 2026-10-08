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
using PermissionDto = Service.Shared.Contracts.DTOs.PermissionDto;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.Permission
{
    public partial class Index : ComponentBase
    {
        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;

        private string? searchKeyword;
        private System.Timers.Timer? _debounceTimer;

        protected string? SearchKeyword
        {
            get => searchKeyword;
            set
            {
                if (searchKeyword != value)
                {
                    searchKeyword = value;
                    DebounceSearch();
                }
            }
        }

        protected FluentDataGrid<PermissionDto> Grid { get; set; } = default!;
        protected PaginationState pagination = new PaginationState { ItemsPerPage = 10 };
        protected List<PermissionDto> PermissionList { get; set; } = new List<PermissionDto>();
        protected bool IsLoadingSync { get; set; } = false;
        private List<PermissionDto>? PermissionTree;
        protected IEnumerable<TreeViewItemDTO> PemissionTreeItem { get; set; } = new List<TreeViewItemDTO>();
        protected ITreeViewItem? SelectedItem;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                PemissionTreeItem = await LoadPermissionTreeAsync();
                StateHasChanged();
            }
        }

        private void DebounceSearch()
        {
            _debounceTimer?.Stop();
            _debounceTimer = new System.Timers.Timer(1000);
            _debounceTimer.Elapsed += async (_, __) =>
            {
                _debounceTimer?.Stop();
                await InvokeAsync(async () =>
                {
                    PemissionTreeItem = await LoadPermissionTreeAsync();
                    if (Grid != null)
                    {
                        await Grid.RefreshDataAsync();
                    }
                    StateHasChanged();
                });
            };
            _debounceTimer.Start();
        }

        protected async Task<bool> RejectAction(Guid id)
        {
            IsLoadingSync = true;
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/Permission/{id}/Reject"
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
                    Endpoint = $"/Permission/{id}/approve"
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
                ToastService.ShowError($"Lỗi khi duyệt quyền: {ex.Message}");
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
                    Title = "Chỉnh sửa quyền",
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

        protected async Task SyncRole()
        {
            IsLoadingSync = true;
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/Permission/syncpermissionfromenum"
                };
                ResultAPI<string> result = await CallService.Get<string>(apiRequest);

                if (result.Status != StatusCode.OK)
                {
                    ToastService.ShowWarning($"Lỗi: {result.Message ?? "Đã xảy ra lỗi khi thực hiện thao tác"}");
                }
                else
                {
                    ToastService.ShowSuccess("Đồng bộ quyền thành công!");
                    await RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi đồng bộ quyền: {ex.Message}");
            }
            finally
            {
                IsLoadingSync = false;
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
                        MarkupMessage = new MarkupString("Bạn có chắc chắn muốn xóa bản ghi quyền này không?"),
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
                        Endpoint = $"/Permission/{id}"
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
                    Title = "Thông tin chi tiết quyền",
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

        protected async ValueTask<GridItemsProviderResult<PermissionDto>> LoadDatas(GridItemsProviderRequest<PermissionDto> request)
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/Permission/GetPaged"
                };
                var sortProp = request.GetSortByProperties().FirstOrDefault();
                var baseQuery = new BaseQuery
                {
                    draw = 1,
                    SearchIn = new List<string> { "PermissionName" },
                    Keyword = SearchKeyword?.ToLower(),
                    SortBy = sortProp.PropertyName,
                    IsAscending = sortProp.Direction == SortDirection.Ascending,
                    gridRequest = new GridRequest
                    {
                        filter = new Filter(),
                        page = request.StartIndex / pagination.ItemsPerPage + 1,
                        pageSize = pagination.ItemsPerPage,
                    }
                };
                ResultAPI<DataTableJson<PermissionDto>> result = await CallService.Post<DataTableJson<PermissionDto>>(apiRequest, baseQuery);

                if (result.Status == StatusCode.OK && result.Data is DataTableJson<PermissionDto> dataTable)
                {
                    var indexedItems = dataTable.Data.Select((item, idx) =>
                    {
                        item.STT = request.StartIndex + idx + 1;
                        return item;
                    }).ToList();

                    var totalRecords = dataTable.RecordsTotal;
                    return GridItemsProviderResult.From(indexedItems, totalRecords);
                }

                return GridItemsProviderResult.From(new List<PermissionDto>(), 0);
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách quyền: {ex.Message}");
                return GridItemsProviderResult.From(new List<PermissionDto>(), 0);
            }
        }

        protected async Task HandleSearchChanged()
        {
            try
            {
                if (Grid != null)
                {
                    await Grid.RefreshDataAsync();
                }
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
            if (Grid != null)
            {
                await Grid.RefreshDataAsync();
            }
        }

        private async Task<IEnumerable<TreeViewItemDTO>> LoadPermissionTreeAsync()
        {
            var apiRequest = new ApiRequestModel
            {
                ApiService = ServicesRegistryEnum.ServicePortal,
                Endpoint = "/Permission/GetPagedTree"
            };

            var baseQuery = new BaseQuery
            {
                draw = 1,
                SearchIn = new List<string> { "PermissionName" },
                Keyword = SearchKeyword?.ToLower(),
                gridRequest = new GridRequest { page = 1, pageSize = 500 }
            };

            var result = await CallService.Post<DataTableJson<PermissionDto>>(apiRequest, baseQuery);

            PermissionTree = result?.Data?.Data?.ToList() ?? new List<PermissionDto>();
            PemissionTreeItem = ConvertToTreeItems(PermissionTree).ToList();

            return PemissionTreeItem;
        }

        private IEnumerable<TreeViewItemDTO> ConvertToTreeItems(IEnumerable<PermissionDto> permissions)
        {
            return permissions.Select(p =>
            {
                bool isParent = p.ListChild != null && p.ListChild.Any();

                return new TreeViewItemDTO
                {
                    Id = p.Id.ToString(),
                    Text = isParent ? p.PermissionName : $"[{(int)p.PermissionCode}] - {p.PermissionName}",
                    IconCollapsed = isParent
                        ? new Icons.Regular.Size20.Person()
                        : new Icons.Regular.Size20.ClipboardTaskListLtr(),
                    IconExpanded = isParent
                        ? new Icons.Regular.Size20.Person()
                        : new Icons.Regular.Size20.ClipboardTaskListLtr(),
                    Expanded = true,
                    Items = isParent
                        ? ConvertToTreeItems(p.ListChild!).ToList()
                        : new List<TreeViewItemDTO>()
                };
            });
        }
    }
}
