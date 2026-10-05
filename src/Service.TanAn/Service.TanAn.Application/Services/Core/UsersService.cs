// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Services;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces.Core;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces;

namespace Service.TanAn.Application.Services.Core
{
    public class UsersService : IUsersService
    {
        private readonly IUnitOfWorkQuanTriHeThong _UnitOfWork;
        private readonly IRequestContext _RequestContext;
        private readonly LoggingThaoTacQueue _loggingQueue;
        private readonly SystemConfigurationService _configuration;

        public UsersService(IUnitOfWorkQuanTriHeThong unitOfWork, IRequestContext requestContext, LoggingThaoTacQueue loggingQueue, SystemConfigurationService configuration)
        {
            _UnitOfWork = unitOfWork;
            _RequestContext = requestContext;
            _loggingQueue = loggingQueue;
            _configuration = configuration;
        }

        public async Task<Guid> CreateAsync(UsersForm item)
        {
            var isExist = await _UnitOfWork.UserRepository
              .FindAsync(x => x.UserName.ToLower() == item.UserName.Trim().ToLower());

            if (isExist != null)
                throw new InvalidOperationException("Tài khoản đã tồn tại.");

            var password = (string.IsNullOrEmpty(item.Password) ? item.PasswordHash : item.Password) ?? "";
            (await _configuration.GetPasswordPolicyAsync()).Validate(password);

            var ItemCreate = new User
            {
                UserName = item.UserName.Trim(),
                Email = item.Email.Trim(),
                FullName = item.FullName.Trim(),
                PasswordHash = Service.Shared.Commons.Helpers.PasswordHashing.Hash(password),
                PhoneNumber = item.PhoneNumber,
                AvatarUrl = item.AvatarUrl,
                PhanLoai = item.PhanLoai,
                KeyPublic = item.KeyPublic
            };

            await _UnitOfWork.UserRepository.AddAsync(ItemCreate);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.ThemMoi.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã thêm mới user '{ItemCreate.UserName}' vào lúc {DateTime.Now}",
                    BeforeChange = "",
                    AfterChange = JsonSerializer.Serialize(new
                    {
                        ItemCreate.Id,
                        ItemCreate.UserName,
                        ItemCreate.Email,
                        ItemCreate.FullName,
                        ItemCreate.PhoneNumber,
                        ItemCreate.AvatarUrl,
                        ItemCreate.PhanLoai,
                        ItemCreate.KeyPublic
                    }),
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
            var itemDelete = await _UnitOfWork.UserRepository.GetByIdAsync(Id);
            if (itemDelete == null)
                return false;

            LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
            {
                UserId = _RequestContext.CurrentUser.UserId.ToString(),
                Action = EnumThaoTac.Xoa.ToString(),
                Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã xóa user '{itemDelete.UserName}' vào lúc {DateTime.Now}",
                BeforeChange = JsonSerializer.Serialize(new
                {
                    itemDelete.Id,
                    itemDelete.UserName,
                    itemDelete.Email,
                    itemDelete.FullName,
                    itemDelete.PhoneNumber
                }),
                AfterChange = "",
                Created = DateTime.UtcNow,
                ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                IPAddress = ""
            };

            _UnitOfWork.UserRepository.Delete(itemDelete);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                await _loggingQueue.EnqueueAsync(log);
            }

            return true;
        }

