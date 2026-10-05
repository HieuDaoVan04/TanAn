// "Một sản phẩm của HieuDV"

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface ISystemParameterRepository : IRepository<SystemParameter>
    {
        Task<SystemParameter?> GetByIdAsync(Guid id);
        Task<SystemParameter?> FindAsync(Expression<Func<SystemParameter, bool>> predicate);
        void Delete(SystemParameter entity);
        Task<(object Items, int Total)> GetPagedDtoAsync(object query);
    }
}
