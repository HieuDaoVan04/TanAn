// "Một sản phẩm của HieuDV"

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByIdAsync(Guid id);
        Task<Role?> FindAsync(Expression<Func<Role, bool>> predicate);
        void Delete(Role entity);
        Task<(object Items, int Total)> GetPagedDtoAsync(object query);
        (object Items, int Total) GetPagedForGanQuyen(Guid id, object query);
    }
}
