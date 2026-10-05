// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Interfaces.Extentions;
using Service.Shared.Commons.Model.Commons;
using Service.Shared.Commons.Model.ServiceCustomHttpClient;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Querys.Grid;
using Service.Shared.Commons.Querys.ModalQuery;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;
using Service.UI.Blazor.Components.Pages.QuanTriHeThong.Menu;
using Services.Components.Layouts.ShareComponent.SecurePage;

namespace Service.UI.Blazor.Components.Pages.QuanTriHeThong.DonVi
{
    public partial class Index : SecurePageBase
    {
        protected override Guid MenuId { get; } = MenuSecure.QuanLyDonViHeThong;
        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;

        private ColumnKeyGridSort<GroupDto> _roleNameSort = new ColumnKeyGridSort<GroupDto>("Name");
        protected string? SearchKeyword { get; set; } = string.Empty;
        protected string TreeSearchKeyword { get; set; } = string.Empty;
        protected FluentDataGrid<GroupDto> Grid { get; set; } = default!;

        protected PaginationState pagination = new PaginationState { ItemsPerPage = 5 };

        private IEnumerable<TreeViewItemDTO> GroupTreeItem { get; set; } = new List<TreeViewItemDTO>();
        private ITreeViewItem? SelectedItem;
        private bool IsLoadingSync { get; set; } = false;
        private bool IsSyncingClientCache { get; set; } = false;
        private string ActiveTab { get; set; } = "grid";

        private async Task SwitchTab(string tabId)
        {
            ActiveTab = tabId;
            if (ActiveTab == "tree")
            {
                await LoadTreeDataAsync();
            }
            StateHasChanged();
        }

        private async Task LoadTreeDataAsync()
        {
            IsLoadingSync = true;
            try
            {
                if (!IsAuthorized) return;

                // Dùng TreeSearchKeyword riêng, độc lập với SearchKeyword của lưới
                var searchParam = string.IsNullOrWhiteSpace(TreeSearchKeyword) ? "" : Uri.EscapeDataString(TreeSearchKeyword.Trim());
                ApiRequestModel apiRequest = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = $"/Group/get-tree?searchTerm={searchParam}"
                };

                ResultAPI<List<GroupTreeDto>> result = await CallService.Get<List<GroupTreeDto>>(apiRequest);
                if (result.Status == StatusCode.OK && result.Data != null)
                {
                    string? oldSelectedId = (SelectedItem as TreeViewItemDTO)?.Id;
                    GroupTreeItem = ConvertToTreeItems(result.Data);

                    // Mở tất cả node để liệt kê đầy đủ mọi bản ghi
                    SetExpanded(GroupTreeItem.OfType<TreeViewItemDTO>(), true);

                    if (!string.IsNullOrEmpty(oldSelectedId))
                    {
                        SelectedItem = FindItemInTree(GroupTreeItem, oldSelectedId);
                    }
                }
                else
                {
                    ToastService.ShowError(result.Message ?? "Lỗi khi tải cây đơn vị.");
                    GroupTreeItem = new List<TreeViewItemDTO>();
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải cây đơn vị: {ex.Message}");
                GroupTreeItem = new List<TreeViewItemDTO>();
            }
            finally
            {
                IsLoadingSync = false;
                StateHasChanged();
            }
        }

        private IEnumerable<TreeViewItemDTO> ConvertToTreeItems(List<GroupTreeDto>? list)
        {
            if (list is null)
            {
                return Enumerable.Empty<TreeViewItemDTO>();
            }

            return list.Select(x =>
            {
                var hasChildren = x.Children != null && x.Children.Any();
                var item = new TreeViewItemDTO
                {
                    Text = string.IsNullOrWhiteSpace(x.MaGroup) ? x.Name : $"{x.Name} ({x.MaGroup})",
                    Id = x.Id.ToString(),
                    Code = x.MaGroup,
                    ModerationStatus = (int)x.ModerationStatus,
                    Expanded = true,
                    Data = x,
                    IconCollapsed = hasChildren
                        ? new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.Building()
                        : new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.Organization(),
                    IconExpanded = hasChildren
                        ? new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.Building()
                        : new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.Organization(),
                    Items = hasChildren ? ConvertToTreeItems(x.Children).ToList() : null
                };
                return item;
            }).ToList();
        }

