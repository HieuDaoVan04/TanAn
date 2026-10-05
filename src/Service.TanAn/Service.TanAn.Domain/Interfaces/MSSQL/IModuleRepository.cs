// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Querys.Grid;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface IModuleRepository : IRepository<Module>
    {
        Task<Module?> GetByIdAsync(Guid id);
        new Task<Module> AddAsync(Module entity);
        void Delete(Module entity);
        IQueryable<Module> FindAll(Expression<Func<Module, bool>> predicate);
        IQueryable<Module> FilterData(Func<IQueryable<Module>, IQueryable<Module>> filter, GridRequest gridRequest, ref int totalRecords);
        IQueryable<Module> GetTableNoTracking();
    }
}
