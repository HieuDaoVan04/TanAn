using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.API.Authentication;

public sealed class LoginSessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
    ILoginSessionStore sessions, TanAnDbContext db)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TanAnSession";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var header)
            || !string.Equals(header.Scheme, SchemeName, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();
        if (!Guid.TryParseExact(header.Parameter, "N", out _))
            return AuthenticateResult.Fail("Phiên không hợp lệ.");
        var session = await sessions.FindAsync(header.Parameter!);
        if (session == null || session.ExpiresAt <= DateTimeOffset.UtcNow)
            return AuthenticateResult.Fail("Phiên đã hết hạn hoặc bị thu hồi.");
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == session.UserId);
        if (user == null || user.ModerationStatus != ModerationStatus.Approved || user.LockoutEnd > DateTime.UtcNow)
            return AuthenticateResult.Fail("Tài khoản không còn hoạt động.");
        var stamp = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(user.PasswordHash)));
        if (stamp != session.CredentialStamp)
            return AuthenticateResult.Fail("Thông tin đăng nhập đã thay đổi.");
        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        }, SchemeName);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName));
    }
}
