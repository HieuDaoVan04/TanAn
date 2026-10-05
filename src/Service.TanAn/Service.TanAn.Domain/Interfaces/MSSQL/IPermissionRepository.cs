// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface IPermissionRepository : IRepository<Permission>
    {
        Task<Permission?> GetByIdAsync(Guid id);
        new Task<Permission> AddAsync(Permission entity);
        void Delete(Permission entity);
        Task<object> GetPagedDtoAsync(object query);
        Task<object> GetPagedTreeForGanQuyenAsync(object query);
    }
}
