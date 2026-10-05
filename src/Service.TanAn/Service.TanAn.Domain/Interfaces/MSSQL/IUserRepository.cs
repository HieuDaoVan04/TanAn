// "Một sản phẩm của HieuDV"

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> FindAsync(Expression<Func<User, bool>> predicate, string[]? includes = null);
        void Delete(User entity);
        (object Items, int Total) GetPagedDto(object query);
        Task<(object Items, int Total)> GetPagedByGroupIdAsync(object query);
    }
}
