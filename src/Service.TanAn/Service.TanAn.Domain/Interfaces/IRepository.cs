// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Service.TanAn.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate, string[]? includes = null);
        Task<List<T>> FindAllAsync(Expression<Func<T, bool>>? predicate = null, string[]? includes = null);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}
