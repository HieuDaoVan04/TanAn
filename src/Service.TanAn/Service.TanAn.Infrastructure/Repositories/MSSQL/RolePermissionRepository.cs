// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces.MSSQL;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.Infrastructure.Repositories.MSSQL
{
    public class RolePermissionRepository : EfRepository<RolePermission>, IRolePermissionRepository
    {
        private readonly TanAnDbContext _dbContext;

        public RolePermissionRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<RolePermission>> FindAllAsync(Expression<Func<RolePermission, bool>> predicate, string[]? includes = null)
        {
            IQueryable<RolePermission> query = _dbContext.Set<RolePermission>().Where(predicate);

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public void DeleteRange(IEnumerable<RolePermission> entities)
        {
            _dbContext.Set<RolePermission>().RemoveRange(entities);
        }
    }
}
