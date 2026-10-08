// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using PermissionDto = Service.Shared.Contracts.DTOs.PermissionDto;

namespace Service.TanAn.Application.Interfaces.Core
{
    public interface IPermissionService
    {
        Task<Guid> CreateAsync(PermissionForm item);
        Task<bool> DeleteAsync(Guid Id);
        Task<bool> UpdateAsync(Guid Id, PermissionForm item);
        Task<DataTableJson> GetPaged(PermissionQuery baseQuery);
        Task<DataTableJson> GetPagedForGanQuyen(PermissionQuery searchOption);
        Task<DataTableJson> GetPagedTree(PermissionQuery searchOption);
        Task<PermissionDto> GetByIdAsync(Guid Id);
        Task<bool> SyncPermissionFromEnum();
        Task<bool> ChangeModerationStatusAsync(Guid id, ModerationStatus moderationStatus);
    }
}
