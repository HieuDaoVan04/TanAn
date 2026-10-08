# Duyệt hồ sơ khai sinh — 07/10/2026

Theo yêu cầu mới: cán bộ nhập trực tiếp, bỏ bước kiểm tra hồ sơ riêng và bước tiếp nhận. Luồng hiện tại là nhập/lưu hồ sơ → Chủ tịch xã duyệt. Giữ form, bảng, nút và phân trang Blazor/Fluent UI đang dùng.

## Sử dụng

1. Cán bộ nhập nhanh qua mã hộ hoặc tự nhập, điền thông tin cha/mẹ, lưu hồ sơ như bước 1.
2. Tab **Hồ sơ khai sinh** chỉ hiển thị hồ sơ **Chưa duyệt**, mỗi dòng có **Xem**, **Sửa**, **Duyệt hồ sơ**. Cửa sổ xem chi tiết hồ sơ chưa duyệt cũng có nút duyệt. Bộ lọc trạng thái được áp dụng trước khi đếm và phân trang, không cần checkbox chọn hồ sơ chưa duyệt.
3. Chỉ tài khoản được gán **vai trò có mã `ctx` đã duyệt**, đồng thời có quyền menu `/bien-dong/khai-sinh`, được bật nút duyệt màu xanh. So sánh mã không phân biệt hoa/thường. Loại tài khoản `RoleEnum.ChuTichXa` hoặc tên vai trò “Chủ tịch xã” tự nó không cấp quyền duyệt. Tài khoản thiếu quyền thấy nút mờ.
4. Duyệt chuyển nhãn từ **Chưa duyệt** sang **Đã duyệt**, đồng thời tạo nhân khẩu mới trong hộ, thành viên hộ và biến động khai sinh. Lưu tài khoản và thời điểm duyệt, tăng phiên bản. Giao diện chuyển ngay sang tab **Khai sinh đã ghi nhận**. Hồ sơ gốc được giữ và khóa sửa.
5. Hồ sơ đã duyệt xuất hiện trong **Khai sinh đã ghi nhận**, chỉ xem chi tiết; không hiện nút sửa, duyệt hoặc ghi nhận nhân khẩu riêng. Đường dẫn thông báo tới hồ sơ đã duyệt cũng mở tab đã ghi nhận. Với dữ liệu cũ chỉ có trạng thái đã duyệt nhưng chưa có nhân khẩu/biến động, cần hoàn tất dữ liệu một lần qua service duyệt hiện có, giữ người/ngày duyệt gốc và chống tạo trùng; không thêm thao tác này vào giao diện cán bộ.

Admin tạo/sửa vai trò tại **Quản lý vai trò**: mã `ctx`, trạng thái **Đã duyệt**, chọn menu khai sinh rồi lưu. Nút **Chọn tất cả** chọn toàn bộ danh sách menu hiện có, không tự lưu; có thể bỏ chọn từng menu trước khi lưu. Sau đó vào **Quản lý người dùng → Sửa tài khoản → Vai trò** để gán vai trò `ctx`. Admin không tự có quyền duyệt nếu chưa được gán `ctx`. Không tự tạo hoặc đổi quyền tài khoản đang dùng khi triển khai.

Phạm vi dữ liệu vẫn theo cấp quyền và thôn được giao: Admin, cán bộ xã và cấp quyền Chủ tịch xã có phạm vi toàn xã; tài khoản phụ trách thôn giữ phạm vi thôn. Gán `ctx` cấp quyền duyệt hồ sơ trong phạm vi truy cập hiện tại, không tự đổi cấp quyền tài khoản.

Nhập/lưu nháp vẫn cho phép thiếu thông tin. Trước khi duyệt phải liên kết hộ và đủ dữ liệu tạo nhân khẩu theo kiểm tra của đăng ký khai sinh: tên, ngày sinh, nơi sinh, địa chỉ, quan hệ với chủ hộ, thông tin người yêu cầu và các trường bắt buộc trên tờ khai. Không bắt buộc số định danh của trẻ nếu chưa có. Thiếu hoặc sai dữ liệu trả thông báo và giữ nguyên trạng thái/phiên bản; chưa tạo nhân khẩu. Luồng này cập nhật `NhanKhau`, `ThanhVienHo`, `BienDongDanCu`, chưa cấp giấy khai sinh hoặc ký kết quả.

