using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Service.Shared.Commons.Interfaces;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.UI.CMS.Blazor.Applications;

public sealed class SessionAdministrationService(IUserService users, ILoginSessionStore sessions, IServiceScopeFactory scopes, IHttpContextAccessor httpContextAccessor)
{
    public async Task<IReadOnlyList<LoginSession>> ListAsync()
    {
        var user = await users.GetCurrentUserAsync();
        if (!user.IsAuthenticated || user.Role != "Admin") throw new UnauthorizedAccessException("Bạn chưa có quyền quản lý phiên.");
        return await sessions.ListAsync();
    }
    public async Task RevokeAsync(string id)
    {
        var user = await users.GetCurrentUserAsync();
        if (!user.IsAuthenticated || user.Role != "Admin") throw new UnauthorizedAccessException("Bạn chưa có quyền quản lý phiên.");
        var target = await sessions.FindAsync(id);
        if (target == null) return;
        await sessions.RevokeAsync(id);
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
        db.AuditLogs.Add(new AuditLog
        {
            Username = user.UserName,
            Action = "Thu hồi phiên đăng nhập",
            EntityName = "LoginSession",
            EntityId = target.UserId.ToString(),
            OldValues = JsonSerializer.Serialize(new
            {
                target.Username,
                target.CreatedAt,
                target.ExpiresAt,
                target.IpAddress,
                target.UserAgent
            }),
            NewValues = JsonSerializer.Serialize(new { Revoked = true }),
            IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        });
        await db.SaveChangesAsync();
    }
}
