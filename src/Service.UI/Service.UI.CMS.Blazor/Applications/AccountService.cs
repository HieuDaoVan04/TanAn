using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Helpers;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Model.SQL;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Infrastructure.Persistence;
using Service.TanAn.Application.Services.Core;

namespace Service.UI.CMS.Blazor.Applications;

public sealed class AccountService(IServiceScopeFactory scopes, ILoginSessionStore sessions)
{
    private static string Stamp(string hash) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(hash)));
    public const string SessionClaim = "tanan_session";
    public static bool IsLocalReturnUrl(string? url) => !string.IsNullOrWhiteSpace(url)
        && url.StartsWith('/') && !url.StartsWith("//") && !url.Contains('\\') && !url.Any(char.IsControl);

    public async Task<ClaimsPrincipal?> SignInAsync(string username, string password, string? ip, string? agent)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password) || username.Length > 100 || password.Length > 1024) return null;
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
        var configuration = await SystemConfigurationService.ReadValuesAsync(db);
        var user = await db.Users.SingleOrDefaultAsync(x => x.UserName == username.Trim());
        if (user == null || user.ModerationStatus != ModerationStatus.Approved || user.LockoutEnd > DateTime.UtcNow) return null;
        // Hỗ trợ tài khoản cũ; nâng cấp hash ngay sau lần đăng nhập hợp lệ đầu tiên.
        var valid = user.PasswordHash.StartsWith("pbkdf2$")
            ? PasswordHashing.Verify(password, user.PasswordHash)
            : user.PasswordHash == password || user.PasswordHash == AESCrypto.EncryptNoAutoGen(password);
        if (!valid)
        {
            user.TotalLoginFaild++;
            if (user.TotalLoginFaild >= int.Parse(configuration["KhoaTaiKhoan"]))
            { user.LockoutEnd = DateTime.UtcNow.AddMinutes(int.Parse(configuration["LoginLockoutMinutes"])); user.TotalLoginFaild = 0; }
            await db.SaveChangesAsync();
            return null;
        }
        if (!user.PasswordHash.StartsWith("pbkdf2$")) user.PasswordHash = PasswordHashing.Hash(password);
        user.TotalLoginFaild = 0; user.LockoutEnd = null; user.LastLogin = DateTime.UtcNow; user.TotalLogin++;
        await db.SaveChangesAsync();
        var session = new LoginSession(Guid.NewGuid().ToString("N"), user.Id, user.UserName,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(int.Parse(configuration["MinuteExpireToken"])), ip, agent?[..Math.Min(agent.Length, 256)], Stamp(user.PasswordHash));
        await sessions.CreateAsync(session);
        return new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString()), new Claim(SessionClaim, session.Id),
            new Claim("tanan_session_expires", session.ExpiresAt.ToUnixTimeSeconds().ToString())
        }, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public async Task<CurrentUserDto> GetCurrentAsync(ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(SessionClaim);
        if (principal.Identity?.IsAuthenticated != true || id == null) return new();
        var session = await sessions.FindAsync(id);
        if (session == null || principal.FindFirstValue(ClaimTypes.NameIdentifier) != session.UserId.ToString()) return new();
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == session.UserId);
        if (user == null || user.ModerationStatus != ModerationStatus.Approved || user.LockoutEnd > DateTime.UtcNow) return new();
        if (session.CredentialStamp != Stamp(user.PasswordHash)) return new();
        var menus = await db.Modules.AsNoTracking().Where(x => x.ModerationStatus == ModerationStatus.Approved
            && (x.PhanHeId == null || x.PhanHe!.HoatDong)).ToListAsync();
        var villageIds = await db.PhuTrachThons.Where(p => p.UserId == user.Id && p.Thon.DangHoatDong).Select(p => p.ApThonId).ToListAsync();
        var roleCodes = await (from ur in db.UserRoles join role in db.Roles on ur.RoleId equals role.Id
            where ur.UserId == user.Id && role.ModerationStatus == ModerationStatus.Approved
            select role.RoleCode).Distinct().ToListAsync();
        // Menu truy cập lấy từ vai trò đã gán, không suy ra từ loại tài khoản hoặc URL.
        var grants = await (from ur in db.UserRoles join role in db.Roles on ur.RoleId equals role.Id
            join rm in db.RoleModules on role.Id equals rm.RoleId
            where ur.UserId == user.Id && role.ModerationStatus == ModerationStatus.Approved
            select rm.ModuleId).Distinct().ToListAsync();
        menus = menus.Where(x => grants.Contains(x.Id)).ToList();
        if (user.Role == RoleEnum.CanBoThon && villageIds.Count == 0) menus.Clear();
        return new CurrentUserDto {
            UserId = user.Id, UserName = user.UserName, FullName = user.FullName, Email = user.Email,
            VillageIds = villageIds, Role = user.Role.ToString(), IsAuthenticated = true, ApThon = user.ApThon ?? "",
            RoleCodes = roleCodes.Select(code => code.Trim().ToLowerInvariant()).Distinct().ToList(),
            MenusActive = menus.Select(x => new MenuItemDto { Id = x.Id, Title = x.TenModule,
                Path = x.LienKet ?? "", Icon = x.Icon, Order = x.ViTri }).ToList()
        };
    }
}
