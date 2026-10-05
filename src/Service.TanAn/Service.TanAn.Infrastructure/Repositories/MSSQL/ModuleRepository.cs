// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Querys.Grid;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces.MSSQL;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.Infrastructure.Repositories.MSSQL
{
    public class ModuleRepository : EfRepository<Module>, IModuleRepository
    {
        private readonly TanAnDbContext _dbContext;

        public ModuleRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Module?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<Module>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public new async Task<Module> AddAsync(Module entity)
        {
            await _dbContext.Set<Module>().AddAsync(entity);
            return entity;
        }

        public void Delete(Module entity)
        {
            _dbContext.Set<Module>().Remove(entity);
        }

        public IQueryable<Module> FindAll(Expression<Func<Module, bool>> predicate)
        {
            return _dbContext.Set<Module>().Where(predicate);
        }

        public IQueryable<Module> FilterData(Func<IQueryable<Module>, IQueryable<Module>> filter, GridRequest gridRequest, ref int totalRecords)
        {
            var query = _dbContext.Set<Module>().AsQueryable();
            query = filter(query);

            totalRecords = query.Count();
            int pageIndex = gridRequest.page > 0 ? gridRequest.page : 1;
            int pageSize = gridRequest.pageSize > 0 ? gridRequest.pageSize : 10;

            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IQueryable<Module> GetTableNoTracking()
        {
            return _dbContext.Set<Module>().AsNoTracking();
        }
    }
}
