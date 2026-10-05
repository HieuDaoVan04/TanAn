// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Interfaces.Elastic;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.Infrastructure.Repositories.Elastic
{
    public class LogHeThongIndexRepository : ILogHeThongIndexRepository
    {
        private readonly TanAnDbContext _dbContext;

        public LogHeThongIndexRepository(TanAnDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> GetCountData(object query)
        {
            return await _dbContext.AuditLogs.CountAsync();
        }

        public async Task<DataTableJson> GetPaged(object query)
        {
            var logQuery = query as LogHeThongQuery ?? new LogHeThongQuery();
            var q = _dbContext.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(logQuery.Keyword))
            {
                var keyword = logQuery.Keyword.Trim().ToLower();
                q = q.Where(x =>
                    x.Username.ToLower().Contains(keyword) ||
                    x.Action.ToLower().Contains(keyword) ||
                    (x.EntityName != null && x.EntityName.ToLower().Contains(keyword)) ||
                    (x.EntityId != null && x.EntityId.ToLower().Contains(keyword)) ||
                    (x.IpAddress != null && x.IpAddress.ToLower().Contains(keyword)) ||
                    (x.OldValues != null && x.OldValues.ToLower().Contains(keyword)) ||
                    (x.NewValues != null && x.NewValues.ToLower().Contains(keyword)));
            }

            if (logQuery.TimKiemTuNgay.HasValue)
            {
                var from = logQuery.TimKiemTuNgay.Value.Date;
                q = q.Where(x => x.Timestamp >= from);
            }

            if (logQuery.TimKiemDenNgay.HasValue)
            {
                var toExclusive = logQuery.TimKiemDenNgay.Value.Date.AddDays(1);
                q = q.Where(x => x.Timestamp < toExclusive);
            }

            int total = await q.CountAsync();
            int pageIndex = logQuery.PageIndex > 0 ? logQuery.PageIndex : 1;
            int pageSize = logQuery.PageSize > 0 ? logQuery.PageSize : 10;

            var items = await q.OrderByDescending(x => x.Timestamp)
                               .Skip((pageIndex - 1) * pageSize)
                               .Take(pageSize)
                               .Select(x => new LogHeThongDto
                               {
                                   Id = x.Id.ToString(),
                                   LogLevel = "Info",
                                   Message = $"{x.Action} {x.EntityName} ({x.EntityId})",
                                   Action = x.Action,
                                   EntityName = x.EntityName,
                                   EntityId = x.EntityId,
                                   OldValues = x.OldValues,
                                   NewValues = x.NewValues,
                                   UserName = x.Username,
                                   ClientIP = x.IpAddress,
                                   CreatedDate = x.Timestamp
                               })
                               .Cast<object>()
                               .ToListAsync();

            return new DataTableJson(items, total, pageIndex, pageSize);
        }

        public async Task<object?> GetByIdAsync(string id)
        {
            if (Guid.TryParse(id, out var guid))
            {
                var auditLog = await _dbContext.AuditLogs.FirstOrDefaultAsync(x => x.Id == guid);
                if (auditLog != null)
                {
                    return new LogHeThongDto
                    {
                        Id = auditLog.Id.ToString(),
                        LogLevel = "Info",
                        Message = $"{auditLog.Action} {auditLog.EntityName} ({auditLog.EntityId})",
                        Action = auditLog.Action,
                        EntityName = auditLog.EntityName,
                        EntityId = auditLog.EntityId,
                        OldValues = auditLog.OldValues,
                        NewValues = auditLog.NewValues,
                        UserName = auditLog.Username,
                        ClientIP = auditLog.IpAddress,
                        CreatedDate = auditLog.Timestamp
                    };
                }
            }

            return null;
        }
    }
}