        public async Task<UserDto> GetByIdAsync(Guid Id)
        {
            var item = await _UnitOfWork.UserRepository.GetByIdAsync(Id);
            if (item == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với Id {Id}");

            UserDto userDto = new UserDto
            {
                Id = item.Id,
                Email = item.Email,
                FullName = item.FullName,
                PhoneNumber = item.PhoneNumber,
                LockoutEnd = item.LockoutEnd,
                LastLogin = item.LastLogin,
                LastLoginIp = item.LastLoginIp,
                UserName = item.UserName,
                AvatarUrl = item.AvatarUrl,
                TotalLogin = item.TotalLogin,
                TotalLoginFaild = item.TotalLoginFaild,
                Created = item.Created,
                LastModified = item.LastModified ?? item.Created,
                LastModifiedBy = item.LastModifiedBy,
                PhanLoai = item.PhanLoai,
                KeyPublic = item.KeyPublic ?? string.Empty,
                ModerationStatus = item.ModerationStatus
            };

            var UserGroup = await _UnitOfWork.UserGroupsRepository.FindAllAsync(x => x.UsersId == item.Id);
            if (UserGroup.Any())
            {
                var groupDescriptions = new List<string>();
                foreach (var group in UserGroup)
                {
                    var ItemGroup = await _UnitOfWork.GroupsRepository.GetByIdAsync(group.GroupId);
                    if (ItemGroup != null)
                    {
                        groupDescriptions.Add(ItemGroup.Name);
                    }
                }
                userDto.DonVitxt = string.Join(", ", groupDescriptions);
            }

            return userDto;
        }

        public DataTableJson GetPaged(UserQuery searchOption)
        {
            var pagedObj = _UnitOfWork.UserRepository.GetPagedDto(searchOption);
            var (items, total) = ((List<UserDto> Items, int Total))pagedObj;

            return new DataTableJson(items.Cast<object>().ToList(), searchOption.draw, total, items.Count);
        }

        public async Task<DataTableJson> GetPagedByGroupId(UserQuery baseQuery)
        {
            var pagedObj = await _UnitOfWork.UserRepository.GetPagedByGroupIdAsync(baseQuery);
            var (items, total) = ((List<UserDto> Items, int Total))pagedObj;

            return new DataTableJson(items.Cast<object>().ToList(), baseQuery.draw, total, items.Count);
        }

        public async Task<bool> UpdateAsync(Guid Id, UsersForm item)
        {
            var itemUpdate = await _UnitOfWork.UserRepository.GetByIdAsync(Id);
            if (itemUpdate == null)
                return false;

            var isExist = await _UnitOfWork.UserRepository
               .FindAsync(x => x.UserName.ToLower() == item.UserName.Trim().ToLower() && x.Id != Id);

            if (isExist != null)
                throw new InvalidOperationException("Tài khoản đã tồn tại.");

            var beforeChange = JsonSerializer.Serialize(new
            {
                itemUpdate.Id,
                itemUpdate.UserName,
                itemUpdate.Email,
                itemUpdate.FullName,
                itemUpdate.PhoneNumber,
                itemUpdate.AvatarUrl
            });

            itemUpdate.UserName = item.UserName.Trim();
            itemUpdate.Email = item.Email.Trim();
            itemUpdate.FullName = item.FullName.Trim();
            if (!string.IsNullOrEmpty(item.PasswordMoi))
            {
                (await _configuration.GetPasswordPolicyAsync()).Validate(item.PasswordMoi);
                itemUpdate.PasswordHash = Service.Shared.Commons.Helpers.PasswordHashing.Hash(item.PasswordMoi);
            }
            itemUpdate.PhoneNumber = item.PhoneNumber;
            itemUpdate.AvatarUrl = item.AvatarUrl;

            _UnitOfWork.UserRepository.Update(itemUpdate);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.CapNhat.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã cập nhật user '{itemUpdate.UserName}' vào lúc {DateTime.Now}",
                    BeforeChange = beforeChange,
                    AfterChange = JsonSerializer.Serialize(new
                    {
                        itemUpdate.Id,
                        itemUpdate.UserName,
                        itemUpdate.Email,
                        itemUpdate.FullName,
                        itemUpdate.PhoneNumber,
                        itemUpdate.AvatarUrl,
                        PasswordChanged = !string.IsNullOrEmpty(item.PasswordMoi)
                    }),
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
            var itemUpdate = await _UnitOfWork.UserRepository.GetByIdAsync(id);
            if (itemUpdate == null)
                return false;

            var beforeChange = JsonSerializer.Serialize(new
            {
                itemUpdate.Id,
                itemUpdate.UserName,
                OldStatus = itemUpdate.ModerationStatus
            });

            itemUpdate.ModerationStatus = moderationStatus;
            _UnitOfWork.UserRepository.Update(itemUpdate);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                var action = moderationStatus == ModerationStatus.Approved ? EnumThaoTac.Duyet : EnumThaoTac.HuyDuyet;
                var actionText = moderationStatus == ModerationStatus.Approved ? "duyệt" : "hủy duyệt";

                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = action.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã {actionText} user '{itemUpdate.UserName}' vào lúc {DateTime.Now}",
                    BeforeChange = beforeChange,
                    AfterChange = JsonSerializer.Serialize(new
                    {
                        itemUpdate.Id,
                        itemUpdate.UserName,
                        NewStatus = moderationStatus
                    }),
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);
            }

