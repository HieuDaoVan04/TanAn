// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Service.Shared.Commons.Extensions;
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
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWorkQuanTriHeThong _UnitOfWork;
        private readonly IRequestContext _RequestContext;
        private readonly LoggingThaoTacQueue _loggingQueue;

        public PermissionService(IUnitOfWorkQuanTriHeThong UnitOfWork, IRequestContext requestContext, LoggingThaoTacQueue loggingThaoTacQueue)
        {
            _UnitOfWork = UnitOfWork;
            _RequestContext = requestContext;
            _loggingQueue = loggingThaoTacQueue;
        }

        public async Task<Guid> CreateAsync(PermissionForm item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var ItemCreate = new Domain.Entities.Permission
            {
                Description = item.Description ?? string.Empty,
                PermissionName = item.PermissionName,
                PermissionCode = item.PermissionCode,
                PermissionParentId = item.PermissionParentId
            };

            await _UnitOfWork.PermissionRepository.AddAsync(ItemCreate);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.ThemMoi.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã thêm mới permission '{ItemCreate.PermissionName}' vào lúc {DateTime.Now}",
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
            var itemDelete = await _UnitOfWork.PermissionRepository.GetByIdAsync(Id);
            if (itemDelete == null)
                return false;

            LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
            {
                UserId = _RequestContext.CurrentUser.UserId.ToString(),
                Action = EnumThaoTac.Xoa.ToString(),
                Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã xóa permission '{itemDelete.PermissionName}' vào lúc {DateTime.Now}",
                BeforeChange = JsonSerializer.Serialize(itemDelete),
                AfterChange = "",
                Created = DateTime.UtcNow,
                ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                IPAddress = ""
            };

            _UnitOfWork.PermissionRepository.Delete(itemDelete);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                await _loggingQueue.EnqueueAsync(log);
            }

            return true;
        }

        public async Task<PermissionDto> GetByIdAsync(Guid Id)
        {
            var entity = await _UnitOfWork.PermissionRepository.GetByIdAsync(Id);

            if (entity == null)
                throw new KeyNotFoundException($"Permission {Id} không tồn tại.");

            var dto = new PermissionDto
            {
                Id = entity.Id,
                PermissionName = entity.PermissionName,
                PermissionCode = entity.PermissionCode,
                Description = entity.Description,
                Created = entity.Created,
                IsSync = entity.IsSync,
                LastModified = entity.LastModified ?? entity.Created,
                ModerationStatus = entity.ModerationStatus
            };
            if (entity.PermissionParentId is not null && entity.PermissionParentId != Guid.Empty)
            {
                var parent = await _UnitOfWork.PermissionRepository.FindAsync(e => e.Id == entity.PermissionParentId);
                if (parent != null)
                {
                    dto.PermissionParentName = parent.PermissionName;
                }
            }
            return dto;
        }

        public async Task<DataTableJson> GetPaged(PermissionQuery searchOption)
        {
            var pagedObj = await _UnitOfWork.PermissionRepository
                                         .GetPagedDtoAsync(searchOption);

            var (items, total) = ((List<PermissionDto> Items, int Total))pagedObj;

            return new DataTableJson(items.ConvertAll(x => (object)x),
                                     searchOption.draw,
                                     total);
        }

        public async Task<DataTableJson> GetPagedForGanQuyen(PermissionQuery searchOption)
        {
            var pagedObj = await _UnitOfWork.PermissionRepository
                                         .GetPagedTreeForGanQuyenAsync(searchOption);

            var (items, total) = ((List<PermissionDto> Items, int Total))pagedObj;

            return new DataTableJson(items.ConvertAll(x => (object)x),
                                     searchOption.draw,
                                     total);
        }

        public async Task<bool> UpdateAsync(Guid Id, PermissionForm item)
        {
            var entity = await _UnitOfWork.PermissionRepository.GetByIdAsync(Id);

            if (entity == null)
                throw new KeyNotFoundException($"Permission {Id} không tồn tại.");

            var beforeChange = JsonSerializer.Serialize(entity);

            entity.PermissionName = item.PermissionName;
            entity.Description = item.Description ?? string.Empty;
            entity.PermissionCode = item.PermissionCode;
            entity.PermissionParentId = item.PermissionParentId;

            _UnitOfWork.PermissionRepository.Update(entity);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.CapNhat.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã cập nhật permission '{entity.PermissionName}' vào lúc {DateTime.Now}",
                    BeforeChange = beforeChange,
                    AfterChange = JsonSerializer.Serialize(entity),
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
            var itemUpdate = await _UnitOfWork.PermissionRepository.GetByIdAsync(id);

            if (itemUpdate == null)
                throw new KeyNotFoundException($"Permission {id} không tồn tại.");

            var beforeChange = JsonSerializer.Serialize(itemUpdate);

            itemUpdate.ModerationStatus = moderationStatus;
            _UnitOfWork.PermissionRepository.Update(itemUpdate);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                var action = moderationStatus == ModerationStatus.Approved ? EnumThaoTac.Duyet : EnumThaoTac.HuyDuyet;
                var actionText = moderationStatus == ModerationStatus.Approved ? "duyệt" : "hủy duyệt";

                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = action.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã {actionText} permission '{itemUpdate.PermissionName}' vào lúc {DateTime.Now}",
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

        public async Task<bool> SyncPermissionFromEnum()
        {
            try
            {
                var enumValues = Enum.GetValues(typeof(EnumPermissions))
                                     .Cast<EnumPermissions>()
                                     .Where(e => e != EnumPermissions.None);
                var permissionDictionary = new Dictionary<EnumPermissions, Domain.Entities.Permission>();

                foreach (var permissionCode in enumValues)
                {
                    var descriptionAttr = permissionCode
                        .GetType()
                        .GetField(permissionCode.ToString())
                        ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() as DescriptionAttribute;
                    string permissionName = descriptionAttr?.Description ?? "";

                    var ThuocTinhMoTa = permissionCode
                        .GetType()
                        .GetField(permissionCode.ToString())
                        ?.GetCustomAttributes(typeof(PermissionDescriptionAttribute), false)
                        .FirstOrDefault() as PermissionDescriptionAttribute;
                    string MoTa = ThuocTinhMoTa?.Description ?? "";

                    var existing = await _UnitOfWork.PermissionRepository
                        .FindAsync(r => r.PermissionCode == permissionCode);

                    EnumPermissions? parentEnum = null;
                    var codeValue = (int)permissionCode;
                    if (codeValue % 100 != 0)
                    {
                        var parentValue = codeValue / 100 * 100;
                        parentEnum = (EnumPermissions)parentValue;
                    }

                    Guid? parentId = null;
                    if (parentEnum.HasValue)
                    {
                        if (permissionDictionary.ContainsKey(parentEnum.Value))
                        {
                            parentId = permissionDictionary[parentEnum.Value].Id;
                        }
                        else
                        {
                            var parentDb = await _UnitOfWork.PermissionRepository
                                .FindAsync(r => r.PermissionCode == parentEnum.Value);
                            if (parentDb != null)
                                parentId = parentDb.Id;
                        }
                    }

                    if (existing == null)
                    {
                        var newPermission = new Domain.Entities.Permission
                        {
                            PermissionName = permissionName,
                            PermissionCode = permissionCode,
                            Description = MoTa,
                            IsSync = true,
                            PermissionParentId = parentId
                        };

                        await _UnitOfWork.PermissionRepository.AddAsync(newPermission);
                        await _UnitOfWork.CompleteAsync();

                        if (codeValue % 100 == 0)
                            permissionDictionary[permissionCode] = newPermission;
                    }
                    else
                    {
                        if (existing.PermissionName != permissionName)
                            existing.PermissionName = permissionName;

                        existing.IsSync = true;
                        existing.PermissionParentId = parentId;

                        _UnitOfWork.PermissionRepository.Update(existing);
                        await _UnitOfWork.CompleteAsync();

                        if (codeValue % 100 == 0)
                            permissionDictionary[permissionCode] = existing;
                    }
                }
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.DongBo.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã đồng bộ permissions từ Enum vào lúc {DateTime.Now}",
                    BeforeChange = "",
                    AfterChange = $"Đồng bộ thành công {enumValues.Count()} permissions",
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Đồng bộ quyền từ Enum thất bại: {ex.Message}", ex);
            }
        }

        public async Task<DataTableJson> GetPagedTree(PermissionQuery searchOption)
        {
            var pagedObj = await _UnitOfWork.PermissionRepository.GetPagedDtoAsync(searchOption);
            var (items, total) = ((List<PermissionDto> Items, int Total))pagedObj;
            var lookup = items.ToLookup(x => x.ParentId);
            List<PermissionDto> BuildTree(Guid? parentId)
            {
                return lookup[parentId].Select(x => new PermissionDto
                {
                    Id = x.Id,
                    PermissionName = x.PermissionName,
                    ModerationStatus = x.ModerationStatus,
                    PermissionCode = x.PermissionCode,
                    ParentId = x.ParentId,
                    ListChild = BuildTree(x.Id)
                }).ToList();
            }
            var tree = BuildTree(null);
            return new DataTableJson(tree.ConvertAll(x => (object)x), searchOption.draw, total);
        }
    }
}
