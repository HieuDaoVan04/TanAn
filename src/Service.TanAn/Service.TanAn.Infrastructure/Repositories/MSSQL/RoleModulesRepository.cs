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
    public class RoleModulesRepository : EfRepository<RoleModule>, IRoleModulesRepository
    {
        private readonly TanAnDbContext _dbContext;

        public RoleModulesRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<RoleModule> FindAll(Expression<Func<RoleModule, bool>> predicate)
        {
            return _dbContext.Set<RoleModule>().Where(predicate);
        }

        public async Task<List<RoleModule>> FindAllAsync(Expression<Func<RoleModule, bool>> predicate, string[]? includes = null)
        {
            IQueryable<RoleModule> query = _dbContext.Set<RoleModule>().Where(predicate);

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public void Delete(RoleModule entity)
        {
            _dbContext.Set<RoleModule>().Remove(entity);
        }
    }
}
