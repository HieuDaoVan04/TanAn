using Microsoft.Extensions.Configuration;
using Service.Shared.Commons.Interfaces;
using Service.TanAn.Infrastructure.Services;
using System.Xml.Linq;

// Chạy từ thư mục gốc dự án. Không truy cập PostgreSQL hoặc in thông tin xác thực.
foreach (var relative in new[] { "src/Service.TanAn/Service.TanAn.API", "src/Service.UI/Service.UI.CMS.Blazor" })
{
    var directory = Path.GetFullPath(relative);
    var project = Directory.GetFiles(directory, "*.csproj").Single();
    var id = XDocument.Load(project).Descendants("UserSecretsId").Single().Value;
    var secrets = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", id, "secrets.json");
    var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(directory, "appsettings.json"))
        .AddJsonFile(secrets, optional: true).Build();
    var name = Path.GetFileName(directory);
    if (string.IsNullOrWhiteSpace(config["Redis:Password"]))
    {
        Console.WriteLine($"FAIL {name}: Redis password missing in User Secrets.");
        Environment.ExitCode = 1;
        continue;
    }
    // Chỉ kết nối Redis cá nhân đã được người dùng chỉ định.
    if (config["Redis:Host"] != "abloom-wealth-mass-64373.db.redis.io" || config["Redis:Port"] != "10605")
        throw new InvalidOperationException("Unexpected Redis destination; check stopped.");
    using var cache = new RedisCacheService(config);
    var key = "connection-check:" + Guid.NewGuid().ToString("N");
    try
    {
        await cache.SetAsync(RedisTypeKey.Cache, key, "tanan-check", TimeSpan.FromSeconds(30));
        if (await cache.GetAsync<string>(RedisTypeKey.Cache, key) != "tanan-check")
            throw new InvalidOperationException("Roundtrip mismatch");
        await cache.RemoveAsync(RedisTypeKey.Cache, key);
        if (await cache.KeyExistsAsync(RedisTypeKey.Cache, key))
            throw new InvalidOperationException("Cleanup failed");
        Console.WriteLine($"PASS {name}: authentication, write, read and delete temporary key.");
    }
    catch (Exception ex)
    {
        // Không in exception message có thể chứa dữ liệu cấu hình.
        Console.WriteLine($"FAIL {name}: {ex.GetType().Name}. Temporary key expires within 30 seconds.");
        var details = ex.ToString();
        foreach (var marker in new[] { "WRONGPASS", "NOAUTH", "AuthenticationFailure", "SocketFailure", "Timeout", "SSL", "TLS" })
            if (details.Contains(marker, StringComparison.OrdinalIgnoreCase)) Console.WriteLine($"Diagnostic: {marker}");
        Environment.ExitCode = 1;
    }
}