Nếu hồ sơ nháp chưa có ngày đăng ký (form hiện tại không yêu cầu cán bộ nhập), hệ thống lấy ngày duyệt làm ngày đăng ký khi tạo nhân khẩu, lưu thống nhất vào nội dung hồ sơ và biến động. Hoàn tất hồ sơ cũ lấy ngày duyệt gốc, không lấy ngày chạy sửa dữ liệu.

## Thông báo việc cần duyệt

Tài khoản có vai trò `ctx` đã duyệt và menu khai sinh thấy mục **Việc cần duyệt** ở đầu khu vực thông báo trên **Bàn làm việc**, đồng thời thấy số lượng trên chuông thông báo. Danh sách lấy trực tiếp các hồ sơ khai sinh `Pending` trong phạm vi dữ liệu được phép; không tạo bản ghi thông báo hoặc lưu bản sao nội dung hồ sơ.

Hiển thị tổng số hồ sơ và tối đa 10 hồ sơ gần nhất (mã hồ sơ, tên trẻ, ngày tạo), không tải JSON khai sinh cho danh sách thông báo. **Xem hồ sơ** chuyển đến `/bien-dong/khai-sinh?hoSoId=<id>` và mở chi tiết để duyệt. **Xem tất cả** mở danh sách hồ sơ khai sinh. Đã bỏ ô **Chỉ hiển thị hồ sơ chưa duyệt**; tab hồ sơ chỉ hiển thị chưa duyệt, tab đã ghi nhận hiển thị biến động sau duyệt và không có nút Thêm mới. Khi mở từ bảng thông báo bên cạnh, bảng tự đóng để không che chi tiết.

Thông báo tải khi vào Bàn làm việc hoặc mở chuông; nút **Làm mới** đọc lại quyền và dữ liệu. Chuông cập nhật số lượng mỗi 30 giây. Hồ sơ đã duyệt được loại khỏi danh sách/số lượng ở lần tải kế tiếp. Thu hồi vai trò/menu chặn đọc thông báo ngay khi tải lại. Tài khoản có vai trò `ctx` không thấy mục **Thông báo theo thôn**, và chuông không cộng thông báo thôn cho tài khoản này. Các tài khoản khác vẫn thấy thông báo theo thôn như trước. Hiện chỉ có nghiệp vụ duyệt khai sinh được nối vào mục này; các nghiệp vụ khác sẽ bổ sung khi có luồng duyệt tương ứng.

Phần thông báo và bộ lọc chờ duyệt không cần migration mới.

## API và bảo vệ dữ liệu

`POST /api/v1/BienDong/khai-sinh/ho-so/{id}/duyet`

```json
{ "phienBan": 1 }
```

API yêu cầu TanAnSession, vai trò đã gán có mã `ctx` đã duyệt và quyền menu khai sinh. API và service cùng đọc lại quyền gán, mã/trạng thái vai trò, menu, trạng thái tài khoản và khóa đăng nhập từ DB, không tin cờ quyền ở UI. Thu hồi vai trò hoặc chuyển vai trò sang Chưa duyệt chặn ngay cả với phiên đang đăng nhập. Không nhận trạng thái duyệt/người duyệt/ngày duyệt từ client.

Phiên bản dùng concurrency token: nếu cán bộ đã sửa hồ sơ kể từ lần đọc, duyệt bị chặn để người duyệt tải và xem bản mới. Thao tác duyệt chiếm phiên bản trước khi tạo nhân khẩu. Trạng thái, nhân khẩu, thành viên hộ, biến động và các audit nằm trong cùng transaction; lỗi bất kỳ bước nào rollback toàn bộ. `BienDongDanCu.Id` dùng ID hồ sơ để chống tạo trùng; DTO `DaGhiNhan` được tính từ biến động hiện có, không thêm cột DB. Duyệt lại hồ sơ đã duyệt và ghi nhận trả cùng kết quả, không tạo nhân khẩu/audit hoặc đổi người/ngày duyệt. API lưu chặn mọi chỉnh sửa hồ sơ đã duyệt, kể cả Admin.

