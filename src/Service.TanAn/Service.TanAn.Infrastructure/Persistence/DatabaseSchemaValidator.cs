using Microsoft.EntityFrameworkCore;

namespace Service.TanAn.Infrastructure.Persistence;

/// <summary>Chỉ kiểm tra schema; không tạo hoặc sửa dữ liệu khi khởi động.</summary>
public static class DatabaseSchemaValidator
{
    public static async Task ValidateAsync(TanAnDbContext db)
    {
        if (!db.Database.IsNpgsql())
            throw new InvalidOperationException("Bộ migration hiện tại dành cho PostgreSQL. Cấu hình PostgreSQL theo docs/MIGRATIONS.md.");
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("Database chưa cập nhật schema. Xem docs/MIGRATIONS.md và chạy dotnet ef database update trước khi khởi động.");
    }
}