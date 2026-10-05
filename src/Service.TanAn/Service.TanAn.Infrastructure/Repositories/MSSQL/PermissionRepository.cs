// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces.MSSQL;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.Infrastructure.Repositories.MSSQL
{
    public class PermissionRepository : EfRepository<Permission>, IPermissionRepository
    {
        private readonly TanAnDbContext _dbContext;

        public PermissionRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Permission?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<Permission>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public new async Task<Permission> AddAsync(Permission entity)
        {
            await _dbContext.Set<Permission>().AddAsync(entity);
            return entity;
        }

        public void Delete(Permission entity)
        {
            _dbContext.Set<Permission>().Remove(entity);
        }

        public async Task<object> GetPagedDtoAsync(object query)
        {
            var searchQuery = query as PermissionQuery ?? new PermissionQuery();
            var q = _dbContext.Set<Permission>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery.Keyword))
            {
                q = q.Where(x => x.PermissionName.Contains(searchQuery.Keyword) || x.Description.Contains(searchQuery.Keyword));
            }

            int total = await q.CountAsync();
            int pageIndex = searchQuery.PageIndex > 0 ? searchQuery.PageIndex : 1;
            int pageSize = searchQuery.PageSize > 0 ? searchQuery.PageSize : 10;

            var items = await q.Skip((pageIndex - 1) * pageSize)
                               .Take(pageSize)
                               .Select(x => new PermissionDto
                               {
                                   Id = x.Id,
                                   PermissionName = x.PermissionName,
                                   PermissionCode = x.PermissionCode,
                                   Description = x.Description,
                                   Created = x.Created,
                                   IsSync = x.IsSync,
                                   LastModified = x.LastModified ?? x.Created,
                                   ModerationStatus = x.ModerationStatus,
                                   ParentId = x.PermissionParentId
                               })
                               .ToListAsync();

            return (items, total);
        }

        public async Task<object> GetPagedTreeForGanQuyenAsync(object query)
        {
            return await GetPagedDtoAsync(query);
        }
    }
}

