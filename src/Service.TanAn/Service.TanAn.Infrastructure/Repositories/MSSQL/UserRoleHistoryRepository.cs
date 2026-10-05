// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces.MSSQL;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.Infrastructure.Repositories.MSSQL
{
    public class UserRoleHistoryRepository : EfRepository<UserRoleHistory>, IUserRoleHistoryRepository
    {
        private readonly TanAnDbContext _dbContext;

        public UserRoleHistoryRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserRoleHistory?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<UserRoleHistory>().FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
