# Menu biến động dân cư

Mã giao diện nằm trong `Components/Pages/BienDong`, mỗi màn hình là một thư mục:

```text
BienDong/
├── BienDongDanCu/   # Danh sách tổng hợp: Index và View, không thêm mới
├── KhaiSinh/
├── KhaiTu/
├── TamTru/
├── TamVang/
├── ChuyenDen/
└── ChuyenDi/
```

Sáu màn hình theo loại đều có `Index.razor` + `Index.razor.cs` + `Index.razor.css`, `Edit.razor` + `Edit.razor.cs`, `View.razor` + `View.razor.cs`. Giao diện bộ lọc, thanh công cụ và bảng nằm trực tiếp trong `Index.razor`; tra cứu, lọc, xuất Excel và điều phối dialog nằm trong `Index.razor.cs` của chính màn hình. Muốn chỉnh cột hoặc bộ lọc Khai sinh, sửa `KhaiSinh/Index.razor` và CSS tương ứng; các màn hình khác có file riêng để nâng cấp độc lập.

`Edit.razor` chứa trực tiếp các trường nhập, `Edit.razor.cs` xử lý tra cứu, kiểm tra quyền và lưu đúng loại của màn hình. `View.razor` chứa trực tiếp nội dung chi tiết. Khai sinh có thêm `ParentFields.razor` + `ParentFields.razor.cs` trong cùng thư mục để nhập thông tin cha/mẹ. `BienDongDanCu/View.razor` chứa cả giao diện tờ khai khai sinh và chi tiết biến động thông thường. Các thành phần cơ bản như `DataDialog`, `SearchPanel`, `Paginator` vẫn dùng chung.

Khi lưu thành công, màn hình đóng form và tải lại danh sách. Mở hoặc đóng dialog giữ nguyên bộ lọc hiện tại. Việc tổ chức giao diện này giữ nguyên URL, dữ liệu và quyền menu, không cần migration database.

**Biến động** là nhóm không có đường dẫn, nằm trong **Quản lý dân cư** và cùng cấp với **Hộ gia đình và nhân khẩu**. Bảy menu dưới đây là các mục con cùng cấp. **Biến động dân cư** (`/bien-dong`) mở danh sách tổng hợp của sáu loại, tìm kiếm, xem chi tiết và xuất Excel; không có thêm mới.

| Menu con | Đường dẫn |
| --- | --- |
| Biến động dân cư | `/bien-dong` |
| Khai sinh | `/bien-dong/khai-sinh` |
| Khai tử | `/bien-dong/khai-tu` |
| Tạm trú | `/bien-dong/tam-tru` |
| Tạm vắng | `/bien-dong/tam-vang` |
| Chuyển đến | `/bien-dong/chuyen-den` |
| Chuyển đi | `/bien-dong/chuyen-di` |

Mỗi trang con lọc danh sách và xuất Excel theo loại tương ứng. Form thêm mới cố định loại theo trang đang mở.
Riêng Khai sinh mở form tờ khai riêng: tra hộ, nhập trẻ mới, người yêu cầu và cha/mẹ; khi lưu tạo nhân khẩu và biến động trong một transaction. Chi tiết trường và cách điền tại [KHAI_SINH.md](KHAI_SINH.md).
Danh sách có bộ lọc từ khóa, từ ngày và đến ngày theo **Ngày phát sinh**. Chỉ màn hình tổng hợp có thêm bộ lọc loại biến động, cho phép chọn tất cả hoặc từng loại. Cả sáu trang đơn lẻ có bộ lọc **Thôn**, mặc định **Tất cả thôn**; danh mục lấy từ các thôn đang hoạt động trong database, cán bộ thôn chỉ được chọn thôn được giao. Thôn của biến động được xác định theo hộ hiện tại của nhân khẩu (`HoGiaDinh.ApThonId`). Bộ lọc thôn kết hợp với từ khóa và khoảng ngày, áp dụng trước khi đếm và phân trang, đồng thời dùng khi xuất Excel. Các trang đơn lẻ không hiển thị bộ lọc loại; danh sách và xuất Excel tự cố định loại theo menu. Tải lại xóa từ khóa, khoảng ngày và thôn; ở màn hình tổng hợp, loại trở về tất cả. Khoảng ngày tính cả hai ngày đầu/cuối; có thể chỉ nhập một đầu mốc. Khoảng ngày đảo ngược được báo lỗi.

`GET /api/v1/BienDong` nhận `loaiBienDong` (1–6), `tuNgay`, `denNgay` (ví dụ `2026-10-05`) và `apThonId` (GUID); không truyền loại thì lấy tất cả loại, không truyền ngày thì không giới hạn mốc đó, không truyền thôn thì lấy tất cả thôn trong phạm vi được phép. Body xuất Excel nhận cùng các trường `loaiBienDong`, `tuNgay`, `denNgay`, `keyword`, `apThonId`, áp dụng cùng bộ lọc trước khi đếm, phân trang hoặc xuất dữ liệu. Không có thay đổi schema database cho bộ lọc thôn.

Cấu hình menu nằm trong database. Script `sql/20261005_population_change_menus.sql` chạy theo yêu cầu, trong một transaction: tạo hoặc dùng lại nhóm Biến động trong Quản lý dân cư, đưa Biến động dân cư và sáu loại về cùng cấp, giữ ID/đường dẫn/quyền của các trang hiện có và gán nhóm/menu mới cho những vai trò đã có quyền vào Biến động dân cư. Script không tự chạy khi ứng dụng khởi động và không tạo trùng khi chạy lại.