            return true;
        }

        public async Task<bool> AddUserToGroupAsync(Guid userId, Guid groupId)
        {
            var user = await _UnitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với Id = {userId}");

            var group = await _UnitOfWork.GroupsRepository.GetByIdAsync(groupId);
            if (group == null)
                throw new KeyNotFoundException($"Không tìm thấy nhóm với Id = {groupId}");

            var donViId = group.ParentId ?? groupId;
            var exists = await _UnitOfWork.UserGroupsRepository
                .FindAsync(x => x.UsersId == userId && x.PhongBanId == groupId);

            if (exists != null)
                throw new InvalidOperationException("Người dùng đã tồn tại trong phòng ban này.");

            var newUserGroup = new UserGroups
            {
                UsersId = userId,
                PhongBanId = groupId,
                GroupId = donViId
            };

            await _UnitOfWork.UserGroupsRepository.AddAsync(newUserGroup);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.CapNhat.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã thêm user '{user.UserName}' vào phòng ban '{group.Name}' vào lúc {DateTime.Now}",
                    BeforeChange = "",
                    AfterChange = JsonSerializer.Serialize(new
                    {
                        UserId = userId,
                        user.UserName,
                        GroupId = groupId,
                        GroupName = group.Name,
                        DonViId = donViId
                    }),
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);
            }

            return result > 0;
        }

        public async Task<bool> RemoveUserFromGroupAsync(Guid UserId, Guid GroupId)
        {
            var user = await _UnitOfWork.UserRepository.GetByIdAsync(UserId);
            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID {UserId}");

            var group = await _UnitOfWork.GroupsRepository.GetByIdAsync(GroupId);
            if (group == null)
                throw new KeyNotFoundException($"Không tìm thấy đơn vị với ID {GroupId}");

            var userGroup = await _UnitOfWork.UserGroupsRepository
                .FindAsync(ug => ug.UsersId == UserId && ug.PhongBanId == GroupId);
            if (userGroup == null)
                throw new InvalidOperationException("Người dùng không thuộc nhóm này.");

            var beforeChange = JsonSerializer.Serialize(new
            {
                UserId,
                user.UserName,
                GroupId,
                GroupName = group.Name
            });

            _UnitOfWork.UserGroupsRepository.Delete(userGroup);
            var result = await _UnitOfWork.CompleteAsync();

            if (result > 0)
            {
                LogThaoTacNguoiDung log = new LogThaoTacNguoiDung
                {
                    UserId = _RequestContext.CurrentUser.UserId.ToString(),
                    Action = EnumThaoTac.Xoa.ToString(),
                    Description = $"Người dùng {_RequestContext.CurrentUser.UserName} đã xóa user '{user.UserName}' khỏi phòng ban '{group.Name}' vào lúc {DateTime.Now}",
                    BeforeChange = beforeChange,
                    AfterChange = "",
                    Created = DateTime.UtcNow,
                    ModuleName = EnumModules.DanhSachNguoiDung.ToString(),
                    IPAddress = ""
                };
                await _loggingQueue.EnqueueAsync(log);
            }

            return result > 0;
        }
    }
}
