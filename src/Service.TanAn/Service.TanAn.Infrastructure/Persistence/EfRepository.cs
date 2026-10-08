// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Domain.Interfaces;

namespace Service.TanAn.Infrastructure.Persistence
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly TanAnDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public EfRepository(TanAnDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate, string[]? includes = null)
        {
            IQueryable<T> query = _dbSet;
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    if (!string.IsNullOrWhiteSpace(include))
                        query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<T>> FindAllAsync(Expression<Func<T, bool>>? predicate = null, string[]? includes = null)
        {
            IQueryable<T> query = _dbSet;
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    if (!string.IsNullOrWhiteSpace(include))
                        query = query.Include(include);
                }
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate != null)
                return await _dbSet.CountAsync(predicate);
            return await _dbSet.CountAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
