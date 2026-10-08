// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Application.Interfaces.Core
{
    public interface IModuleService
    {
        DataTableJson GetPaged(BaseQuery query);
        Task<Module> GetByIdAsync(Guid id);
        List<Module> GetAll();

        Task<Guid> AddAsync(ModuleForm request);
        Task<bool> UpdateAsync(Guid id, ModuleForm request);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<bool> RejectAsync(Guid id);

        List<ModuleTreeDto> GetFilteredTree(int phanLoai, string searchTerm);
        List<ModuleTreeDto> GetFilteredTreePhanQuyen(int phanLoai, string searchTerm, Guid RoleID);
        List<ModuleTreeDto> GetTreePublishingForMCS(EnumModuleType phanLoai);
    }
}
