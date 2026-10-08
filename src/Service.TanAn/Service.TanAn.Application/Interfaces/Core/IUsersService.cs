// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces.Core
{
    public interface IUsersService
    {
        Task<Guid> CreateAsync(UsersForm item);
        Task<bool> DeleteAsync(Guid Id);
        Task<bool> RemoveUserFromGroupAsync(Guid UserId, Guid GroupId);
        Task<bool> AddUserToGroupAsync(Guid userId, Guid groupId);
        Task<bool> UpdateAsync(Guid Id, UsersForm item);
        DataTableJson GetPaged(UserQuery baseQuery);
        Task<DataTableJson> GetPagedByGroupId(UserQuery baseQuery);
        Task<UserDto> GetByIdAsync(Guid Id);
        Task<bool> ChangeModerationStatusAsync(Guid id, ModerationStatus moderationStatus);
    }
}
