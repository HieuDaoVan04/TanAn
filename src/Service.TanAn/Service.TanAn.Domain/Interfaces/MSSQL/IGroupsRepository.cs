// "Một sản phẩm của HieuDV"

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface IGroupsRepository : IRepository<Groups>
    {
        Task<Groups?> GetByIdAsync(Guid id);
        Task<Groups?> FindAsync(Expression<Func<Groups, bool>> predicate);
        void Delete(Groups entity);
    }
}
