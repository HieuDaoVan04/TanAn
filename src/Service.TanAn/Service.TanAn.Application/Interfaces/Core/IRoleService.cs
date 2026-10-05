// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces.Core
{
    public interface IRoleService
    {
        Task<Guid> CreateAsync(RoleForm item);
        Task<bool> DeleteAsync(Guid Id);
        Task<bool> UpdateAsync(Guid Id, RoleForm item);
        Task<DataTableJson> GetPaged(BaseQuery baseQuery);
        DataTableJson GetPagedForGanQuyen(Guid id, RoleQuery query);
        Task<RoleDto> GetByIdAsync(Guid Id);
        Task<UserRoleHistoryDto> GetUserRoleHistoryById(Guid Id);
        Task<bool> SyncRoleFromEnum();
        Task<bool> GanQuyenVaoVaiTro(Guid roleId, GanQuyenDto request);
        Task<bool> GanMenuAsync(GanMenuVaoVaiTroDto item);
        Task<bool> ChangeModerationStatusAsync(Guid id, ModerationStatus moderationStatus);
        Task<List<Service.Shared.Contracts.DTOs.PermissionDto>> GetPermissionFromRoleId(Guid id);
        Task<List<ModuleDto>> GetModuleFromRoleId(Guid id);
        Task<List<Service.Shared.Commons.Models.PermissionDto>> GetPermissionFromLstRoleId(List<Guid> LstId);
        Task<List<MenuItemDto>> GetModuleFromLstRoleId(List<Guid> LstId);
    }
}
