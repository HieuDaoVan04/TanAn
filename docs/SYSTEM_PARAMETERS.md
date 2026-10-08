# Tham số hệ thống Tân An

## Vận dụng mã tham khảo

Dự án đã có entity `SystemParameter`, bảng `SystemParameters`, API và trang danh mục quản trị. Phần bổ sung dùng chung những thành phần đó, thay vì thêm bảng `THAM_SO_HE_THONG` và các phụ thuộc AIM/S3 không tồn tại trong dự án.

Giữ các ý tưởng phù hợp: tra cứu theo mã, danh mục có kiểu dữ liệu, giá trị mặc định, duyệt/hủy duyệt, cấu hình theo nhóm và ghi nhật ký trước/sau. Chính sách mật khẩu và đăng nhập được đọc tại nơi thực thi; banner/footer dùng văn bản được Razor mã hóa.

Các vấn đề trong mã tham khảo không được mang sang:

- `GetByCode(donViId, code)` bỏ qua `donViId`; `LayDanhSachTheoDonVi` so sánh tên tham số với GUID đơn vị. Tân An hiện dùng cấu hình toàn hệ thống, chưa triển khai cấu hình ghi đè theo xã/thôn.
- API anonymous đọc tham số bất kỳ theo mã có thể công khai dữ liệu nhạy cảm. API công khai mới chỉ trả DTO gồm tám trường hiển thị cố định và một DTO chính sách mật khẩu riêng.
- `Approve`, `Reject`, `Delete` chưa cập nhật cache đầy đủ; một số hàm trả `true` dù không lưu thành công. Phần mới đọc cấu hình từ database trong mỗi thao tác nghiệp vụ, không dùng cache Redis cho tham số.
- Tạo/cập nhật lần lượt từng tham số dễ lưu dở dang. API mới kiểm tra toàn bộ danh sách trước khi sửa entity và gọi `SaveChangesAsync` một lần; EF lưu các tham số cùng audit trong một giao dịch.
- Kiểu `yesno/other` và mặc định `true` cho DTO không mô tả được trường số. Danh mục mới khai báo rõ `Text/Boolean/Integer`, giới hạn và mặc định từng mã.
- Không thêm các tham số LGSP, OCR, spell check, SSO mobile, đồng bộ văn bản, hàng đợi, thời hạn đổi mật khẩu, CAPTCHA hoặc tự xóa nhật ký khi chưa có nghiệp vụ thực thi tương ứng.
- Không dùng file đính kèm/S3 cho tham số: các cấu hình hiện được chọn chỉ cần văn bản, số và boolean.

## Sử dụng

Đăng nhập Admin, mở `/quan-ly-tham-so-ht`, chọn **Cấu hình chung hệ thống**, hoặc đi trực tiếp tới `/quan-ly-tham-so-ht/cau-hinh-chung`. Đường dẫn danh sách cũ `/quan-tri-he-thong/tham-so-he-thong` cũng mở trang mới.

Màn hình cấu hình chung hiển thị giá trị hiệu lực, chia thành bốn nhóm và chỉ gửi các trường đã sửa. Nút **Lưu cấu hình** tự duyệt các trường được lưu. Nếu giá trị hiệu lực đã thay đổi kể từ khi tải form, API yêu cầu tải lại. Đây là kiểm tra tại thời điểm đọc dữ liệu; bảng hiện chưa có concurrency token để bảo đảm chống mọi cập nhật đồng thời.

Đọc cấu hình không thêm bản ghi vào database. Nút **Bổ sung tham số mặc định** là thao tác tường minh, chỉ thêm mã còn thiếu, không ghi đè giá trị/trạng thái đã có. Không cần migration vì không đổi schema; tên, kiểu, nhóm và giới hạn của mã tích hợp được khai báo trong `SystemParameterCatalog`.

| Mã | Mặc định | Nơi sử dụng |
| --- | --- | --- |
| AppName | Tên cổng quản trị Tân An | Header |
| AppVersion | V1.1 | Footer |
| SupportEmail, Hotline | Rỗng | Footer; email được kiểm tra định dạng |
| HeaderEnabled | false | Bật thông báo đầu trang |
| HeaderContent | Rỗng | Thông báo đầu trang, văn bản thuần |
| FooterEnabled | true | Bật nội dung footer |
| FooterContent | Tên cổng CSDL Tân An | Footer, văn bản thuần |
| PasswordMinLength | 8; cho phép 8–128 | Tạo/đổi mật khẩu qua dịch vụ người dùng và quản trị |
| PasswordRequireUppercase | true | Bắt buộc chữ hoa |
| PasswordRequireLowercase | true | Bắt buộc chữ thường |
| PasswordRequireDigit | true | Bắt buộc chữ số |
| PasswordRequireSpecialChar | true | Bắt buộc ký tự đặc biệt, khoảng trắng không tính |
| MinuteExpireToken | 60; cho phép 5–1440 | Phiên mới trên Blazor và token JWT mới |
| KhoaTaiKhoan | 5; cho phép 3–20 | Số lần sai liên tiếp trước khi khóa |
| LoginLockoutMinutes | 15; cho phép 1–1440 | Thời gian khóa, không phải cửa sổ đếm số lần sai |

