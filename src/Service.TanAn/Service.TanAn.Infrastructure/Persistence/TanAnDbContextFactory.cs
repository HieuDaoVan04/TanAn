using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Service.TanAn.Infrastructure.Persistence;

/// <summary>
/// Design-time factory cho EF Core. Factory chỉ đọc cùng DefaultConnection của API,
/// không dùng database hoặc connection string riêng cho migration.
/// </summary>
public class TanAnDbContextFactory : IDesignTimeDbContextFactory<TanAnDbContext>
{
    public TanAnDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var apiDirectory = FindApiDirectory();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .Build();

        var connection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Không tìm thấy ConnectionStrings:DefaultConnection trong cấu hình Service.TanAn.API.");

        return new TanAnDbContext(
            new DbContextOptionsBuilder<TanAnDbContext>()
                .UseNpgsql(connection)
                .Options);
    }

    private static string FindApiDirectory()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var fromSolutionRoot = Path.Combine(
                current.FullName,
                "src",
                "Service.TanAn",
                "Service.TanAn.API");

            if (File.Exists(Path.Combine(fromSolutionRoot, "appsettings.json")))
                return fromSolutionRoot;

            var siblingApi = Path.Combine(current.FullName, "Service.TanAn.API");
            if (File.Exists(Path.Combine(siblingApi, "appsettings.json")))
                return siblingApi;

            if (current.Name.Equals("Service.TanAn.API", StringComparison.OrdinalIgnoreCase)
                && File.Exists(Path.Combine(current.FullName, "appsettings.json")))
                return current.FullName;

            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Không tìm thấy thư mục Service.TanAn.API để đọc appsettings.json.");
    }
}
