// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.Group
{
    public partial class ModalGanVaiTro : ComponentBase
    {
        #region Parameters & Injection

        [CascadingParameter]
        public FluentDialog Dialog { get; set; } = default!;

        [Parameter]
        public GanVaiTroParametersDto Content { get; set; } = new();

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;

        [Inject]
        private IToastService ToastService { get; set; } = default!;

        #endregion

        #region Properties

        private IQueryable<ChuyenTrangDto> DanhSachChuyenTrang = Enumerable.Empty<ChuyenTrangDto>().AsQueryable();
        private IQueryable<RoleDto> DanhSachVaiTro = Enumerable.Empty<RoleDto>().AsQueryable();
        private ChuyenTrangDto? SelectedChuyenTrang;
        public bool IsSaving { get; set; }

        private List<ITreeViewItem> ChuyenMucTreeData = new();
        private List<TinTucChuyenMucDto> DanhSachChuyenMuc = new();
        private HashSet<Guid> SelectedChuyenMucIds = new();
        private bool IsAllChuyenMucSelected => DanhSachChuyenMuc.Any() &&
           DanhSachChuyenMuc.All(cm => SelectedChuyenMucIds.Contains(cm.Id));

        private bool IsAllVaiTroSelected => DanhSachVaiTro.Any() &&
            DanhSachVaiTro.All(vt => vt.DaGan);

        private void ToggleAllChuyenMuc()
        {
            if (IsAllChuyenMucSelected)
            {
                SelectedChuyenMucIds.Clear();
            }
            else
            {
                SelectedChuyenMucIds = DanhSachChuyenMuc.Select(cm => cm.Id).ToHashSet();
            }

            StateHasChanged();
        }

        private void ToggleAllVaiTro()
        {
            var selectAll = !IsAllVaiTroSelected;

            foreach (var vaiTro in DanhSachVaiTro)
            {
                vaiTro.DaGan = selectAll;
            }

            StateHasChanged();
        }
        #endregion

        #region Lifecycle

        protected override async Task OnInitializedAsync()
        {
            await LoadChuyenTrangData();
        }

        #endregion

        #region Data Loading

        private async Task LoadChuyenTrangData()
        {
            try
            {
                var apiRequest = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/ChuyenTrang/GetByUser"
                };

                var query = new ChuyenTrangQuery
                {
                    UserId = Content.UserId,
                    gridRequest = new GridRequest
                    {
                        page = 1,
                        pageSize = int.MaxValue
                    }
                };

                var result = await CallService.Post<List<ChuyenTrangDto>>(apiRequest, query);

                if (result.Status == StatusCode.OK && result.Data != null)
                {
                    DanhSachChuyenTrang = result.Data.AsQueryable();
                    SelectedChuyenTrang = DanhSachChuyenTrang.FirstOrDefault();

                    if (SelectedChuyenTrang != null)
                    {
                        await LoadChuyenMucData();
                        await LoadAndMergeVaiTroData();
                    }
                }
                else
                {
                    DanhSachChuyenTrang = new List<ChuyenTrangDto>().AsQueryable();
                    SelectedChuyenTrang = null;
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách chuyên trang: {ex.Message}");
            }
        }

        private async Task LoadChuyenMucData()
        {
            if (SelectedChuyenTrang is null) return;

            try
            {
                var apiRequest = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/TinTucChuyenMuc/LayDanhSachForPhanQuyen"
                };

                var query = new TinTucChuyenMucQuery
                {
                    ChuyenTrangId = SelectedChuyenTrang.Id,
                    UserId = Content.UserId,
                    gridRequest = new GridRequest
                    {
                        page = 1,
                        pageSize = int.MaxValue
                    }
                };

                var result = await CallService.Post<List<TinTucChuyenMucDto>>(apiRequest, query);

                if (result.Status == StatusCode.OK && result.Data != null)
                {
                    DanhSachChuyenMuc = result.Data;

                    ChuyenMucTreeData = BuildChuyenMucTree(DanhSachChuyenMuc).Cast<ITreeViewItem>().ToList();
                    SelectedChuyenMucIds = DanhSachChuyenMuc.Where(x => x.DaGan).Select(x => x.Id).ToHashSet();
                }
                else
                {
                    DanhSachChuyenMuc = new List<TinTucChuyenMucDto>();
                    ChuyenMucTreeData = new List<ITreeViewItem>();
                    SelectedChuyenMucIds = new HashSet<Guid>();
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách chuyên mục: {ex.Message}");
            }
        }

        private async Task LoadAndMergeVaiTroData()
        {
            if (SelectedChuyenTrang is null) return;

            try
            {
                var apiVaiTro = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/Role/GetPaged"
                };

                var roleQuery = new RoleQuery
                {
                    gridRequest = new GridRequest
                    {
                        page = 1,
                        pageSize = int.MaxValue
                    },
                    ModerationStatus = 0
                };

                var allRolesResult = await CallService.Post<DataTableJson<RoleDto>>(apiVaiTro, roleQuery);
                var allRoles = allRolesResult.Status == StatusCode.OK && allRolesResult.Data != null
                    ? allRolesResult.Data.Data?.ToList() ?? new List<RoleDto>()
                    : new List<RoleDto>();

                var apiGanVaiTro = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/PhanQuyen/LayDanhSachVaiTroDaPhan"
                };

                var ganQuery = new TinTucChuyenMucQuery
                {
                    ChuyenTrangId = SelectedChuyenTrang.Id,
                    UserId = Content.UserId,
                    gridRequest = new GridRequest
                    {
                        page = 1,
                        pageSize = int.MaxValue
                    }
                };

                var assignedRolesResult = await CallService.Post<List<RoleDto>>(apiGanVaiTro, ganQuery);
                var assignedRoleIds = assignedRolesResult.Status == StatusCode.OK && assignedRolesResult.Data != null
                    ? assignedRolesResult.Data.Select(r => r.Id).ToHashSet()
                    : new HashSet<Guid>();

                var merged = allRoles
                    .Select((role, index) => new RoleDto
                    {
                        Id = role.Id,
                        RoleName = role.RoleName,
                        RoleCode = role.RoleCode,
                        STT = index + 1,
                        DaGan = assignedRoleIds.Contains(role.Id)
                    })
                    .ToList();

                DanhSachVaiTro = merged.AsQueryable();
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách vai trò: {ex.Message}");
            }
        }

        #endregion

        #region Tree Building

        private List<TreeViewItemDTO> BuildChuyenMucTree(IEnumerable<TinTucChuyenMucDto> allChuyenMuc, Guid? parentId = null)
        {
            return allChuyenMuc
                .Where(c => c.ChaId == parentId)
                .OrderBy(c => c.Ten)
                .Select(c =>
                {
                    var children = BuildChuyenMucTree(allChuyenMuc, c.Id);

                    return new TreeViewItemDTO
                    {
                        Id = c.Id.ToString(),
                        Text = c.Ten,
                        IconCollapsed = children.Any()
                            ? new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.FolderOpen()
                            : new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.Document(),
                        IconExpanded = children.Any()
                            ? new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.FolderOpen()
                            : new Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20.Document(),
                        Expanded = true,
                        Items = children
                    };
                })
                .ToList();
        }

        #endregion

        #region Event Handlers

        private async Task OnChuyenTrangChanged(ChuyenTrangDto chuyenTrang)
        {
            SelectedChuyenTrang = chuyenTrang;

            DanhSachChuyenMuc = new List<TinTucChuyenMucDto>();
            ChuyenMucTreeData = new List<ITreeViewItem>();
            SelectedChuyenMucIds = new HashSet<Guid>();
            DanhSachVaiTro = Enumerable.Empty<RoleDto>().AsQueryable();

            await LoadChuyenMucData();
            await LoadAndMergeVaiTroData();

            StateHasChanged();
        }

        private void OnChuyenMucCheckedChanged(string nodeId, bool isChecked)
        {
            if (!Guid.TryParse(nodeId, out var chuyenMucId)) return;

            if (isChecked)
            {
                SelectedChuyenMucIds.Add(chuyenMucId);
                SelectAllChildren(nodeId, true);
                SelectParentIfAllSiblingsSelected(chuyenMucId);
            }
            else
            {
                SelectedChuyenMucIds.Remove(chuyenMucId);
                SelectAllChildren(nodeId, false);
                UnselectParents(chuyenMucId);
            }

            StateHasChanged();
        }

        private void SelectAllChildren(string parentId, bool isSelected)
        {
            var parentNode = FindNodeById(ChuyenMucTreeData, parentId);
            if (parentNode?.Items != null)
            {
                foreach (var child in parentNode.Items.Cast<TreeViewItemDTO>())
                {
                    if (Guid.TryParse(child.Id, out var childId))
                    {
                        if (isSelected)
                        {
                            SelectedChuyenMucIds.Add(childId);
                        }
                        else
                        {
                            SelectedChuyenMucIds.Remove(childId);
                        }

                        SelectAllChildren(child.Id, isSelected);
                    }
                }
            }
        }

        private void SelectParentIfAllSiblingsSelected(Guid childId)
        {
            var child = DanhSachChuyenMuc.FirstOrDefault(x => x.Id == childId);
            if (child?.ChaId != null)
            {
                var parentId = child.ChaId.Value;

                var siblings = DanhSachChuyenMuc.Where(x => x.ChaId == parentId).ToList();

                var allSiblingsSelected = siblings.All(s => SelectedChuyenMucIds.Contains(s.Id));

                if (allSiblingsSelected && !SelectedChuyenMucIds.Contains(parentId))
                {
                    SelectedChuyenMucIds.Add(parentId);

                    SelectParentIfAllSiblingsSelected(parentId);
                }
            }
        }

        private void UnselectParents(Guid childId)
        {
            var child = DanhSachChuyenMuc.FirstOrDefault(x => x.Id == childId);
            if (child?.ChaId != null)
            {
                var parentId = child.ChaId.Value;

                if (SelectedChuyenMucIds.Contains(parentId))
                {
                    SelectedChuyenMucIds.Remove(parentId);

                    UnselectParents(parentId);
                }
            }
        }

        private TreeViewItemDTO? FindNodeById(IEnumerable<ITreeViewItem> items, string id)
        {
            foreach (var item in items.Cast<TreeViewItemDTO>())
            {
                if (item.Id == id)
                    return item;

                if (item.Items != null)
                {
                    var found = FindNodeById(item.Items, id);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        private void OnVaiTroCheckedChanged(RoleDto vaiTro, bool isChecked)
        {
            vaiTro.DaGan = isChecked;
        }

        private async Task OpenPhanChuyenTrangModal()
        {
            try
            {
                var parameters = new EditOrUpdateModuleParametersDto
                {
                    ParameterGuid = Content.UserId,
                    PhongBanId = Content.PhongBanId,
                    OnRefresh = EventCallback.Factory.Create(this, LoadChuyenTrangData),
                };

                await DialogService.ShowDialogAsync<ModalPhanChuyenTrang>(parameters, new DialogParameters
                {
                    Title = "Phân chuyên trang",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "800px",
                    TrapFocus = false
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal: {ex.Message}");
            }
        }

        #endregion

        #region Submit & Actions

        private async Task LuuPhanQuyen()
        {
            if (SelectedChuyenTrang is null)
            {
                ToastService.ShowError("Vui lòng chọn chuyên trang.");
                return;
            }

            IsSaving = true;

            try
            {
                var phanQuyenDto = new PhanQuyenNguoiDungDto
                {
                    UserId = Content.UserId,
                    ChuyenTrangId = SelectedChuyenTrang.Id,
                    ChuyenMucIds = SelectedChuyenMucIds.ToList(),
                    VaiTros = DanhSachVaiTro
                        .Where(x => x.DaGan)
                        .Select(v => new VaiTroDto
                        {
                            Id = v.Id,
                            TenVaiTro = v.RoleName,
                            VaiTroCode = v.RoleCode,
                        })
                        .ToList()
                };

                var apiRequest = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/PhanQuyen/LuuPhanQuyen"
                };

                var response = await CallService.Post<object>(apiRequest, phanQuyenDto);

                if (response.Status == StatusCode.OK)
                {
                    ToastService.ShowSuccess("Phân quyền thành công");
                    await Dialog.CloseAsync();
                }
                else
                {
                    var errorMessage = response.Message ?? "Phân quyền thất bại";
                    ToastService.ShowError(errorMessage);
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi phân quyền: {ex.Message}");
            }
            finally
            {
                IsSaving = false;

                if (Content.OnRefresh.HasDelegate)
                    await Content.OnRefresh.InvokeAsync();
            }
        }

        private async Task HideDialog()
        {
            await Dialog.CloseAsync();
        }

        #endregion
    }
}
