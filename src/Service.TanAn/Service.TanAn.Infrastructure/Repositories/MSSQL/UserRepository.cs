// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces.MSSQL;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.Infrastructure.Repositories.MSSQL
{
    public class UserRepository : EfRepository<User>, IUserRepository
    {
        private readonly TanAnDbContext _dbContext;

        public UserRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<User>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> FindAsync(Expression<Func<User, bool>> predicate, string[]? includes = null)
        {
            IQueryable<User> query = _dbContext.Set<User>().Where(predicate);
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public void Delete(User entity)
        {
            _dbContext.Set<User>().Remove(entity);
        }

        public (object Items, int Total) GetPagedDto(object query)
        {
            var searchQuery = query as UserQuery ?? new UserQuery();
            var q = _dbContext.Set<User>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery.Keyword))
            {
                q = q.Where(x => x.UserName.Contains(searchQuery.Keyword) || x.FullName.Contains(searchQuery.Keyword) || x.Email.Contains(searchQuery.Keyword));
            }

            int total = q.Count();
            int pageIndex = searchQuery.PageIndex > 0 ? searchQuery.PageIndex : 1;
            int pageSize = searchQuery.PageSize > 0 ? searchQuery.PageSize : 10;

            var items = q.Skip((pageIndex - 1) * pageSize)
                         .Take(pageSize)
                         .Select(x => new UserDto
                         {
                             Id = x.Id,
                             UserName = x.UserName,
                             Email = x.Email,
                             FullName = x.FullName,
                             PhoneNumber = x.PhoneNumber,
                             LockoutEnd = x.LockoutEnd,
                             LastLogin = x.LastLogin,
                             LastLoginIp = x.LastLoginIp,
                             AvatarUrl = x.AvatarUrl,
                             TotalLogin = x.TotalLogin,
                             TotalLoginFaild = x.TotalLoginFaild,
                             ModerationStatus = x.ModerationStatus,
                             PhanLoai = x.PhanLoai,
                             KeyPublic = x.KeyPublic ?? string.Empty,
                             Created = x.Created
                         })
                         .ToList();

            return (items, total);
        }

        public async Task<(object Items, int Total)> GetPagedByGroupIdAsync(object query)
        {
            var searchQuery = query as UserQuery ?? new UserQuery();
            var (items, total) = GetPagedDto(query);
            return (items, total);
        }
    }
}
