# Đăng ký khai sinh

Cập nhật 07/10/2026: cán bộ nhập và lưu hồ sơ, Chủ tịch xã duyệt; không có bước tiếp nhận riêng. Hướng dẫn mới nhất: [KHAI_SINH_APPROVAL.md](KHAI_SINH_APPROVAL.md).

## Cập nhật bước 1

Form hiện chuyển sang **lưu hồ sơ nháp**, có nhập nhanh/tự nhập, mở lại sửa và chưa tạo nhân khẩu/biến động. Cần migration `20261005150646_AddBirthDrafts`. Hướng dẫn hiện hành: [KHAI_SINH_STEP_1.md](KHAI_SINH_STEP_1.md). Phần dưới mô tả luồng ghi nhận cũ và cấu trúc snapshot vẫn được giữ để xem dữ liệu lịch sử; API ghi nhận trực tiếp cũ đã bị chặn.

Nguồn phân tích là file `Mẫu-tờ-khai-đăng-ký-khai-sinh-tại-ĐSQ.docx` người dùng cung cấp. File chứa tờ khai đăng ký khai sinh và một tờ khai BHXH/BHYT riêng. Màn hình `/bien-dong/khai-sinh` dùng phần khai sinh, với cơ quan mặc định là UBND xã Tân An và cho phép chỉnh sửa.

| Nhóm | Trường dùng trên màn hình | Cách nhập |
| --- | --- | --- |
| Hộ gia đình | Mã hộ, chủ hộ, địa chỉ | Tra mã hộ chính xác; hoặc tìm theo tên chủ hộ và chọn hộ |
| Trẻ | Họ tên, ngày sinh, giới tính, dân tộc, quốc tịch, nơi sinh, quê quán | Cán bộ nhập và xác nhận; ngày sinh bằng chữ được tạo tự động |
| Bổ sung vào hộ | Thường trú, quan hệ với chủ hộ, số định danh | Địa chỉ điền từ hộ; quan hệ nhập rõ Con/Cháu/...; số định danh để trống nếu chưa có |
| Người yêu cầu | Họ tên, quan hệ với trẻ, loại/số giấy tờ, ngày/nơi cấp, nơi cư trú | Chọn thành viên hộ để điền họ tên, CCCD và địa chỉ; hoặc nhập thủ công |
| Cha/mẹ | Họ tên, ngày sinh, dân tộc, quốc tịch, nơi cư trú | Chọn thành viên hộ để điền dữ liệu đang có; hoặc nhập thủ công; bỏ chọn nhóm nếu chưa khai thông tin |
| Đăng ký | Cơ quan, ngày đăng ký, số giấy chứng sinh, ghi chú | Ngày đăng ký mặc định hôm nay; giấy chứng sinh và ghi chú tùy chọn |

Ngày/nơi cấp giấy tờ, ngày sinh và các thông tin phụ của cha/mẹ được phép bổ sung khi có dữ liệu. Quốc tịch mặc định Việt Nam, vẫn sửa được; cơ sở dữ liệu nhân khẩu hiện chưa có quốc tịch nên không coi giá trị mặc định là thông tin đã tra được. Chọn người trong hộ không tự suy ra quan hệ cha/mẹ/người yêu cầu; cán bộ phải xác nhận vai trò. Đổi hộ hoặc tra không thành công sẽ xóa lựa chọn và dữ liệu lấy từ hộ cũ.

Khi lưu cần chọn một hộ đang tồn tại trong phạm vi quản lý. Hệ thống tạo một nhân khẩu mới, một lịch sử thành viên hộ, một biến động KhaiSinh và nhật ký trong cùng transaction. Ngày phát sinh biến động là ngày sinh; ngày đăng ký được lưu riêng trong hồ sơ và dùng làm ngày bắt đầu thành viên hộ. Không bắt buộc trẻ có số định danh, nhưng nếu nhập phải đủ 12 chữ số và chưa bị dùng.

Thông tin tờ khai được lưu dưới dạng snapshot JSON trong cột nullable `BienDongDanCus.HoSoKhaiSinhJson`, trả về bằng `BienDongDto.HoSoKhaiSinh` và hiển thị khi mở chi tiết. Hồ sơ không bị thay đổi theo các lần chỉnh sửa thông tin cha/mẹ sau đó. Các biến động cũ không có snapshot vẫn xem được bằng phần chi tiết cũ. Quốc tịch, nơi sinh và thông tin giấy tờ được giữ đầy đủ trong snapshot; không ghi đè thông tin nhân khẩu của cha/mẹ/người yêu cầu.

`RequestId` của form được dùng làm ID biến động để việc lưu lại cùng một form không tạo thêm trẻ. Cùng ID nhưng nội dung khác bị từ chối. Lỗi ở bất kỳ bước ghi dữ liệu hoặc nhật ký nào đều rollback transaction.

API bổ sung:

- `GET /api/v1/BienDong/khai-sinh/ho-gia-dinh?maSoHo=...`
- `POST /api/v1/BienDong/khai-sinh`, body `KhaiSinhForm`.

Hai API yêu cầu phiên TanAnSession hợp lệ và quyền menu khai sinh (hoặc Admin). Đọc/ghi hộ vẫn theo phạm vi thôn của DbContext. Giao diện kiểm tra lại quyền trước khi lưu và dùng service trong scope riêng cho từng thao tác.

Migration `AddBirthDeclarationSnapshot` chỉ thêm cột nullable lưu snapshot. Không seed hoặc tự cập nhật schema khi khởi động. Các môi trường khác cần chạy migration trước khi dùng bản build này.
