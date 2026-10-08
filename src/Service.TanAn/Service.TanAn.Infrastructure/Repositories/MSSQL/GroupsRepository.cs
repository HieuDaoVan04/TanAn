// "Một sản phẩm của HieuDV"

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces.MSSQL;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.Infrastructure.Repositories.MSSQL
{
    public class GroupsRepository : EfRepository<Groups>, IGroupsRepository
    {
        private readonly TanAnDbContext _dbContext;

        public GroupsRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Groups?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<Groups>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Groups?> FindAsync(Expression<Func<Groups, bool>> predicate)
        {
            return await _dbContext.Set<Groups>().FirstOrDefaultAsync(predicate);
        }

        public void Delete(Groups entity)
        {
            _dbContext.Set<Groups>().Remove(entity);
        }
    }
}
