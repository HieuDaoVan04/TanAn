// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces.MSSQL;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.Infrastructure.Repositories.MSSQL
{
    public class RoleRepository : EfRepository<Role>, IRoleRepository
    {
        private readonly TanAnDbContext _dbContext;

        public RoleRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Role?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<Role>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Role?> FindAsync(Expression<Func<Role, bool>> predicate)
        {
            return await _dbContext.Set<Role>().FirstOrDefaultAsync(predicate);
        }

        public void Delete(Role entity)
        {
            _dbContext.Set<Role>().Remove(entity);
        }

        public async Task<(object Items, int Total)> GetPagedDtoAsync(object query)
        {
            var searchQuery = query as RoleQuery ?? new RoleQuery();
            var q = _dbContext.Set<Role>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery.Keyword))
            {
                q = q.Where(x => x.RoleName.Contains(searchQuery.Keyword) || x.RoleCode.Contains(searchQuery.Keyword));
            }
            if (!string.IsNullOrWhiteSpace(searchQuery.RoleName))
            {
                q = q.Where(x => x.RoleName.Contains(searchQuery.RoleName));
            }
            if (!string.IsNullOrWhiteSpace(searchQuery.RoleCode))
            {
                q = q.Where(x => x.RoleCode.Contains(searchQuery.RoleCode));
            }

            int total = await q.CountAsync();
            int pageIndex = searchQuery.PageIndex > 0 ? searchQuery.PageIndex : 1;
            int pageSize = searchQuery.PageSize > 0 ? searchQuery.PageSize : 10;

            var items = await q.Skip((pageIndex - 1) * pageSize)
                               .Take(pageSize)
                               .Select(x => new RoleDto
                               {
                                   Id = x.Id,
                                   RoleName = x.RoleName,
                                   RoleCode = x.RoleCode,
                                   Mota = x.Mota,
                                   IsSync = x.IsSync,
                                   Created = x.Created,
                                   LastModified = x.LastModified ?? x.Created,
                                   ModerationStatus = x.ModerationStatus
                               })
                               .ToListAsync();

            return (items, total);
        }

        public (object Items, int Total) GetPagedForGanQuyen(Guid id, object query)
        {
            var searchQuery = query as RoleQuery ?? new RoleQuery();
            var q = _dbContext.Set<Role>().AsQueryable();

            int total = q.Count();
            int pageIndex = searchQuery.PageIndex > 0 ? searchQuery.PageIndex : 1;
            int pageSize = searchQuery.PageSize > 0 ? searchQuery.PageSize : 10;

            var items = q.Skip((pageIndex - 1) * pageSize)
                         .Take(pageSize)
                         .Select(x => new RoleDto
                         {
                             Id = x.Id,
                             RoleName = x.RoleName,
                             RoleCode = x.RoleCode,
                             Mota = x.Mota,
                             IsSync = x.IsSync,
                             Created = x.Created,
                             LastModified = x.LastModified ?? x.Created,
                             ModerationStatus = x.ModerationStatus
                         })
                         .ToList();

            return (items, total);
        }
    }
}