        private TreeViewItemDTO? FindItemInTree(IEnumerable<TreeViewItemDTO> items, string targetId)
        {
            if (items == null) return null;

            foreach (var item in items)
            {
                if (item.Id == targetId) return item;

                if (item.Items != null && item.Items.Any())
                {
                    var foundInChildren = FindItemInTree(item.Items.OfType<TreeViewItemDTO>(), targetId);
                    if (foundInChildren != null) return foundInChildren;
                }
            }
            return null;
        }

        private void SelectTreeItem(TreeViewItemDTO item)
        {
            SelectedItem = item;
            StateHasChanged();
        }

        private string GetTextStyle(int status)
        {
            return (ModerationStatus)status switch
            {
                ModerationStatus.Approved => "",
                ModerationStatus.Rejected => "text-decoration: line-through; color: #888; font-style: italic;",
                ModerationStatus.Pending => "color: #e67e22;",
                _ => ""
            };
        }

        private void ExpandAll()
        {
            SetExpanded(GroupTreeItem, true);
            StateHasChanged();
        }

        private void CollapseAll()
        {
            SetExpanded(GroupTreeItem, false);
            StateHasChanged();
        }

        private void SetExpanded(IEnumerable<TreeViewItemDTO>? items, bool expanded)
        {
            if (items == null) return;

            foreach (var treeItem in items)
            {
                treeItem.Expanded = expanded;

                if (treeItem.Items != null && treeItem.Items.Any())
                {
                    SetExpanded(treeItem.Items.OfType<TreeViewItemDTO>(), expanded);
                }
            }
        }

