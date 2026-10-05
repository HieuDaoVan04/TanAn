// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Services;
using Service.Shared.Contracts.DTOs;
using PermissionDto = Service.Shared.Contracts.DTOs.PermissionDto;
using Service.TanAn.Application.Interfaces.Core;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces;

namespace Service.TanAn.Application.Services.Core
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWorkQuanTriHeThong _UnitOfWork;
        private readonly IRequestContext _RequestContext;
        private readonly LoggingThaoTacQueue _loggingQueue;

        public RoleService(IUnitOfWorkQuanTriHeThong UnitOfWork, IRequestContext requestContext, LoggingThaoTacQueue loggingThaoTacQueue)
        {
            _UnitOfWork = UnitOfWork;
            _RequestContext = requestContext;
            _loggingQueue = loggingThaoTacQueue;
        }

        public async Task<Guid> CreateAsync(RoleForm item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var ItemCreate = new Role
            {
                RoleName = item.RoleName,
                RoleCode = item.RoleCode,
                Mota = item.Mota ?? string.Empty,
            };

            await _UnitOfWork.RoleRepository.AddAsync(ItemCreate);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.ThemMoi.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã thêm mới role '{ItemCreate.RoleName}' vào lúc {DateTime.Now}",
                    BeforeChange = "",
                    AfterChange = JsonSerializer.Serialize(ItemCreate),
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);
            }

            return ItemCreate.Id;
        }

        public async Task<bool> DeleteAsync(Guid Id)
        {
            var itemDelete = await _UnitOfWork.RoleRepository.GetByIdAsync(Id);
            if (itemDelete == null)
                return false;

            LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
            {
                UserId = _RequestContext.CurrentUser.UserId.ToString(),
                Action = EnumThaoTac.Xoa.ToString(),
                Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã xóa role '{itemDelete.RoleName}' vào lúc {DateTime.Now}",
                BeforeChange = JsonSerializer.Serialize(itemDelete),
                AfterChange = "",
                Created = DateTime.UtcNow,
                ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                IPAddress = ""
            };

            _UnitOfWork.RoleRepository.Delete(itemDelete);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                await _loggingQueue.EnqueueAsync(log);
            }

            return true;
        }

        public async Task<RoleDto> GetByIdAsync(Guid Id)
        {
            Role? entity = await _UnitOfWork.RoleRepository.GetByIdAsync(Id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy vai trò với Id: {Id}");

            RoleDto dto = new RoleDto
            {
                Id = entity.Id,
                RoleCode = entity.RoleCode,
                RoleName = entity.RoleName,
                Mota = entity.Mota,
                IsSync = entity.IsSync,
                Created = entity.Created,
                LastModified = entity.LastModified ?? entity.Created,
                ModerationStatus = entity.ModerationStatus
            };

            return dto;
        }

        public async Task<DataTableJson> GetPaged(BaseQuery searchOption)
        {
            var pagedObj = await _UnitOfWork.RoleRepository.GetPagedDtoAsync(searchOption);
            var (items, total) = ((List<RoleDto> Items, int Total))pagedObj;

            return new DataTableJson(items.Cast<object>().ToList(), searchOption.draw, total, items.Count);
        }

        public DataTableJson GetPagedForGanQuyen(Guid id, RoleQuery query)
        {
            var pagedObj = _UnitOfWork.RoleRepository.GetPagedForGanQuyen(id, query);
            var (items, total) = ((List<RoleDto> Items, int Total))pagedObj;

            return new DataTableJson(items.Cast<object>().ToList(), query.draw, total, items.Count);
        }

        public async Task<bool> UpdateAsync(Guid id, RoleForm item)
        {
            var itemUpdate = await _UnitOfWork.RoleRepository.GetByIdAsync(id);

            if (itemUpdate == null)
                throw new KeyNotFoundException($"Không tìm thấy Role với Id = {id}");

            var beforeChange = JsonSerializer.Serialize(itemUpdate);

            itemUpdate.RoleName = item.RoleName;
            itemUpdate.RoleCode = item.RoleCode;
            itemUpdate.Mota = item.Mota ?? string.Empty;

            _UnitOfWork.RoleRepository.Update(itemUpdate);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.CapNhat.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã cập nhật role '{itemUpdate.RoleName}' vào lúc {DateTime.Now}",
                    BeforeChange = beforeChange,
                    AfterChange = JsonSerializer.Serialize(itemUpdate),
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);
            }

            return true;
        }

        public async Task<bool> ChangeModerationStatusAsync(Guid id, ModerationStatus moderationStatus)
        {
            var itemUpdate = await _UnitOfWork.RoleRepository.GetByIdAsync(id);

            if (itemUpdate == null)
                return false;

            var beforeChange = JsonSerializer.Serialize(itemUpdate);

            itemUpdate.ModerationStatus = moderationStatus;
            _UnitOfWork.RoleRepository.Update(itemUpdate);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                var action = moderationStatus == ModerationStatus.Approved ? EnumThaoTac.Duyet : EnumThaoTac.HuyDuyet;
                var actionText = moderationStatus == ModerationStatus.Approved ? "duyệt" : "hủy duyệt";

                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = action.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã {actionText} role '{itemUpdate.RoleName}' vào lúc {DateTime.Now}",
                    BeforeChange = beforeChange,
                    AfterChange = JsonSerializer.Serialize(itemUpdate),
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);
            }

            return true;
        }

        public async Task<bool> SyncRoleFromEnum()
        {
            var enumValues = Enum.GetValues(typeof(EnumRoles)).Cast<EnumRoles>();

            try
            {
                foreach (var roleCode in enumValues)
                {
                    var description = roleCode
                        .GetType()
                        .GetField(roleCode.ToString())?
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() as DescriptionAttribute;

                    var roleName = description?.Description ?? roleCode.ToString();
                    var roleCodeStr = roleCode.ToString();

                    var existingRole = await _UnitOfWork.RoleRepository.FindAsync(r => r.RoleCode == roleCodeStr);

                    if (existingRole == null)
                    {
                        var newRole = new Role
                        {
                            RoleName = roleName,
                            RoleCode = roleCodeStr,
                            IsSync = true,
                        };

                        await _UnitOfWork.RoleRepository.AddAsync(newRole);
                        await _UnitOfWork.CompleteAsync();
                    }
                    else
                    {
                        if (existingRole.RoleName != roleName)
                        {
                            existingRole.RoleName = roleName;
                            existingRole.Mota = "";

                            _UnitOfWork.RoleRepository.Update(existingRole);
                            await _UnitOfWork.CompleteAsync();
                        }
                    }
                }

                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.DongBo.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã đồng bộ roles từ Enum vào lúc {DateTime.Now}",
                    BeforeChange = "",
                    AfterChange = $"Đồng bộ thành công {enumValues.Count()} roles",
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Đồng bộ vai trò từ Enum thất bại: {ex.Message}");
            }
        }

        public async Task<bool> GanQuyenVaoVaiTro(Guid roleId, GanQuyenDto request)
        {
            var role = await _UnitOfWork.RoleRepository.GetByIdAsync(roleId);
            if (role == null)
                return false;

            var currentPermissions = await _UnitOfWork.RolePermissionRepository
                .FindAllAsync(rp => rp.RoleId == roleId);

            var currentPermissionIds = currentPermissions.Select(rp => rp.PermissionId).ToList();
            var newPermissionIds = request.LstIdPermission ?? new List<Guid>();

            var beforeChange = JsonSerializer.Serialize(new
            {
                RoleId = roleId,
                CurrentPermissions = currentPermissionIds
            });

            var toAdd = newPermissionIds.Except(currentPermissionIds).ToList();
            foreach (var permissionId in toAdd)
            {
                await _UnitOfWork.RolePermissionRepository.AddAsync(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
            }

            var toDelete = currentPermissions
                .Where(rp => !newPermissionIds.Contains(rp.PermissionId))
                .ToList();
            _UnitOfWork.RolePermissionRepository.DeleteRange(toDelete);

            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.CapNhat.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã gán quyền cho role '{role.RoleName}' vào lúc {DateTime.Now}",
                    BeforeChange = beforeChange,
                    AfterChange = JsonSerializer.Serialize(new
                    {
                        RoleId = roleId,
                        NewPermissions = newPermissionIds,
                        AddedPermissions = toAdd,
                        RemovedPermissions = toDelete.Select(d => d.PermissionId)
                    }),
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);
            }

            return true;
        }

        public async Task<bool> GanMenuAsync(GanMenuVaoVaiTroDto item)
        {
            var role = await _UnitOfWork.RoleRepository.GetByIdAsync(item.RoleId);
            if (role == null)
                throw new KeyNotFoundException($"Không tìm thấy vai trò với ID {item.RoleId}");

            var requestModuleIds = item.ModuleIds?.Distinct().ToList() ?? new();

            foreach (var moduleId in requestModuleIds)
            {
                var moduleObj = await _UnitOfWork.ModuleRepository.GetByIdAsync(moduleId);
                if (moduleObj == null)
                    throw new ArgumentException($"Module với ID {moduleId} không tồn tại");
            }

            var currentRoleModules = await _UnitOfWork.RoleModulesRepository
                .FindAllAsync(ur => ur.RoleId == item.RoleId);

            var currentModuleIds = currentRoleModules.Select(x => x.ModuleId).ToList();
            var modulesToAdd = requestModuleIds.Except(currentModuleIds).ToList();
            var modulesToRemove = currentRoleModules.Where(x => !requestModuleIds.Contains(x.ModuleId)).ToList();

            var beforeChange = JsonSerializer.Serialize(new
            {
                item.RoleId,
                CurrentModuleIds = currentModuleIds,
            });

            foreach (var moduleToRemove in modulesToRemove)
                _UnitOfWork.RoleModulesRepository.Delete(moduleToRemove);

            foreach (var moduleToAdd in modulesToAdd)
            {
                var newRoleModule = new RoleModule
                {
                    RoleId = item.RoleId,
                    ModuleId = moduleToAdd,
                };
                await _UnitOfWork.RoleModulesRepository.AddAsync(newRoleModule);
            }

            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.CapNhat.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã gán menu cho vai trò '{role.RoleName}' vào lúc {DateTime.Now}",
                    BeforeChange = beforeChange,
                    AfterChange = JsonSerializer.Serialize(new
                    {
                        item.RoleId,
                        NewModuleIds = requestModuleIds,
                        ModulesAdded = modulesToAdd,
                        ModulesRemoved = modulesToRemove.Select(r => r.ModuleId),
                    }),
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);
            }

            return result > 0;
        }

        public async Task<List<PermissionDto>> GetPermissionFromRoleId(Guid roleId)
        {
            var rolePermissions = await _UnitOfWork.RolePermissionRepository.FindAllAsync(e => e.RoleId == roleId);

            if (rolePermissions == null || !rolePermissions.Any())
                return new List<PermissionDto>();

            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();
            var permissions = await _UnitOfWork.PermissionRepository.FindAllAsync(p => permissionIds.Contains(p.Id));

            var permissionDtos = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                PermissionName = p.PermissionName,
                PermissionCode = p.PermissionCode,
                IsSelected = true,
            }).ToList();

            return permissionDtos;
        }

        public async Task<List<ModuleDto>> GetModuleFromRoleId(Guid roleId)
        {
            var roleModules = await _UnitOfWork.RoleModulesRepository.FindAllAsync(e => e.RoleId == roleId);

            if (roleModules == null || !roleModules.Any())
                return new List<ModuleDto>();

            var moduleIds = roleModules.Select(rp => rp.ModuleId).ToList();
            var modules = await _UnitOfWork.ModuleRepository.FindAllAsync(p => moduleIds.Contains(p.Id));

            var moduleDtos = modules.Select(p => new ModuleDto
            {
                Id = p.Id,
                PhanLoai = p.PhanLoai,
                TenModule = p.TenModule,
                Icon = p.Icon,
                LienKet = p.LienKet,
                ViTri = p.ViTri,
                ModuleChaId = p.ModuleChaId,
                ModerationStatus = p.ModerationStatus,
                DaGan = true
            }).ToList();

            return moduleDtos;
        }

        public async Task<List<Service.Shared.Commons.Models.PermissionDto>> GetPermissionFromLstRoleId(List<Guid> LstId)
        {
            if (LstId == null || !LstId.Any())
                return new List<Service.Shared.Commons.Models.PermissionDto>();

            var rolePermissions = await _UnitOfWork.RolePermissionRepository
                .FindAllAsync(rp => LstId.Contains(rp.RoleId), new[] { nameof(RolePermission.Permission) });

            if (rolePermissions == null || !rolePermissions.Any())
                return new List<Service.Shared.Commons.Models.PermissionDto>();

            var result = rolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => new Service.Shared.Commons.Models.PermissionDto
                {
                    Code = rp.Permission!.PermissionCode.ToString(),
                    Name = rp.Permission.PermissionName
                })
                .DistinctBy(p => p.Code)
                .ToList();

            return result;
        }

        public async Task<List<MenuItemDto>> GetModuleFromLstRoleId(List<Guid> LstId)
        {
            if (LstId == null || !LstId.Any())
                return new List<MenuItemDto>();

            var roleModules = await _UnitOfWork.RoleModulesRepository
                .FindAllAsync(rm => LstId.Contains(rm.RoleId));

            if (roleModules == null || !roleModules.Any())
                return new List<MenuItemDto>();

            var moduleIds = roleModules.Select(rm => rm.ModuleId).Distinct().ToList();
            var modules = await _UnitOfWork.ModuleRepository.FindAllAsync(m => moduleIds.Contains(m.Id));

            var result = modules
                .Select(rm => new MenuItemDto
                {
                    Id = rm.Id,
                    Title = rm.TenModule,
                    Path = rm.LienKet ?? string.Empty,
                    Icon = rm.Icon,
                    Order = rm.ViTri
                })
                .ToList();

            return result;
        }

        public async Task<UserRoleHistoryDto> GetUserRoleHistoryById(Guid Id)
        {
            var entity = await _UnitOfWork.UserRoleHistoryRepository.GetByIdAsync(Id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy lịch sử với Id {Id}");

            return new UserRoleHistoryDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ChuyenTrangId = entity.ChuyenTrangId,
                ChuyenTrangName = entity.ChuyenTrangName,
                TenNguoiThucHien = entity.TenNguoiThucHien,
                LstChuyenMucAfterEdit = entity.LstChuyenMucAfterEdit,
                LstChuyenMucBeforeEdit = entity.LstChuyenMucBeforeEdit,
                ListRoleAfterEdit = entity.ListRoleAfterEdit,
                ListRoleBeforeEdit = entity.ListRoleBeforeEdit,
                Created = entity.Created,
            };
        }
    }
}
