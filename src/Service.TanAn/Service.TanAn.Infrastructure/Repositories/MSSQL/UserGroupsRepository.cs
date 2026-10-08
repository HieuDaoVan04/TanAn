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
    public class UserGroupsRepository : EfRepository<UserGroups>, IUserGroupsRepository
    {
        private readonly TanAnDbContext _dbContext;

        public UserGroupsRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserGroups?> FindAsync(Expression<Func<UserGroups, bool>> predicate)
        {
            return await _dbContext.Set<UserGroups>().FirstOrDefaultAsync(predicate);
        }

        public async Task<List<UserGroups>> FindAllAsync(Expression<Func<UserGroups, bool>> predicate)
        {
            return await _dbContext.Set<UserGroups>().Where(predicate).ToListAsync();
        }

        public void Delete(UserGroups entity)
        {
            _dbContext.Set<UserGroups>().Remove(entity);
        }
    }
}