        private async ValueTask<GridItemsProviderResult<GroupDto>> LoadGroup(GridItemsProviderRequest<GroupDto> request)
        {
            IsLoadingSync = true;
            try
            {
                if (!IsAuthorized) { return GridItemsProviderResult.From(new List<GroupDto>(), 0); }
                ApiRequestModel apiRequest = new ApiRequestModel()
                {

                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = "/Group/GetPaged"

                };
                var baseQuery = new GroupQuery
                {
                    draw = 1,
                    SearchIn = new List<string> { "Name", "MaGroup" },
                    Keyword = SearchKeyword?.Trim().ToLower(),
                    sort = request.GetSortByProperties().Select(s => new Sort { field = s.PropertyName, dir = s.Direction == SortDirection.Ascending ? "asc" : "desc" }).ToList(),
                    gridRequest = new GridRequest
                    {
                        filter = new Filter(),
                        sort = request.GetSortByProperties().Select(s => new Sort { field = s.PropertyName, dir = s.Direction == SortDirection.Ascending ? "asc" : "desc" }).ToList(),
                        page = (request.StartIndex / pagination.ItemsPerPage) + 1,
                        pageSize = pagination.ItemsPerPage,

                    }
                };

                ResultAPI<DataTableJson<GroupDto>> result = await CallService.Post<DataTableJson<GroupDto>>(apiRequest, baseQuery);

                if (result.Status == StatusCode.OK && result.Data is DataTableJson<GroupDto>)
                {
                    var dataTable = result.Data;

                    var items = dataTable.Data.ToList();
                    var totalRecords = dataTable.RecordsTotal;

                    return GridItemsProviderResult.From(items, totalRecords);
                }



                return GridItemsProviderResult.From(new List<GroupDto>(), 0);
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách đơn vị: {ex.Message}");
                return GridItemsProviderResult.From(new List<GroupDto>(), 0);
            }
            finally
            {
                IsLoadingSync = false;
                StateHasChanged();
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
                    OnRefresh = EventCallback.Factory.Create(this, RefreshDataAsync),
                };

                await DialogService.ShowDialogAsync<Edit>(parameters, new DialogParameters
                {
                    Title = "Thêm mới đơn vị",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal thêm mới: {ex.Message}");
            }
        }
        protected async Task OpenUpdateModal(Guid id)
        {
            try
            {
                var parameters = new EditOrUpdateParametersDto
                {
                    Id = id,
                    IsEditMode = true,
                    OnRefresh = EventCallback.Factory.Create(this, RefreshDataAsync),
                };

                await DialogService.ShowDialogAsync<Edit>(parameters, new DialogParameters
                {
                    Title = "Sửa đơn vị",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal sửa: {ex.Message}");
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
                    Title = "Thông tin chi tiết đơn vị",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    PrimaryAction = null,
                    SecondaryAction = null
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal chi tiết: {ex.Message}");
            }
        }
        private async Task OpenModalClient(Guid id, string tenDonVi)
        {
            try
            {
                // Truyền thêm tên đơn vị để hiển thị trong modal
                var parameters = new EditOrUpdateParametersDto
                {
                    Id = id,
                    OnRefresh = EventCallback.Factory.Create(this, RefreshDataAsync),
                    Parameter = tenDonVi
                };
                var dialog = await DialogService.ShowDialogAsync<Client.Index>(parameters, new DialogParameters
                {
                    Title = "Danh sách Client - " + tenDonVi,
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "950px",
                    TrapFocus = false
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal: {ex.Message}");
            }
        }
        protected async Task OpenModalDelete(Guid id)
        {
            try
            {
                var confirmDialog = await DialogService.ShowDialogAsync<GroupDeleteConfirm>(
                    new GroupDeleteConfirm.Request
                    {
                        Title = "Xác nhận xóa đơn vị",
                        Message = "Xóa đơn vị sẽ xóa vĩnh viễn toàn bộ dữ liệu liên quan, bao gồm các đơn vị con."
                    }, new DialogParameters());

                var confirmResult = await confirmDialog.Result;

                if (confirmResult.Cancelled)
                    return;

                if (confirmResult.Data is not bool success || !success)
                    return;

                ApiRequestModel apiRequest = new()
                {
                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = $"/Group/{id}"
                };

                var result = await CallService.Delete(apiRequest);
                if (result.Status != StatusCode.OK)
                {
                    ToastService.ShowError(result.Message);
                    return;
                }

                if (result.Status == StatusCode.OK)
                {
                    ToastService.ShowSuccess("Xóa đơn vị thành công!");
                    await RefreshDataAsync();
                }

            }
            catch (Exception ex)
            {
                ToastService.ShowError(ex.Message);
            }
        }

        protected async Task HandleSearchChanged()
        {
            try
            {
                await RefreshDataAsync();
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tìm kiếm: {ex.Message}");
            }
        }

        protected async Task HandleTreeSearchKeyUp(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await SearchTree();
            }
        }

        protected async Task SearchTree()
        {
            try
            {
                await LoadTreeDataAsync();
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tìm kiếm trên cây: {ex.Message}");
            }
        }

        public async Task RejectAsync(Guid id)
        {
            IsLoadingSync = true;
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {

                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = $"/Group/{id}/Reject"

                };

                ResultAPI result = await CallService.Put(apiRequest, new object());
                if (result.Status == StatusCode.OK)
                {
                    ToastService.ShowSuccess("Huỷ duyệt đơn vị thành công");
                    await RefreshDataAsync();
                }
                else
                {
                    ToastService.ShowError("Huỷ duyệt đơn vị thất bại!  " + result.Message);
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError("Huỷ duyệt đơn vị thất bại!  " + ex.Message);
            }
            finally
            {
                IsLoadingSync = false;
            }


        }
        public async Task ApproveAsync(Guid id)
        {
            IsLoadingSync = true;
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {

                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = $"/Group/{id}/Approve"

                };
                ResultAPI result = await CallService.Put(apiRequest, new object());
                if (result.Status == StatusCode.OK)
                {
                    ToastService.ShowSuccess("Duyệt đơn vị thành công");
                    await RefreshDataAsync();

                }
                else
                {
                    ToastService.ShowError("Duyệt đơn vị thất bại!  " + result.Message);
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError("Duyệt đơn vị thất bại!  " + ex.Message);

            }
            finally
            {
                IsLoadingSync = false;
            }



        }

        /// <summary>
        /// Đồng bộ lại toàn bộ client đã duyệt vào cache Redis (OAuthClients),
        /// dùng khi cache lệch với DB mà không muốn đợi restart app.
        /// </summary>
        protected async Task SyncClientCacheAsync()
        {
            if (IsSyncingClientCache) return;

            IsSyncingClientCache = true;
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = "/Client/sync-cache"
                };

                ResultAPI<int> result = await CallService.Post<int>(apiRequest, new object());

                if (result.Status == StatusCode.OK)
                {
                    ToastService.ShowSuccess($"Đã đồng bộ {result.Data} client đã duyệt vào cache.");
                }
                else
                {
                    ToastService.ShowError("Đồng bộ cache client thất bại! " + result.Message);
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError("Đồng bộ cache client thất bại! " + ex.Message);
            }
            finally
            {
                IsSyncingClientCache = false;
            }
        }

        private async Task RefreshData(int value)
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
        private async Task RefreshDataAsync()
        {
            if (Grid != null)
            {
                await Grid.RefreshDataAsync();
            }
            if (ActiveTab == "tree" || (GroupTreeItem != null && GroupTreeItem.Any()))
            {
                await LoadTreeDataAsync();
            }
        }
    }
}
