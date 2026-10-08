# MenuAccessCache

Các thành phần nằm trong Components/Layouts/ShareComponent/SecurePage của UI.

- IMenuAccessCache và MenuAccessCache giữ HasMenu(CurrentUserDto, Guid) và Invalidate() theo mẫu.
- Đăng ký Scoped theo circuit Blazor. Cache dựng lại khi đổi user hoặc thay đối tượng danh sách MenusActive; sửa danh sách tại chỗ phải gọi Invalidate.
- MenusActive là alias của Menus, dùng được với DTO backend hiện có.
- Người chưa xác thực, ID rỗng hoặc menu không có trong danh sách đều bị từ chối. Không tự bỏ qua quyền cho chuỗi Role = Admin.
- NavMenu lấy cây từ Modules trong database; không dùng GUID cố định tự đặt hay fallback danh mục giả.
- MenuSecure.FindMenuId(user, "/ho-khau") trả ID thực hoặc null. Khi có ID mới gọi HasMenu; không có thì không hiển thị.
- Menu lấy trực tiếp từ dữ liệu đã cấu hình; `NavMenu` dùng `LienKet` của bản ghi menu để điều hướng tới `@page` của màn hình tương ứng.

Xem [MENU_TREE.md](MENU_TREE.md) để biết cây menu, cách khởi tạo và giới hạn demo. Ẩn menu không thay thế quyền truy cập trang hoặc API. Thay đổi này không cần migration mới.
