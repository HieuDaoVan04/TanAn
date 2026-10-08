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
    public class SystemParameterRepository : EfRepository<SystemParameter>, ISystemParameterRepository
    {
        private readonly TanAnDbContext _dbContext;

        public SystemParameterRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SystemParameter?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<SystemParameter>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<SystemParameter?> FindAsync(Expression<Func<SystemParameter, bool>> predicate)
        {
            return await _dbContext.Set<SystemParameter>().FirstOrDefaultAsync(predicate);
        }

        public void Delete(SystemParameter entity)
        {
            _dbContext.Set<SystemParameter>().Remove(entity);
        }

        public async Task<(object Items, int Total)> GetPagedDtoAsync(object query)
        {
            var searchQuery = query as BaseQuery ?? new BaseQuery();
            var q = _dbContext.Set<SystemParameter>().AsNoTracking();

            if (query is SystemParameterQuery parameterQuery)
            {
                if (!string.IsNullOrWhiteSpace(parameterQuery.Code)) q = q.Where(x => x.Code == parameterQuery.Code.Trim());
                if (!string.IsNullOrWhiteSpace(parameterQuery.Value)) q = q.Where(x => x.Value != null && x.Value.Contains(parameterQuery.Value));
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Keyword))
            {
                q = q.Where(x => x.Code.Contains(searchQuery.Keyword) || (x.Description != null && x.Description.Contains(searchQuery.Keyword)));
            }

            int total = await q.CountAsync();
            int pageIndex = searchQuery.PageIndex > 0 ? searchQuery.PageIndex : 1;
            int pageSize = searchQuery.PageSize > 0 ? searchQuery.PageSize : 10;

            var items = await q.OrderBy(x => x.Code).ThenBy(x => x.Id).Skip((pageIndex - 1) * pageSize)
                               .Take(pageSize)
                               .Select(x => new SystemParameterDto
                               {
                                   Id = x.Id,
                                   Code = x.Code,
                                   Value = x.Value,
                                   Description = x.Description,
                                   IsSync = x.IsSync,
                                   Created = x.Created,
                                   LastModified = x.LastModified ?? x.Created,
                                   ModerationStatus = x.ModerationStatus
                               })
                               .ToListAsync();

            return (items, total);
        }
    }
}

