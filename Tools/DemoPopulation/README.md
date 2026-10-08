# Dữ liệu dân cư demo Tân An

Công cụ chỉ thêm dữ liệu, không xóa dữ liệu có sẵn; dùng transaction và mã hộ ổn định để chạy lại không nhân đôi. Chỉ cho phép kết nối Neon cá nhân được cấu hình trong Blazor/User Secrets; không ghi mật khẩu vào mã nguồn.

- 11 bản ghi đơn vị `Groups` và 11 địa bàn `ApThons` (tận dụng bản ghi cùng tên/mã nếu đã có).
- Mã hộ `DEMO-TA-xx-yyyy`; CCCD giả `DEMO` + 8 chữ số, không có giá trị giấy tờ.
- Tên người Việt Nam được ghép ngẫu nhiên theo quy tắc xác định; mọi hộ/nhân khẩu/lịch sử thành viên mang ghi chú `DEMO-TANAN-20260926`.
- Mỗi hộ 3–5 người, có một chủ hộ, quan hệ vợ/chồng/con và lịch sử cư trú.
- 5 thôn đầu theo số hộ do người dùng đưa; 6 thôn sau là số giả: 612, 684, 705, 658, 743, 626.
- Đây là dữ liệu tổng hợp để thử phần mềm, không phải số liệu dân cư chính thức.

Chạy thử SQLite RAM: `dotnet run --project Tools/DemoPopulation -- --self-test`.
Chạy vào DB cấu hình: `dotnet run --project Tools/DemoPopulation -- --apply "D:\Đồ Án"`.

Không chạy tự động khi ứng dụng khởi động. Không cần migration.

## Liên kết đơn vị với dân cư

`dotnet run --project Tools/DemoPopulation -- --link-units "D:\Đồ Án"`

Chỉ áp dụng migration `LinkLocalityToUnit` trên Neon cá nhân đã cấu hình; từ chối nếu có migration khác đang chờ. Không sinh thêm dân cư. Kiểm tra 11 liên kết và số lượng dữ liệu demo, đồng thời bảo đảm tổng số bản ghi hộ/nhân khẩu không thay đổi.

Quan hệ: `Groups.Id <- ApThons.GroupId <- HoGiaDinhs.ApThonId <- NhanKhaus.MaHoGiaDinh` (mỗi mũi tên biểu thị bước nối bảng; ApThonId tham chiếu ApThons.Id và MaHoGiaDinh tham chiếu HoGiaDinhs.Id). GroupId có khóa ngoại, chỉ mục duy nhất và chặn xóa đơn vị còn liên kết. Migration ghép theo mã đơn vị trùng duy nhất; đơn vị không có mã thôn tương ứng để trống liên kết.

Trong Quản lý đơn vị/hệ thống, nút “Hộ và nhân khẩu” mở danh sách theo đơn vị, tìm kiếm và phân trang 20 bản ghi trực tiếp tại DB. Tự kiểm tra `--self-test` bao gồm lọc đơn vị, phân trang, truy vấn nhân khẩu và đổi tên đơn vị không làm mất liên kết.
