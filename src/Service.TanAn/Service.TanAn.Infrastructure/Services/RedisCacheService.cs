using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Service.Shared.Commons.Interfaces;
using StackExchange.Redis;

namespace Service.TanAn.Infrastructure.Services;

public sealed class RedisCacheService : ICacheService, ILoginSessionStore, IDisposable
{
    private readonly Lazy<Task<ConnectionMultiplexer>> _connection;
    private readonly string _prefix;

    public RedisCacheService(IConfiguration configuration)
    {
        var host = configuration["Redis:Host"];
        var password = configuration["Redis:Password"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Thiếu Redis:Host hoặc Redis:Password. Hãy cấu hình User Secrets cho Redis cá nhân.");
        _prefix = configuration["Redis:KeyPrefix"] ?? "tanan:";
        var options = new ConfigurationOptions
        {
            User = configuration["Redis:User"] ?? "default",
            Password = password,
            Ssl = configuration.GetValue<bool>("Redis:Ssl"),
            AbortOnConnectFail = false,
            ConnectTimeout = 5000,
            AsyncTimeout = 5000
        };
        options.EndPoints.Add(host, configuration.GetValue<int>("Redis:Port"));
        _connection = new(() => ConnectionMultiplexer.ConnectAsync(options));
    }

    private string Key(RedisTypeKey type, string key) => $"{_prefix}{type}:{key}";
    private async Task<IDatabase> DatabaseAsync() => (await _connection.Value).GetDatabase();
    private string SessionKey(string id) => Key(RedisTypeKey.Session, "login:" + id);
    private string SessionIndex => _prefix + "login-session-index";
    public async Task CreateAsync(LoginSession session)
    {
        var db = await DatabaseAsync();
        var ttl = session.ExpiresAt - DateTimeOffset.UtcNow;
        if (ttl <= TimeSpan.Zero) throw new ArgumentException("Phiên đã hết hạn.");
        var transaction = db.CreateTransaction();
        _ = transaction.StringSetAsync(SessionKey(session.Id), JsonSerializer.Serialize(session), ttl);
        _ = transaction.SortedSetAddAsync(SessionIndex, session.Id, session.ExpiresAt.ToUnixTimeSeconds());
        if (!await transaction.ExecuteAsync()) throw new InvalidOperationException("Không thể tạo phiên Redis.");
    }
    public async Task<LoginSession?> FindAsync(string id)
    {
        if (!Guid.TryParseExact(id, "N", out _)) return null;
        var value = await (await DatabaseAsync()).StringGetAsync(SessionKey(id));
        var session = value.IsNull ? null : JsonSerializer.Deserialize<LoginSession>(value.ToString());
        return session?.ExpiresAt > DateTimeOffset.UtcNow ? session : null;
    }
    public async Task<IReadOnlyList<LoginSession>> ListAsync()
    {
        var db = await DatabaseAsync();
        await db.SortedSetRemoveRangeByScoreAsync(SessionIndex, double.NegativeInfinity, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        var ids = await db.SortedSetRangeByRankAsync(SessionIndex, 0, 199, Order.Descending);
        if (ids.Length == 0) return Array.Empty<LoginSession>();
        var values = await db.StringGetAsync(ids.Select(x => (RedisKey)SessionKey(x.ToString())).ToArray());
        return values.Where(x => !x.IsNull).Select(x => JsonSerializer.Deserialize<LoginSession>(x.ToString())!)
            .Where(x => x.ExpiresAt > DateTimeOffset.UtcNow).ToList();
    }
    public async Task RevokeAsync(string id)
    {
        if (!Guid.TryParseExact(id, "N", out _)) throw new ArgumentException("Phiên không hợp lệ.");
        var db = await DatabaseAsync();
        await db.KeyDeleteAsync(SessionKey(id));
        await db.SortedSetRemoveAsync(SessionIndex, id);
    }
    public async Task<T?> GetAsync<T>(RedisTypeKey typeKey, string key)
    {
        var value = await (await DatabaseAsync()).StringGetAsync(Key(typeKey, key));
        return value.IsNull ? default : JsonSerializer.Deserialize<T>(value.ToString());
    }
    public async Task SetAsync<T>(RedisTypeKey typeKey, string key, T value, TimeSpan? expiration = null)
    {
        await (await DatabaseAsync()).StringSetAsync(Key(typeKey, key), JsonSerializer.Serialize(value), expiration);
    }
    public async Task<bool> KeyExistsAsync(RedisTypeKey typeKey, string key)
        => await (await DatabaseAsync()).KeyExistsAsync(Key(typeKey, key));
    public async Task RemoveAsync(RedisTypeKey typeKey, string key)
        => await (await DatabaseAsync()).KeyDeleteAsync(Key(typeKey, key));
    public void Dispose()
    {
        if (_connection.IsValueCreated)
            _ = DisposeConnectionAsync(_connection.Value);
    }
    private static async Task DisposeConnectionAsync(Task<ConnectionMultiplexer> task)
    {
        try { (await task).Dispose(); }
        catch { /* Không có kết nối để giải phóng khi khởi tạo thất bại. */ }
    }
}