## DB

Thay đổi quyền duyệt theo mã `ctx` và nút chọn tất cả ngày 08/10/2026 không cần migration mới; dùng các bảng `Roles`, `UserRoles`, `RoleModules` hiện có.

Thay đổi duyệt tạo nhân khẩu/biến động và bỏ checkbox không cần migration mới. Hồ sơ cũ chưa ghi nhận được hoàn tất qua nút Ghi nhận nhân khẩu, không tự sửa dữ liệu đang dùng khi triển khai.

Hồ sơ dùng enum chung `ModerationStatus`: **Pending = 1** (Chưa duyệt, mặc định khi tạo), **Approved = 0** (Đã duyệt). Ràng buộc DB chỉ cho hai giá trị này. DTO, điều kiện bật nút, khóa sửa và audit đều dùng cùng enum.

Migration `20261007105518_AddBirthApproval` trước đây thêm cờ bool và thông tin người/ngày duyệt. Migration bổ sung `20261007110402_UseBirthModerationStatus` chuyển sang cột `ModerationStatus`, sao chép `DaDuyet = true` thành Approved, false thành Pending trước khi bỏ cột bool. Giữ nguyên người và ngày duyệt, hỗ trợ cả DB đã hoặc chưa chạy migration trước. Không sửa lịch sử migration cũ. Các giá trị enum vai trò cũ giữ nguyên; `ChuTichXa` dùng giá trị mới 5.

Chưa áp dụng migration lên DB đang dùng. Dừng debug, chạy trong Package Manager Console:

```powershell
Update-Database -Migration 20261007110402_UseBirthModerationStatus -Project Service.TanAn.Infrastructure -StartupProject Service.TanAn.API -Context TanAnDbContext
```

Lệnh tự áp dụng các migration trước nếu môi trường chưa chạy. SQL từ nền `AddBirthApproval` sang enum: `docs/sql/20261007_birth_moderation_status.sql`. SQL lịch sử từ nền `AddBirthDrafts` tới `AddBirthApproval` vẫn giữ tại `docs/sql/20261007_birth_approval.sql`.

## Kiểm tra

- PopulationUiChecks: nút mờ khi thiếu mã vai trò `ctx` hoặc quyền menu; nút xanh cho tài khoản `CanBoXa` được gán `ctx`; callback duyệt cập nhật danh sách/chi tiết và khóa editor; các bộ lọc, phân trang và sáu màn hình vẫn hoạt động.
- AdministrationApiChecks: 401/403, tài khoản thiếu `ctx` bị chặn, `ctx` chưa duyệt hoặc thiếu menu bị chặn, phiên bản cũ, lưu thông tin duyệt, retry không trùng audit, thu hồi vai trò có hiệu lực ngay và chặn sửa sau duyệt.
- LoginChecks: thông tin người dùng hiện tại tải mã vai trò đã duyệt từ DB, chuẩn hóa `CTX` thành `ctx` và loại bỏ ngay khi vai trò bị chuyển sang Chưa duyệt.
- EntityModelChecks: Pending mặc định, quyền service, phạm vi toàn xã, concurrency, duyệt tạo một nhân khẩu/thành viên hộ/biến động, rollback toàn bộ khi audit lỗi, chặn dữ liệu thiếu/định danh trùng, hoàn tất hồ sơ đã duyệt cũ giữ nguyên metadata và không tạo trùng; SQL chuyển trạng thái true/false và rollback giữ nguyên hai trạng thái cùng metadata duyệt.
- VillageScopeChecks: các giới hạn thôn trước đây vẫn được giữ.
