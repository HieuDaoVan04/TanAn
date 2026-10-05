// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        new Task<List<RolePermission>> FindAllAsync(Expression<Func<RolePermission, bool>> predicate, string[]? includes = null);
        void DeleteRange(IEnumerable<RolePermission> entities);
    }
}
