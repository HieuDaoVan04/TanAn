// "Một sản phẩm của HieuDV"

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;
using Service.UI.CMS.Blazor.Components.Layout.Component;
using Service.UI.CMS.Blazor.Components.Shared;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.Group
{
    public partial class Index : ComponentBase
    {
        [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
        [Inject] private IToastService ToastService { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        private List<GroupsDto> GroupTree { get; set; } = new();
        private string? SearchKeyword { get; set; }
        private IEnumerable<TreeViewItemDTO>? Items = new List<TreeViewItemDTO>();
        public bool CanEditOrDelete { get; set; }
        public bool CanReject { get; set; }
        public bool CanCreated { get; set; } = true;
        Guid IdTreeViewSelected;
        private GroupsDto? SelectedGroup;

        #region Bulk Selection
        private HashSet<(Guid UserId, Guid PhongBanId)> SelectedUsers { get; set; } = new();
        private bool IsAllSelected { get; set; }
        private List<UserDto> CurrentPageUsers { get; set; } = new();

        private bool GetIsSelected(Guid userId, Guid phongBanId)
        {
            return SelectedUsers.Contains((userId, phongBanId));
        }

        private void ToggleUserSelection(Guid userId, Guid phongBanId, bool isSelected)
        {
            var userPair = (userId, phongBanId);

            if (isSelected)
            {
                SelectedUsers.Add(userPair);
            }
            else
            {
                SelectedUsers.Remove(userPair);
            }

            UpdateSelectAllState();
            StateHasChanged();
        }

        private void UpdateSelectAllState()
        {
            if (CurrentPageUsers.Any())
            {
                IsAllSelected = CurrentPageUsers.All(user =>
                    SelectedUsers.Contains((user.Id, user.PhongBanId)));
            }
            else
            {
                IsAllSelected = false;
            }
        }

        private async Task BulkAssignRoles()
        {
            if (SelectedUsers.Any())
            {
                try
                {
                    var parameters = new GanVaiTroParametersDto
                    {
                        OnRefresh = EventCallback.Factory.Create(this, RefreshTreeAndGrid),
                        ListUser = SelectedUsers
                    };
                    await DialogService.ShowDialogAsync<ModalBulkAssignRoles>(parameters, new DialogParameters
                    {
                        Title = "Gán vai trò cho người dùng",
                        PreventDismissOnOverlayClick = true,
                        PreventScroll = true,
                        Modal = true,
                        Width = "1150px",
                        TrapFocus = false
                    });
                }
                catch (Exception ex)
                {
                    ToastService.ShowError($"Lỗi khi mở modal: {ex.Message}");
                }
            }
        }

        private void ClearSelection()
        {
            SelectedUsers.Clear();
            IsAllSelected = false;
            StateHasChanged();
        }

        #endregion

        #region tree menu

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Items = await LoadGroupsTreeAsync();
                StateHasChanged();
            }
        }

        private GroupsDto? FindGroupInTree(IEnumerable<GroupsDto> groups, string id)
        {
            if (groups == null) return null;

            foreach (var group in groups)
            {
                if (group.Id.ToString() == id)
                    return group;

                if (group.ListChild != null && group.ListChild.Any())
                {
                    var found = FindGroupInTree(group.ListChild, id);
                    if (found != null) return found;
                }
            }
            return null;
        }

        protected async Task OpenAddModal()
        {
            try
            {
                var parameters = new EditOrUpdateParametersGroupDto
                {
                    Id = Guid.Empty,
                    IsEditMode = false,
                    OnRefresh = EventCallback.Factory.Create(this, RefreshTreeAndGrid)
                };
                if (SelectedGroup is not null)
                {
                    parameters.IsAddGroup = false;
                    parameters.Object = SelectedGroup;
                }
                await DialogService.ShowDialogAsync<Edit>(parameters, new DialogParameters
                {
                    Title = parameters.IsAddGroup ? "Thêm mới đơn vị" : "Thêm mới phòng ban",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "1000px"
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal thêm mới: {ex.Message}");
            }
        }

        private async Task SelectChange(ITreeViewItem selectedItem)
        {
            if (selectedItem is null)
            {
                SelectedGroup = null;
                CanEditOrDelete = false;
                CanReject = false;
                CanCreated = true;
                return;
            }

            SelectedGroup = FindGroupInTree(GroupTree, selectedItem.Id);

            CanEditOrDelete = false;
            CanReject = false;
            CanCreated = true;

            if (SelectedGroup is not null)
            {
                if (SelectedGroup.ModerationStatus == ModerationStatus.Approved)
                    CanReject = true;

                if (SelectedGroup.ModerationStatus != ModerationStatus.Approved)
                    CanEditOrDelete = true;

                if (SelectedGroup.UnitType == OrganizationUnitType.PhongBan)
                    CanCreated = false;
            }

            StateHasChanged();

            if (Guid.TryParse(selectedItem.Id, out Guid groupId))
            {
                IdTreeViewSelected = groupId;
                await Grid.RefreshDataAsync();
            }
        }

        protected async Task EditAction()
        {
            try
            {
                var parameters = new EditOrUpdateParametersGroupDto
                {
                    Id = IdTreeViewSelected,
                    IsEditMode = true,
                    OnRefresh = EventCallback.Factory.Create(this, RefreshTreeAndGrid)
                };
                if (SelectedGroup is not null)
                {
                    parameters.IsAddGroup = false;
                    parameters.Object = SelectedGroup;
                }
                await DialogService.ShowDialogAsync<Edit>(parameters, new DialogParameters
                {
                    Title = parameters.IsAddGroup ? "Chỉnh sửa đơn vị" : "Chỉnh sửa phòng ban",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "1000px"
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal sửa: {ex.Message}");
            }
        }

        protected async Task OpenDetailsModal()
        {
            if (SelectedGroup == null)
            {
                ToastService.ShowWarning("Vui lòng chọn bản ghi cần xem");
                return;
            }

            try
            {
                var parameters = new EditOrUpdateParametersDto
                {
                    Id = SelectedGroup.Id,
                };
                await DialogService.ShowDialogAsync<View>(parameters, new DialogParameters
                {
                    Title = "Thông tin chi tiết ",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "1000px",
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal chi tiết: {ex.Message}");
            }
        }

        protected async Task OpenModalDelete()
        {
            try
            {
                var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
                {
                    Content = new MessageBoxContent
                    {
                        Title = "Xác nhận xóa",
                        MarkupMessage = new MarkupString("Bạn có chắc chắn muốn xóa đơn vị này không?"),
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
                        Endpoint = $"/Groups/{IdTreeViewSelected}"
                    };
                    var result = await CallService.Delete(apiRequest);
                    if (result.Status == StatusCode.OK)
                    {
                        await RefreshTreeAndGrid();
                        ToastService.ShowSuccess("Xóa đơn vị thành công!");
                    }
                    else
                    {
                        ToastService.ShowError(result.Message ?? "Lỗi khi xóa đơn vị.");
                    }
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi xóa đơn vị: {ex.Message}");
            }
        }

        protected async Task<bool> ApproveAction()
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/Groups/{IdTreeViewSelected}/approve"
                };

                ResultAPI<object> result = await CallService.Put<object>(apiRequest, new object());
                if (result.Status == StatusCode.OK)
                {
                    await RefreshTreeAndGrid();
                    ToastService.ShowSuccess("Duyệt đơn vị thành công!");
                    return true;
                }
                else
                {
                    throw new Exception(result.Message ?? "Lỗi khi duyệt đơn vị.");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi duyệt đơn vị: {ex.Message}");
                return false;
            }
        }

        protected async Task<bool> RejectAction()
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/Groups/{IdTreeViewSelected}/Reject"
                };

                ResultAPI<object> result = await CallService.Put<object>(apiRequest, new object());
                if (result.Status == StatusCode.OK)
                {
                    await RefreshTreeAndGrid();
                    ToastService.ShowSuccess("Hủy duyệt đơn vị thành công!");
                    return true;
                }
                else
                {
                    throw new Exception(result.Message ?? "Lỗi khi hủy duyệt đơn vị.");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi hủy duyệt đơn vị: {ex.Message}");
                return false;
            }
        }

        private async Task RefreshTreeAndGrid()
        {
            Items = null;
            StateHasChanged();

            Items = await LoadGroupsTreeAsync();

            CanEditOrDelete = false;
            CanReject = false;
            CanCreated = true;
            SelectedGroup = null;

            if (Grid != null)
            {
                await Grid.RefreshDataAsync();
            }
            StateHasChanged();
        }

        private async Task HandleSearchTreeChanged(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await RefreshTreeAndGrid();
            }
        }

        private async Task<IEnumerable<TreeViewItemDTO>> LoadGroupsTreeAsync()
        {
            try
            {
                var apiRequest = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/Groups/getpagedtree"
                };

                var baseQuery = new BaseQuery
                {
                    draw = 1,
                    SearchIn = new List<string> { "Name", "ShortName", "IdentifierCode" },
                    Keyword = SearchKeyword?.ToLower(),
                    gridRequest = new GridRequest
                    {
                        filter = new Filter(),
                        page = 1,
                        pageSize = 500
                    }
                };

                var result = await CallService.Post<DataTableJson<GroupsDto>>(apiRequest, baseQuery);

                if (result.Status == StatusCode.OK && result.Data != null)
                {
                    GroupTree = result.Data.Data?.ToList() ?? new List<GroupsDto>();
                }
                else
                {
                    GroupTree = new List<GroupsDto>();
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách nhóm: {ex.Message}");
                GroupTree = new List<GroupsDto>();
            }

            return ConvertToTreeItems(GroupTree);
        }

        private IEnumerable<TreeViewItemDTO> ConvertToTreeItems(IEnumerable<GroupsDto> groups)
        {
            if (groups == null) return Enumerable.Empty<TreeViewItemDTO>();

            var result = groups.Select(x =>
            {
                bool isPhongBan = x.UnitType == OrganizationUnitType.PhongBan;

                string text = x.Name;

                var groupItem = new TreeViewItemDTO
                {
                    Id = x.Id.ToString(),
                    Text = text,
                    Type = isPhongBan ? 1 : 2,
                    ModerationStatus = (int)x.ModerationStatus,
                    IconCollapsed = isPhongBan
                        ? new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.PeopleTeam()
                        : new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.Folder(),
                    IconExpanded = isPhongBan
                        ? new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.PeopleTeam()
                        : new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.FolderOpen(),
                    Expanded = true,
                    Items = x.ListChild != null && x.ListChild.Any()
                        ? ConvertToTreeItems(x.ListChild).ToList()
                        : new List<TreeViewItemDTO>()
                };

                return groupItem;
            }).ToList();

            return result;
        }

        #endregion tree menu

        #region grid user

        protected FluentDataGrid<UserDto> Grid { get; set; } = default!;
        private PaginationState pagination = new PaginationState { ItemsPerPage = 10 };
        private string userSearchKeyword = string.Empty;

        private async ValueTask<GridItemsProviderResult<UserDto>> LoadDatas(GridItemsProviderRequest<UserDto> request)
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/Users/getpagedbygroupid"
                };

                var baseQuery = new UserQuery
                {
                    GroupId = IdTreeViewSelected,
                    draw = 1,
                    SearchIn = new List<string> { "UserName", "FullName" },
                    Keyword = userSearchKeyword,
                    gridRequest = new GridRequest
                    {
                        filter = new Filter(),
                        page = request.StartIndex / pagination.ItemsPerPage + 1,
                        pageSize = pagination.ItemsPerPage,
                    }
                };

                ResultAPI<DataTableJson<UserDto>> result =
                    await CallService.Post<DataTableJson<UserDto>>(apiRequest, baseQuery);

                if (result.Status == StatusCode.OK
                    && result.Data is DataTableJson<UserDto> dataTable
                    && dataTable.Data != null)
                {
                    var indexedItems = dataTable.Data.Select((item, idx) =>
                    {
                        item.Index = request.StartIndex + idx + 1;
                        return item;
                    }).ToList();

                    return GridItemsProviderResult.From(indexedItems, dataTable.RecordsTotal);
                }
                return GridItemsProviderResult.From(new List<UserDto>(), 0);
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách người dùng: {ex.Message}");
                return GridItemsProviderResult.From(new List<UserDto>(), 0);
            }
        }

        private async Task HandleSearchChanged(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Grid.RefreshDataAsync();
            }
        }

        private async Task RefreshData(int value)
        {
            try
            {
                pagination.ItemsPerPage = value;
                await pagination.SetCurrentPageIndexAsync(0);
                ClearSelection();
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách: {ex.Message}");
            }
        }

        private async Task OpenAddUserModal()
        {
            if (SelectedGroup == null)
            {
                ToastService.ShowWarning("Vui lòng chọn đơn vị trước khi thêm người dùng");
                return;
            }

            try
            {
                var parameters = new EditOrUpdateParametersGroupDto
                {
                    Id = Guid.Empty,
                    IsEditMode = false,
                    OnRefresh = EventCallback.Factory.Create(this, RefreshTreeAndGrid)
                };
                if (SelectedGroup is not null) parameters.Object = SelectedGroup;
                await DialogService.ShowDialogAsync<ModalThemNguoIDungVaoPhongBan>(parameters, new DialogParameters
                {
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "1200px",
                    OnDialogClosing = EventCallback.Factory.Create<DialogInstance>(this, async (dialog) =>
                    {
                        await RefreshTreeAndGrid();
                    })
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal thêm người dùng: {ex.Message}");
            }
        }

        private async Task OpenDetailsModal(Guid userId, Guid phongBanId)
        {
            try
            {
                var parameters = new ViewParametersDto
                {
                    Id = userId,
                    GroupId = phongBanId
                };

                await DialogService.ShowDialogAsync<InfoUser>(parameters, new DialogParameters
                {
                    Title = "Thông tin chi tiết người dùng",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "1000px"
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal chi tiết: {ex.Message}");
            }
        }

        private async Task OpenModalXoaNguoiDungKhoiPhongBan(Guid userId, Guid? PhongBanId)
        {
            try
            {
                var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
                {
                    Content = new MessageBoxContent
                    {
                        Title = "Xác nhận gỡ người dùng",
                        MarkupMessage = new MarkupString("Bạn có chắc chắn muốn gỡ người dùng khỏi phòng ban này không?"),
                        Icon = new Icons.Regular.Size24.Warning(),
                        IconColor = Color.Warning,
                    },
                    PrimaryAction = "Gỡ",
                    SecondaryAction = "Hủy",
                });

                var result = await dialog.Result;
                if (!result.Cancelled)
                {
                    var apiRequest = new ApiRequestModel
                    {
                        ApiService = ServicesRegistryEnum.ServicePortal,
                        Endpoint = $"/Users/RemoveFromGroup/{userId}/{PhongBanId}"
                    };

                    var deleteResult = await CallService.Delete(apiRequest);
                    if (deleteResult.Status == StatusCode.OK)
                    {
                        ToastService.ShowSuccess("Xóa người dùng khỏi đơn vị thành công!");
                        await RefreshTreeAndGrid();
                    }
                    else
                    {
                        ToastService.ShowError($"Lỗi khi xóa người dùng: {deleteResult.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi xóa người dùng: {ex.Message}");
            }
        }

        #endregion grid user

        #region Gán vai trò
        private async Task OpenModalGanVaiTro(Guid UserId, Guid PhongBanId)
        {
            try
            {
                if (PhongBanId == Guid.Empty)
                {
                    ToastService.ShowWarning("Không xác định được phòng ban của người dùng");
                }
                else
                {
                    var parameters = new GanVaiTroParametersDto
                    {
                        UserId = UserId,
                        PhongBanId = PhongBanId,
                        OnRefresh = EventCallback.Factory.Create(this, RefreshTreeAndGrid),
                    };
                    await DialogService.ShowDialogAsync<ModalGanVaiTro>(parameters, new DialogParameters
                    {
                        Title = "Danh sách vai trò",
                        PreventDismissOnOverlayClick = true,
                        PreventScroll = true,
                        Modal = true,
                        Width = "1150px",
                        TrapFocus = false
                    });
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal: {ex.Message}");
            }
        }
        #endregion

        #region copy role
        private UserDto? CopiedUser { get; set; }

        private void CopyUser(UserDto user)
        {
            CopiedUser = user;
            ToastService.ShowSuccess($"Đã copy thông tin của {user.FullName}");
            StateHasChanged();
        }

        private void ClearCopiedUser()
        {
            CopiedUser = null;
            StateHasChanged();
        }

        private async Task AssignFromCopiedUser(UserDto targetUser)
        {
            if (CopiedUser == null)
            {
                ToastService.ShowWarning("Không có thông tin người dùng nào được copy");
                return;
            }

            try
            {
                var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
                {
                    Content = new MessageBoxContent
                    {
                        Title = "Xác nhận gán vai trò",
                        MarkupMessage = new MarkupString($"Bạn chắc chắn muốn gán quyền cho người dùng này không?<br/>Hành động này sẽ gán toàn bộ quyền của <b>{CopiedUser.FullName}</b> sang người dùng này và mất mọi quyền sử dụng ứng dụng trước đó."),
                        Icon = new Icons.Regular.Size24.Warning(),
                        IconColor = Color.Warning,
                    },
                    PrimaryAction = "Xác nhận",
                    SecondaryAction = "Hủy",
                });
                var result = await dialog.Result;
                if (!result.Cancelled)
                {
                    var apiRequest = new ApiRequestModel
                    {
                        ApiService = ServicesRegistryEnum.ServicePortal,
                        Endpoint = "/PhanQuyen/CopyUserInfo"
                    };

                    var copyRequest = new CopyUserInfoDto
                    {
                        SourceUserId = CopiedUser.Id,
                        TargetUserId = targetUser.Id,
                    };

                    var copyResult = await CallService.Post<object>(apiRequest, copyRequest);
                    if (copyResult.Status == StatusCode.OK)
                    {
                        ToastService.ShowSuccess($"Đã gán thông tin từ {CopiedUser.FullName} cho {targetUser.FullName}");
                        await Grid.RefreshDataAsync();
                    }
                    else
                    {
                        ToastService.ShowError($"Lỗi khi gán thông tin: {copyResult.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi gán thông tin: {ex.Message}");
            }
        }
        #endregion
    }
}
