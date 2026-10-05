# Quản lý menu từ database

Ứng dụng không chạy seeder. API và Blazor chỉ kiểm tra schema khi khởi động; không tạo tài khoản, dữ liệu mẫu, menu hoặc quyền và không đổi đường dẫn, thứ tự hay trạng thái đã lưu.

## Luồng tương ứng LGSP

1. Mỗi trang `.razor` khai báo `@page` riêng. Khai báo route không tự tạo menu.
2. Quản trị viên thêm/sửa menu và nhập đường dẫn của trang tại `/quan-tri-he-thong/quan-tri-menu`.
3. Gán menu cho vai trò tại `/quan-tri-he-thong/vai-tro`, rồi gán vai trò cho tài khoản tại `/quan-tri-he-thong/nguoi-dung`.
4. `AccountService` đọc `UserRoles → Roles → RoleModules` để lấy `MenusActive`. Loại tài khoản Admin không tự cấp toàn bộ menu; cán bộ thôn vẫn phải có địa bàn phụ trách.
5. `MenuTreeService` đọc menu đã duyệt thuộc phân hệ đang hoạt động. `NavMenu` lọc theo quyền và dựng cây theo cha/con, thứ tự, icon và liên kết đã lưu. Không trộn menu từ registry hoặc thay đường dẫn lúc đọc.

Tân An tiếp tục sử dụng các bảng hiện có: `Module` lưu menu (tương ứng `MenuQuanTri` của LGSP), `PhanHe` lưu phân hệ. Không đổi tên bảng hay ID và không cần migration schema cho thay đổi này. Blazor Server gọi service trong scope riêng; không thay toàn bộ hạ tầng thành API của LGSP.

## Cấu hình và dữ liệu hiện có

- Đăng nhập bằng tài khoản đã được cấp trong database. Không còn tài khoản/mật khẩu mẫu tự sinh.
- Admin có thể mở trực tiếp các trang quản trị bằng URL để cấu hình kể cả khi chưa có menu. Quyền quản trị này độc lập với danh sách menu hiển thị.
- Bảng menu trống hoặc tài khoản chưa được gán menu thì thanh menu trống; ứng dụng không tự nạp cây dự phòng.
- Mỗi màn hình chỉ có một route chính ngay trong file `.razor`. Không dùng route alias `/quan-tri-he-thong/danh-muc/...`, endpoint chuyển hướng hay trang dùng tham số để chọn UI.
- Khi thêm menu, nên dùng đường dẫn chính trong bảng dưới. Menu nhóm để trống đường dẫn.

| Trang | Đường dẫn chính |
| --- | --- |
| Đơn vị/hệ thống | `/quan-tri-he-thong/don-vi-he-thong` |
| Người dùng | `/quan-tri-he-thong/nguoi-dung` |
| Vai trò | `/quan-tri-he-thong/vai-tro` |
| Tham số | `/quan-ly-tham-so-ht` |
| Phân hệ/module | `/quan-tri-he-thong/module` |
| Menu | `/quan-tri-he-thong/quan-tri-menu` |
| Phiên đăng nhập | `/quan-tri-he-thong/quan-ly-phien` |
| Nhật ký | `/audit-logs` |

## Kiểm tra

Các công cụ dùng SQLite trong RAM, không kết nối database triển khai:

```powershell
dotnet run --project Tools/EntityModelChecks/EntityModelChecks.csproj -- --menu-only
dotnet run --project Tools/LoginChecks/LoginChecks.csproj
dotnet run --project Tools/MenuAccessChecks/MenuAccessChecks.csproj
./Tools/CheckPageRoutes.ps1
```

Kiểm tra CRUD và gán quyền, database menu trống không tự sinh bản ghi, đọc cây giữ nguyên cấu hình, quyền theo vai trò đã duyệt, đăng nhập/thu hồi phiên và route riêng không trùng.
## Mã nguồn từng trang quản trị

Mỗi thư mục `Components/Pages/QuanTriHeThong/NguoiDung`, `DonViHeThong`, `VaiTro`, `ThamSoHeThong`, `Module`, `Menu` có bộ `Index.razor`, `Index.razor.cs`, `Index.razor.css` riêng. `Index.razor` chứa trực tiếp bảng/cây, bộ lọc, form và sự kiện của chức năng; `Index.razor.cs` xử lý tải/lưu, trạng thái và quyền gán liên quan. Không còn `AdminCatalogView` hoặc tham số `Catalog` chọn màn hình. Chỉ các nút thao tác và dialog nhỏ dùng lại component chung.
