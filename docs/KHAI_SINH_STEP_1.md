# Bước 1 — Nhập hồ sơ khai sinh và lưu nháp

Đã triển khai theo phê duyệt của người dùng. Bước này chỉ nhập và lưu hồ sơ, chưa tiếp nhận/thẩm tra/phê duyệt/ký/cấp kết quả.

## Cách sử dụng

1. Mở `/bien-dong/khai-sinh`, tab **Hồ sơ nháp**, chọn **Thêm mới**.
2. Chọn **Nhập nhanh qua mã hộ** hoặc **Tự nhập thông tin**.
3. Nhập nhanh: tra mã hộ/tên chủ hộ, chọn hộ, chọn thành viên làm người yêu cầu hoặc cha/mẹ rồi kiểm tra các thông tin được điền. Có thể nhập phần còn thiếu trực tiếp. Đổi hộ đã liên kết sẽ xóa dữ liệu lấy từ hộ cũ; chuyển sang tự nhập bỏ liên kết/ID người trong hộ và giữ văn bản đang nhập.
4. Tự nhập: không cần hộ có sẵn; có thể chọn thôn hoặc để chưa xác định. Hồ sơ chưa có thôn được cán bộ tạo đọc/sửa, và cán bộ xã/Admin theo phạm vi hiện tại; không mở cho cán bộ thôn khác.
5. Nhập thông tin trẻ, nơi sinh (cơ sở y tế/ngoài cơ sở/nước ngoài), loại cư trú, người yêu cầu, giấy tờ và tình trạng thông tin cha/mẹ.
6. Bấm **Lưu nháp**. Chấp nhận thiếu thông tin; các giá trị đã nhập phải đúng định dạng, độ dài và ngày hợp lệ. Dữ liệu chưa được xác minh phải được cán bộ kiểm tra ở bước sau.
7. Danh sách hiển thị hồ sơ nháp với **Xem / Sửa**. Mở **Sửa** để tải lại dữ liệu từ DB và lưu thay đổi. Nếu có người sửa trước, ứng dụng chặn ghi đè và yêu cầu mở lại hồ sơ.

Hồ sơ nháp chưa tạo `NhanKhau`, `ThanhVienHo`, `BienDongDanCu` hay `YeuCauNguoiDan`. Nhãn phê duyệt hiện là **Chưa duyệt**. Tab **Khai sinh đã ghi nhận** vẫn đọc dữ liệu/snapshot cũ và xuất Excel. `/bien-dong` giữ danh sách biến động đã ghi nhận.

Ngày lọc trong tab nháp là **ngày tạo hồ sơ**; trong tab đã ghi nhận là **ngày phát sinh biến động**. Danh sách nháp phân trang ở server, 10 hồ sơ mỗi trang. Sau lưu quay về tab nháp và xóa bộ lọc để nhìn thấy hồ sơ vừa lưu.

## Giao diện và dữ liệu

Giao diện nằm trực tiếp trong `Components/Pages/BienDong/KhaiSinh`: `Index`, `Edit`, `View`, `ParentFields`, cùng code-behind/CSS. Bảng mới `HoSoKhaiSinhs` lưu metadata tìm kiếm, liên kết hộ/thôn, người tạo/sửa, nội dung tờ khai JSON và phiên bản chống ghi đè. Không tạo bản sao JSON tổng chứa toàn bộ dân cư.

Tự điền chỉ đọc dữ liệu hộ/người được phép truy cập, không ghi đè thông tin gốc của hộ/cha/mẹ. Hộ và thôn liên kết được kiểm tra lại khi lưu; mã hộ/chủ hộ lấy từ DB, không tin giá trị do client gửi. Giới hạn dữ liệu đã nhập được kiểm tra cả service và UI; API nháp dùng kiểm tra riêng để không ép các trường bắt buộc của đăng ký hoàn tất.

## API