Mật khẩu đang có không bị từ chối chỉ vì chính sách mới; kiểm tra khi đặt mật khẩu mới. Phiên đã cấp giữ thời hạn cũ. Phiên giao diện đồng bộ thời hạn cookie và phiên trên server.

Hộp **Đổi mật khẩu** tải chính sách hiện hành, yêu cầu mật khẩu hiện tại và gửi tới API phiên thật. Backend xác định tài khoản từ phiên đăng nhập, không nhận ID tài khoản tùy ý. Mật khẩu mới lưu bằng PBKDF2; sau khi đổi, các phiên `TanAnSession` cũ bị từ chối do dấu kiểm tra hash thay đổi, giao diện yêu cầu đăng nhập lại. Nhật ký không chứa mật khẩu hoặc hash. Điều này không bổ sung cơ chế thu hồi JWT cho các API cũ.

Giá trị tích hợp chỉ có hiệu lực khi bản ghi được duyệt và vượt qua kiểm tra. Thiếu, chưa duyệt, sai kiểu hoặc trùng mã trong dữ liệu cũ đều dùng mặc định. Mã được trim và chuẩn hóa về mã danh mục, kiểm tra trùng không phân biệt hoa thường ở tầng dịch vụ. Bảng hiện chưa có unique index cho mã: kiểm tra này không thay thế ràng buộc database trong trường hợp nhiều yêu cầu tạo đồng thời. Nếu dữ liệu cũ trùng mã, thao tác lưu cấu hình chung từ chối và yêu cầu xử lý dữ liệu trước.

Không cho đổi mã hoặc xóa tham số tích hợp. Có thể hủy duyệt để quay về mặc định. Tham số tùy chỉnh vẫn là văn bản và chỉ có tác dụng khi có nghiệp vụ đọc nó; không trả qua API công khai, giá trị được ẩn khỏi audit. Không lưu API key/bí mật triển khai trong cấu hình hiển thị; giữ chúng ở cấu hình môi trường hiện có.

Header/footer tải lại ngay trong phiên giao diện vừa lưu; các phiên giao diện khác nhận giá trị khi tải lại trang. Nghiệp vụ mật khẩu/đăng nhập đọc giá trị mới trong mỗi thao tác.

## API

Các endpoint có tiền tố `/api/v1/system-configuration`:

- `GET /public`: thông tin hiển thị cố định, cho phép anonymous.
- `GET /password-policy`: chính sách mật khẩu, cho phép anonymous.
- `GET /general`: danh mục cấu hình hiệu lực; cần phiên `TanAnSession` của Admin.
- `PUT /general`: danh sách `{ code, value, expectedValue? }`; cần Admin. Giá trị truyền ở dạng chuỗi; boolean là `true/false`.
- `POST /sync`: bổ sung mặc định còn thiếu; cần Admin.

Danh mục `/administration/parameters` và API `SystemParameter` cũ dùng chung kiểm tra/lưu/audit. Đồng bộ từ enum trên API cũ chuyển sang POST vì có thay đổi dữ liệu.

`PUT /api/v1/session-account/password`: `{ currentPassword, newPassword }`, cần phiên `TanAnSession` hợp lệ; mọi loại tài khoản có thể đổi mật khẩu của chính mình.

## Kiểm tra

`Tools/AdministrationApiChecks`: HTTP thật trên localhost, SQLite trong RAM và phiên test. Kiểm tra quyền, giá trị có hiệu lực, kiểu/range, batch không lưu dở dang, form cũ, mã trùng, bảo vệ mã hệ thống, sync không ghi đè, không lộ tham số tùy chỉnh, audit và kiểm tra mật khẩu.

`Tools/LoginChecks`: thời hạn phiên/cookie, ngưỡng sai và thời gian khóa theo cấu hình; dữ liệu cũ sai kiểu dùng mặc định. `Tools/EntityModelChecks`: hồi quy danh mục, bảo toàn trạng thái Draft và lưu người dùng. Các kiểm tra không thay đổi database đang chạy.
