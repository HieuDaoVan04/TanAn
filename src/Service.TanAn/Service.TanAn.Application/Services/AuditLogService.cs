// "Một sản phẩm của HieuDV"

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;
using Service.Shared.Commons.Models;

namespace Service.TanAn.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ITanAnDbContext _db;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AuditLogService(ITanAnDbContext db, IHttpContextAccessor? httpContextAccessor = null)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string username, string action, string entityName, string entityId, string? oldValues = null, string? newValues = null, string? ipAddress = null)
        {
            var log = new AuditLog
            {
                Username = username,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? GetClientIpAddress() : ipAddress,
                Timestamp = DateTime.Now
            };

            await _db.AuditLogs.AddAsync(log);
            await _db.SaveChangesAsync();
        }

        public async Task<ApiResult<PagedResult<AuditLog>>> GetAuditLogsAsync(string? keyword, int pageIndex, int pageSize)
        {
            var query = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalizedKeyword = keyword.Trim().ToLower();
                query = query.Where(l =>
                    l.Username.ToLower().Contains(normalizedKeyword) ||
                    l.Action.ToLower().Contains(normalizedKeyword) ||
                    l.EntityName.ToLower().Contains(normalizedKeyword) ||
                    l.EntityId.ToLower().Contains(normalizedKeyword) ||
                    (l.OldValues != null && l.OldValues.ToLower().Contains(normalizedKeyword)) ||
                    (l.NewValues != null && l.NewValues.ToLower().Contains(normalizedKeyword)) ||
                    (l.IpAddress != null && l.IpAddress.ToLower().Contains(normalizedKeyword)));
            }

            int totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(l => l.Timestamp)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return ApiResult<PagedResult<AuditLog>>.Ok(new PagedResult<AuditLog>(items, totalCount, pageIndex, pageSize));
        }

        private string? GetClientIpAddress()
        {
            return _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString();
        }
    }
}