- `GET /api/v1/BienDong/khai-sinh/ho-gia-dinh?maSoHo=...`: tra hộ như trước.
- `GET /api/v1/BienDong/khai-sinh/ho-so?keyword=...&pageIndex=1&pageSize=10&apThonId=...&tuNgay=...&denNgay=...`: danh sách metadata nháp, tối đa 100 mỗi trang.
- `GET /api/v1/BienDong/khai-sinh/ho-so/{id}`: chi tiết và tờ khai để mở lại.
- `POST /api/v1/BienDong/khai-sinh/ho-so`: tạo hoặc sửa nháp; body gồm `hoSo` (`KhaiSinhForm`), `apThonId` tùy chọn, `phienBan` (0 khi tạo, phiên bản đã đọc khi sửa).

`hoSo.requestId` giữ nguyên khi thử lại. Gửi lại cùng dữ liệu không sinh bản ghi khác. Tác vụ lưu và audit nằm trong cùng transaction; lỗi ghi audit rollback hồ sơ. API yêu cầu TanAnSession và quyền menu khai sinh/Admin. Phạm vi đọc/ghi thôn được kiểm tra ở DbContext; hồ sơ không có thôn chỉ có quyền đọc theo người tạo hoặc quyền toàn xã.

`POST /api/v1/BienDong/khai-sinh` cũ trả 409 và hướng dẫn dùng API nháp. Đường tạo biến động chung không nhận loại KhaiSinh. Hàm tạo dân cư cũ vẫn được giữ để chuẩn bị bước hoàn tất sau này, nhưng form/API khai sinh không gọi nó ở bước 1.

## Cập nhật DB

Migration **20261005150646_AddBirthDrafts** chỉ thêm bảng `HoSoKhaiSinhs`, khóa ngoại và chỉ mục. Chưa áp dụng migration lên DB đang dùng trong lượt triển khai này. Không sửa/xóa các hộ, nhân khẩu hoặc biến động cũ.

Trong Package Manager Console, dừng phiên debug trước khi build rồi chạy:

```powershell
Update-Database -Migration 20261005150646_AddBirthDrafts -Project Service.TanAn.Infrastructure -StartupProject Service.TanAn.API -Context TanAnDbContext
```

Hoặc từ PowerShell tại thư mục dự án:

```powershell
dotnet ef database update 20261005150646_AddBirthDrafts --project src/Service.TanAn/Service.TanAn.Infrastructure --startup-project src/Service.TanAn/Service.TanAn.API --context TanAnDbContext
```

SQL riêng cho nâng cấp từ migration snapshot khai sinh: `docs/sql/20261005_birth_drafts.sql`. Script idempotent dựa vào lịch sử migration, dành cho DB đã ở đúng nền migration trước đó. Kiểm tra đúng DefaultConnection; API và Blazor phải dùng cùng DB.

## Kiểm tra

- `EntityModelChecks`: lưu nháp chưa đủ dữ liệu và chưa có hộ; mở lại/sửa; từ chối phiên cũ; kiểm tra ngày/định dạng/độ dài; khóa hộ/thôn; không lộ hồ sơ cho thôn/cán bộ khác; thu hồi địa bàn; audit lỗi rollback; không sinh dân cư hoặc yêu cầu tiếp nhận.
- `PopulationUiChecks`: form hoạt động, điền và xóa dữ liệu nguồn khi đổi hộ; nhập thủ công/lưu nháp, mở lại dữ liệu, hiển thị danh sách; các màn hình biến động khác vẫn hoạt động.
- `AdministrationApiChecks`: tạo/lấy/gửi lại nháp qua HTTP; nháp thiếu dữ liệu được lưu; định dạng sai bị chặn; quyền menu/đăng nhập; phân trang; đường ghi nhận cũ không tạo dân cư.
- Sinh SQL PostgreSQL và kiểm tra migration khớp model không kết nối DB thật. Các test dùng SQLite trong RAM/local HTTP.

Ngày 07/10/2026, người dùng bỏ bước kiểm tra riêng và tiếp nhận vì cán bộ nhập trực tiếp. Đã bổ sung Chủ tịch xã duyệt hồ sơ, nút mờ cho các vai trò khác. Hướng dẫn/migration bổ sung: [KHAI_SINH_APPROVAL.md](KHAI_SINH_APPROVAL.md).
