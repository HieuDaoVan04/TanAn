using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Service.Shared.Commons.Helpers;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Application.Services.Core;

public sealed class AccountPasswordService(ITanAnDbContext db, SystemConfigurationService configuration, IHttpContextAccessor http)
{
    public async Task ChangeOwnPasswordAsync(ChangeOwnPasswordForm form, CurrentUserDto actor)
    {
        if (!actor.IsAuthenticated || actor.UserId == Guid.Empty) throw new UnauthorizedAccessException();
        if (string.IsNullOrEmpty(form.CurrentPassword) || form.CurrentPassword.Length > 1024)
            throw new ArgumentException("Hãy nhập mật khẩu hiện tại.");
        var user = await db.Users.FindAsync(actor.UserId) ?? throw new KeyNotFoundException("Tài khoản không tồn tại.");
        var valid = user.PasswordHash.StartsWith("pbkdf2$") ? PasswordHashing.Verify(form.CurrentPassword, user.PasswordHash)
            : user.PasswordHash == form.CurrentPassword || user.PasswordHash == AESCrypto.EncryptNoAutoGen(form.CurrentPassword);
        if (!valid) throw new ArgumentException("Mật khẩu hiện tại không đúng.");
        (await configuration.GetPasswordPolicyAsync()).Validate(form.NewPassword);
        user.PasswordHash = PasswordHashing.Hash(form.NewPassword);
        user.LastModified = DateTime.UtcNow;
        db.AuditLogs.Add(new AuditLog
        {
            Username = actor.UserName, Action = "Đổi mật khẩu cá nhân", EntityName = "Users", EntityId = actor.UserId.ToString(),
            NewValues = JsonSerializer.Serialize(new { PasswordChanged = true }),
            IpAddress = http.HttpContext?.Connection.RemoteIpAddress?.ToString()
        });
        // Thay hash làm mất hiệu lực phiên TanAnSession cũ qua CredentialStamp; không ghi mật khẩu/hash vào audit.
        await db.SaveChangesAsync();
    }
}
