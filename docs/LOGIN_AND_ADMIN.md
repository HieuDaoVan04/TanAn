# Đăng nhập và quản trị Tân An

## Chạy ứng dụng

Dừng bản đang chạy trong Visual Studio/IIS Express rồi build và chạy lại ở Development.
Redis cá nhân dùng User Secrets đã cấu hình cho API và Blazor.
Không cần migration schema cho thay đổi đăng nhập/menu này.

Truy cập `/` hoặc `/ban-lam-viec` khi chưa đăng nhập sẽ chuyển tới `/account/login`.
Đăng nhập bằng tài khoản trong bảng Users. Tài khoản thử nghiệm admin hiện có được giữ nguyên.
Mật khẩu cũ được nâng cấp sang PBKDF2 khi đăng nhập hợp lệ. Không dùng mật khẩu Redis để đăng nhập ứng dụng.

## Cây quản trị

Quản trị hệ thống có sáu mục trực tiếp:

1. Quản lý đơn vị/hệ thống: cây nhóm, mã, tên, loại, trạng thái.
2. Quản lý người dùng: tài khoản, email, mật khẩu mới, vai trò, trạng thái.
3. Quản lý vai trò: mã/tên và danh sách menu được gán.
4. Tham số hệ thống: mã, giá trị và mô tả.
5. Quản lý menu: cha/con, module, liên kết, icon, vị trí, trạng thái.
6. Quản lý phiên: phiên đăng nhập Blazor lưu trên Redis, tìm theo tài khoản, thu hồi có xác nhận.

Không có seeder. Cây menu, thứ tự và trạng thái do quản trị viên cấu hình trong database; xem MENU_TREE.md.
Danh mục module cũ vẫn có tại `/quan-tri-he-thong/module`.
Ba mục an sinh: hộ nghèo, hộ cận nghèo, người cao tuổi, lọc theo loại đối tượng của dữ liệu hiện có.

## Xác thực và quyền

- Cookie HttpOnly, cùng-site Lax; dùng Secure trên HTTPS. Cookie/phiên hết hạn sau một giờ.
- Form đăng nhập và đăng xuất kiểm tra antiforgery. Chuyển hướng sau đăng nhập chỉ tới đường dẫn nội bộ.
- Giới hạn đăng nhập 10 yêu cầu/phút/IP và khóa tài khoản 15 phút sau 5 lần sai mật khẩu.
- Tài khoản không duyệt/bị khóa, phiên hết hạn/thu hồi hoặc đổi mật khẩu sẽ không còn hợp lệ.
- Trang đang mở kiểm tra lại phiên mỗi 30 giây. Thao tác quản trị kiểm tra lại danh tính trước mỗi lần gọi.
- Admin được mở trực tiếp trang quản trị để cấu hình. Menu hiển thị của mọi tài khoản lấy từ UserRoles → Roles → RoleModules.
- Tạo vai trò và gán menu, sau đó gán vai trò cho người dùng. Tài khoản mới chưa có vai trò chỉ có bàn làm việc.
- Nếu Redis lỗi, đăng nhập báo không khả dụng; không tự chuyển sang tài khoản demo.
- Quản lý phiên hiện quản lý cookie đăng nhập Blazor. Các JWT API cũ chưa tích hợp thu hồi vào danh sách này.
- Endpoint đăng nhập demo API đã được chuyển sang kiểm tra mật khẩu thực; không còn tự cấp Admin.

## Kiểm tra

`Tools/LoginChecks`: SQLite trong RAM, kiểm tra mật khẩu, quyền, khóa, thu hồi, đổi mật khẩu, cây menu từ database, không tự sinh bản ghi và quyền theo vai trò.
`Tools/RedisChecks`: kiểm tra xác thực Redis và ghi/đọc/xóa một key tạm có TTL.
Đã kiểm tra HTTP thực: chuyển hướng người dùng ẩn danh, CSRF, đăng nhập admin, xem phiên Redis, đăng xuất.

## Phần chưa tích hợp

Chưa có quên mật khẩu qua email, SSO hoặc MFA. Nếu cần, phải cấu hình nhà cung cấp cá nhân tương ứng.
Không dùng dịch vụ của công ty. Các bảng nghiệp vụ vẫn giữ dữ liệu thử nghiệm hiện có.
