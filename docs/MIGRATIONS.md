# EF Core Migrations — Tân An

## Trạng thái sau khi làm lại DB thử nghiệm

Database `neondb` được cấu hình chung cho API/Blazor đã được làm lại theo yêu cầu người dùng:
áp dụng `InitialDomainModel` và `AlignRuntimeTimestampMapping`, nạp 2 tài khoản, 2 hộ,
3 nhân khẩu, 1 đối tượng an sinh và 21 menu. Đã kiểm tra lại sau commit; lịch sử có 2 migration.
Không cần chạy lại migration đầu tiên. Tool reset dùng một lần đã được xóa khỏi dự án.
EF Core migration dùng cùng `ConnectionStrings:DefaultConnection` của `Service.TanAn.API`; không có database migration riêng.
Các hướng dẫn database cũ bên dưới chỉ áp dụng cho những bản sao chưa được chuyển đổi.

## Thiết lập đã có

- Provider: PostgreSQL, theo cấu hình API/Blazor đang có.
- DbContext: TanAnDbContext.
- Project chứa migration: src/Service.TanAn/Service.TanAn.Infrastructure.
- Startup project cho EF: src/Service.TanAn/Service.TanAn.API.
- Migration đầu tiên: InitialDomainModel, gồm model nghiệp vụ và các bảng quản trị hiện có.
- `TanAnDbContextFactory` chỉ phục vụ EF design-time và đọc `DefaultConnection` từ cấu hình API; không có connection string migration riêng.
- DatabaseSchemaValidator chỉ kiểm tra provider và migration chưa áp dụng khi khởi động. Không tự chạy migration hoặc seed dữ liệu.
- SQLite chỉ được dùng trong công cụ kiểm tra model. Bộ migration này không dùng cho SQLite.

Các lệnh dưới đây chạy ở `D:\Đồ Án` bằng PowerShell.

```powershell
$efProject = 'src/Service.TanAn/Service.TanAn.Infrastructure'
$startupProject = 'src/Service.TanAn/Service.TanAn.API'
dotnet ef --version
```

EF Core runtime của dự án là 9.0.2. Máy hiện có dotnet-ef 10.0.11 và đã sinh migration thành công.
Khi thiết lập máy khác nên dùng dotnet-ef tương thích với phiên bản EF Core của dự án.

## Tạo database mới (trống)

EF sử dụng `ConnectionStrings:DefaultConnection` của startup project `Service.TanAn.API`.
Hãy kiểm tra connection string của môi trường đang chạy trước khi cập nhật database.

```powershell
dotnet ef database update --project $efProject --startup-project $startupProject --context TanAnDbContext
```

API và Blazor cần cùng trỏ `DefaultConnection` đến database tương ứng.
Chỉ khởi động ứng dụng sau khi lệnh cập nhật thành công.

## Database đã có dữ liệu từ EnsureCreated

**Không chạy InitialDomainModel trực tiếp lên database đang có bảng.** EF chưa có lịch sử migration
nên sẽ cố tạo lại bảng và báo trùng. Cũng không chỉ thêm một dòng vào `__EFMigrationsHistory`:
schema cũ chưa có các bảng/cột/ràng buộc mới.

Quy trình: sao lưu → kiểm kê schema và dữ liệu thật → lập baseline tương ứng schema cũ → tạo migration
nâng cấp từ baseline → xử lý dữ liệu trùng/không hợp lệ → thử trên bản sao → áp dụng database đích.
Migration initial hiện tại dành cho database trống. Chưa kiểm kê hay nâng cấp database từ xa.

## Sau khi thay đổi entity

```powershell
dotnet ef migrations add TenThayDoi --project $efProject --startup-project $startupProject --context TanAnDbContext --output-dir Persistence/Migrations
dotnet ef migrations has-pending-model-changes --project $efProject --startup-project $startupProject --context TanAnDbContext
```

Đọc `Up`/`Down` của migration trước khi cập nhật database. Không sửa migration đã áp dụng ở môi trường dùng chung.

## Xuất SQL để kiểm tra trước

```powershell
dotnet ef migrations script --idempotent --project $efProject --startup-project $startupProject --context TanAnDbContext --output artifacts/tanan-migrations.sql
```

Script idempotent dựa trên `__EFMigrationsHistory`, không tự nhận biết schema được tạo bằng EnsureCreated.
Sinh migration/script và kiểm tra snapshot không kết nối database. `database update` mới thay đổi database.
