namespace Service.Shared.Commons.Interfaces;

public sealed record LoginSession(string Id, Guid UserId, string Username, DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt, string? IpAddress, string? UserAgent, string CredentialStamp = "");

public interface ILoginSessionStore
{
    Task CreateAsync(LoginSession session);
    Task<LoginSession?> FindAsync(string id);
    Task<IReadOnlyList<LoginSession>> ListAsync();
    Task RevokeAsync(string id);
}
