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
using PermissionDto = Service.Shared.Contracts.DTOs.PermissionDto;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.Group
{
    public partial class ModalBulkAssignRoles : ComponentBase
    {
        [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
        [Parameter] public GanVaiTroParametersDto Content { get; set; } = new();
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
        [Inject] private IToastService ToastService { get; set; } = default!;

        private List<RoleDto> DanhSachVaiTro = new List<RoleDto>();
        private IEnumerable<TreeViewItemDTO> PemissionTreeItem { get; set; } = new List<TreeViewItemDTO>();
        private List<PermissionDto>? PermissionTree;

        private HashSet<Guid> PermissionUserSelected = new();
        private HashSet<Guid> PermissionFromRole = new();
        private HashSet<Guid> AllSelectedPermissions => PermissionUserSelected.Union(PermissionFromRole).ToHashSet();

        public bool IsSaving { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadRoleData();
            PemissionTreeItem = await LoadPermissionTreeAsync();
        }

        #region submit
        private async Task GanQuyenChoNguoiDung()
        {
            try
            {
                IsSaving = true;

                var roleIds = DanhSachVaiTro
                        .Where(x => x.DaGan)
                        .Select(x => x.Id)
                        .ToList();
                var form = new GanVaiTroVaoNguoiDungDto
                {
                    PhongBanId = Content.ListUser.FirstOrDefault().PhongBanId,
                    RoleIds = roleIds,
                    LstUserIds = Content.ListUser.Select(e => e.UserId).Distinct().ToList(),
                    PermissionIds = AllSelectedPermissions.ToList()
                };

                var request = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/Users/BulkGanVaiTro"
                };
                var response = await CallService.Post<object>(request, form);

                if (response.Status == StatusCode.OK)
                {
                    ToastService.ShowSuccess("Gán vai trò thành công");
                    await Dialog.CloseAsync();
                }
                else
                {
                    ToastService.ShowError("Gán vai trò thất bại");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi gán quyền: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
                await Content.OnRefresh.InvokeAsync();
            }
        }
        #endregion

        #region vai trò
        private async Task LoadRoleData()
        {
            ApiRequestModel apiRequest = new()
            {
                ApiService = ServicesRegistryEnum.ServicePortal,
                Endpoint = $"/Role/GetPagedForGanQuyen/{Guid.Empty}"
            };

            var baseQuery = new RoleQuery
            {
                gridRequest = new GridRequest
                {
                    page = 1,
                    pageSize = int.MaxValue
                }
            };

            var result = await CallService.Post<DataTableJson<RoleDto>>(apiRequest, baseQuery);
            if (result.Status == StatusCode.OK && result.Data != null)
            {
                DanhSachVaiTro = result.Data.Data
                .Select((item, index) =>
                {
                    item.STT = index + 1;
                    return item;
                })
                .ToList();
            }
        }

        private void OnRoleCheckedChanged(RoleDto role, bool isChecked)
        {
            role.DaGan = isChecked;
            if (role.IdPermissions is not null)
            {
                if (isChecked)
                {
                    foreach (var id in role.IdPermissions)
                    {
                        PermissionUserSelected.Remove(id);
                        PermissionFromRole.Add(id);
                    }
                }
                else
                {
                    foreach (var id in role.IdPermissions)
                    {
                        bool stillUsed = DanhSachVaiTro
                                        .Where(r => r.DaGan && r.IdPermissions != null && r.Id != role.Id)
                                        .Any(r => r.IdPermissions!.Contains(id));

                        if (!stillUsed)
                        {
                            PermissionFromRole.Remove(id);
                        }
                    }
                }
            }

            StateHasChanged();
        }

        #endregion

        #region quyền

        private async Task<IEnumerable<TreeViewItemDTO>> LoadPermissionTreeAsync()
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/Permission/GetPagedForGanQuyen"
                };
                var baseQuery = new PermissionQuery
                {
                    draw = 1,
                    gridRequest = new GridRequest
                    {
                        filter = new Filter(),
                        page = 1,
                        pageSize = int.MaxValue,
                    }
                };

                var result = await CallService.Post<DataTableJson<PermissionDto>>(apiRequest, baseQuery);
                if (result.Status == StatusCode.OK && result.Data != null)
                {
                    PermissionTree = result.Data.Data?.ToList() ?? new List<PermissionDto>();

                    var allPermissions = FlattenPermissions(PermissionTree);

                    var permissionIdsFromRole = allPermissions
                        .Where(p => DanhSachVaiTro.Any(r => r.DaGan && r.IdPermissions != null && r.IdPermissions.Contains(p.Id)))
                        .Select(p => p.Id)
                        .ToHashSet();

                    PermissionFromRole = permissionIdsFromRole;

                    PermissionUserSelected = allPermissions
                        .Where(p => p.IsSelected && !permissionIdsFromRole.Contains(p.Id))
                        .Select(p => p.Id)
                        .ToHashSet();
                }
                else
                {
                    PermissionTree = new List<PermissionDto>();
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách: {ex.Message}");
                PermissionTree = new List<PermissionDto>();
            }
            return ConvertToTreeItems(PermissionTree);
        }

        private IEnumerable<TreeViewItemDTO> ConvertToTreeItems(IEnumerable<PermissionDto> permissions, int level = 0)
        {
            return permissions.Select(x =>
            {
                bool isParent = x.ListChild != null && x.ListChild.Any();

                return new TreeViewItemDTO
                {
                    Id = x.Id.ToString(),
                    Text = $"[{(int)x.PermissionCode}] - {x.PermissionName}",
                    ModerationStatus = (int)x.ModerationStatus,
                    Expanded = true,
                    Type = level,
                    Items = isParent ? ConvertToTreeItems(x.ListChild!, level + 1).ToList() : new()
                };
            }).ToList();
        }

        private void OnPermissionCheckedChanged(string id, bool isChecked)
        {
            if (!Guid.TryParse(id, out var guid)) return;
            if (PermissionFromRole.Contains(guid)) return;

            if (isChecked)
                PermissionUserSelected.Add(guid);
            else
                PermissionUserSelected.Remove(guid);
        }

        private List<PermissionDto> FlattenPermissions(IEnumerable<PermissionDto> tree)
        {
            var list = new List<PermissionDto>();

            foreach (var node in tree)
            {
                list.Add(node);
                if (node.ListChild != null && node.ListChild.Any())
                {
                    list.AddRange(FlattenPermissions(node.ListChild));
                }
            }

            return list;
        }

        #endregion

        private async Task HideDialog()
        {
            await Dialog.CloseAsync();
        }
    }
}
