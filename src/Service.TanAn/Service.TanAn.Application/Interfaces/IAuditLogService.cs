using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;
using Service.Shared.Commons.Models;

namespace Service.TanAn.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string username, string action, string entityName, string entityId, string? oldValues = null, string? newValues = null, string? ipAddress = null);
        Task<ApiResult<PagedResult<AuditLog>>> GetAuditLogsAsync(string? keyword, int pageIndex, int pageSize);
    }
}


