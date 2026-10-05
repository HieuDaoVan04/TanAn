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
    public class LogThaoTacNguoiDungRepository : EfRepository<LogThaoTacNguoiDung>, ILogThaoTacNguoiDungRepository
    {
        private readonly TanAnDbContext _dbContext;

        public LogThaoTacNguoiDungRepository(TanAnDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LogThaoTacNguoiDung?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<LogThaoTacNguoiDung>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<object> GetPagedDtoAsync(object query)
        {
            var logQuery = query as LogThaoTacNguoiDungQuery ?? new LogThaoTacNguoiDungQuery();
            var q = _dbContext.Set<LogThaoTacNguoiDung>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(logQuery.Keyword))
            {
                q = q.Where(x => (x.Description != null && x.Description.Contains(logQuery.Keyword)) ||
                                 (x.ModuleName != null && x.ModuleName.Contains(logQuery.Keyword)) ||
                                 (x.Action != null && x.Action.Contains(logQuery.Keyword)));
            }

            if (!string.IsNullOrWhiteSpace(logQuery.UserId))
            {
                q = q.Where(x => x.UserId == logQuery.UserId);
            }

            int total = await q.CountAsync();
            int pageIndex = logQuery.PageIndex > 0 ? logQuery.PageIndex : 1;
            int pageSize = logQuery.PageSize > 0 ? logQuery.PageSize : 10;

            var items = await q.Skip((pageIndex - 1) * pageSize)
                               .Take(pageSize)
                               .Select(x => new LogThaoTacNguoiDungDto
                               {
                                   Id = x.Id,
                                   UserId = x.UserId,
                                   ModuleName = x.ModuleName,
                                   Action = x.Action,
                                   Created = x.Created,
                                   BeforeChange = x.BeforeChange,
                                   AfterChange = x.AfterChange,
                                   IPAddress = x.IPAddress,
                                   Description = x.Description
                               })
                               .ToListAsync();

            return (items, total);
        }
    }
}
