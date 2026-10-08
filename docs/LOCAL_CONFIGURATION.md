# Cấu hình sau khi clone repository

Hai file `appsettings.json` của API và Blazor được bỏ qua trong Git vì cấu hình trên máy có thông tin kết nối riêng. Các file đang có trên máy được giữ nguyên.

Trên máy mới, chạy từ thư mục gốc repository:

```powershell
Copy-Item src/Service.TanAn/Service.TanAn.API/appsettings.example.json src/Service.TanAn/Service.TanAn.API/appsettings.json
Copy-Item src/Service.UI/Service.UI.CMS.Blazor/appsettings.example.json src/Service.UI/Service.UI.CMS.Blazor/appsettings.json
```

Chỉ chạy các lệnh sao chép khi chưa có cấu hình riêng cần giữ. Cấu hình mẫu sử dụng SQLite local và tắt Redis. Đổi `Jwt:SecretKey` thành khóa ngẫu nhiên riêng, tối thiểu 32 ký tự, trước khi chạy API.

Nếu dùng PostgreSQL, đặt `ConnectionStrings:DefaultConnection` cho cả API và Blazor trỏ đến cùng database. Trong môi trường Development có thể lưu chuỗi kết nối và khóa JWT bằng `dotnet user-secrets set --project <đường-dẫn-project> <tên-khóa> <giá-trị>`. Hai project hiện dùng cùng `UserSecretsId`, vì vậy chia sẻ các giá trị User Secrets.

Khi triển khai, có thể dùng biến môi trường `ConnectionStrings__DefaultConnection`, `Jwt__SecretKey` và `Redis__Password`. Không đưa giá trị thật vào cấu hình mẫu hoặc tài liệu.

Git bỏ qua `bin/`, `obj/`, `.vs/`, `.codex-build/`, `artifacts/`, log, database SQLite local, thiết lập IDE cá nhân và các bản render báo cáo trung gian. Mã nguồn migrations, công cụ, tài liệu và tài nguyên giao diện vẫn được quản lý trong Git.
