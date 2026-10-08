// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface IRoleModulesRepository : IRepository<RoleModule>
    {
        IQueryable<RoleModule> FindAll(Expression<Func<RoleModule, bool>> predicate);
        new Task<List<RoleModule>> FindAllAsync(Expression<Func<RoleModule, bool>> predicate, string[]? includes = null);
        void Delete(RoleModule entity);
    }
}
