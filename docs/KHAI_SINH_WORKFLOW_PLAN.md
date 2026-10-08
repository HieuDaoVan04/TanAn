# Đối chiếu và kế hoạch hoàn thiện quy trình khai sinh

## Phạm vi được người dùng chốt ngày 07/10/2026

Cán bộ nhập hồ sơ trực tiếp; bỏ bước kiểm tra hồ sơ riêng (bước 2) và bước tiếp nhận (bước 3) trong kế hoạch cũ. Đã bổ sung nút duyệt hồ sơ màu xanh cho tài khoản được gán vai trò mã `ctx` đã duyệt và quyền menu khai sinh; các tài khoản thiếu quyền hiện mờ và bị chặn ở server. Cập nhật 08/10/2026: duyệt tự tạo nhân khẩu trong hộ, thành viên hộ và biến động khai sinh trong cùng giao dịch; chuyển sang tab Khai sinh đã ghi nhận. Chưa ký/cấp hoặc in kết quả. Hướng dẫn hiện hành: [KHAI_SINH_APPROVAL.md](KHAI_SINH_APPROVAL.md). Phân tích bên dưới là kế hoạch tham khảo trước khi người dùng điều chỉnh phạm vi.

Ngày rà soát: 05/10/2026. Phạm vi: mã nguồn hiện tại và nội dung quy trình người dùng cung cấp. Đây là kế hoạch triển khai; chưa thay đổi nghiệp vụ, chạy migration hay ghi dữ liệu dân cư trong lượt rà soát này.

## 1. Kết luận và căn cứ

Hệ thống đã có form khai sinh và ghi nhận vào dữ liệu dân cư. Chưa có chu trình xử lý hồ sơ hộ tịch từ tiếp nhận đến cấp và trả kết quả. Điểm cần sửa trước tiên: nút **Lưu khai sinh** hiện tạo ngay nhân khẩu, thành viên hộ và biến động, dù chưa có bước kiểm tra hồ sơ hay phê duyệt.

Nội dung cung cấp cần tách thành đăng ký lần đầu và đăng ký lại. Luồng xác minh 05–05–03 ngày trích dẫn thuộc quy định đăng ký lại trước đây. Điều 26 đã được sửa đổi năm 2026: khi đăng ký lại tại nơi khác nơi đăng ký trước, thực hiện tra cứu dữ liệu hộ tịch điện tử để xác định điều kiện; khoản 3 cũ đã bị bãi bỏ. Không lập trình nguyên chuỗi công văn và thời hạn cũ cho mọi hồ sơ. [Văn bản hợp nhất 401/VBHN-BTP, Điều 26](https://datafiles.chinhphu.vn/cpp/files/vbpq/2026/01/401-vbhn-btp.pdf), [giải thích sửa đổi Nghị định 18/2026 của Chính phủ](https://baochinhphu.vn/chi-dao-dieu-hanh-cua-chinh-phu-thu-tuong-chinh-phu-ngay-16-1-2026-102260117131206348.htm).

Thẩm quyền cấp huyện trong tài liệu cũ cũng cần cập nhật theo mô hình chính quyền địa phương hai cấp; Nghị định 120/2025 chuyển các thẩm quyền hộ tịch liên quan cho UBND cấp xã. [Thông tin chính thức về Điều 4 Nghị định 120/2025](https://baochinhphu.vn/uy-ban-nhan-dan-xa-phuong-dac-khu-thuc-hien-tham-quyen-dang-ky-ho-tich-102250612094928351.htm).

Các quy tắc giấy tờ phải phân biệt giấy tờ nộp, xuất trình và thông tin đã khai thác hợp lệ từ cơ sở dữ liệu. Không bắt buộc tải lên bản sao của mọi giấy tờ. Thành phần hồ sơ có các phương án thay giấy chứng sinh và trường hợp đặc biệt. [Danh mục thủ tục cập nhật theo Quyết định 163/QĐ-BTP năm 2026, phần đăng ký khai sinh](https://hue.gov.vn/Portals/0/Uploads/00.00.H57/Nam2026/Thang3/PL_163_2026_QD_BTP.pdf).

Chuẩn tên in hoa có dấu và ngày sinh bằng số/bằng chữ được đối chiếu với Điều 31; mẫu in và cách ghi địa danh phải dùng phiên bản áp dụng tại thời điểm đăng ký. [Thông tư 04/2020/TT-BTP, Điều 30–31; văn bản đã được sửa đổi một phần](https://vbpl.vn/TW/Pages/vbpq-toanvan.aspx?ItemID=142805).

Không mặc định quy trình địa phương Ninh Bình trong tài liệu là quy trình áp dụng cho xã Tân An của dự án. Cần xác định đúng tỉnh/xã, mã thủ tục, mẫu đang dùng, thời hạn và lệ phí trước khi cấu hình vận hành.

## 2. Mã nguồn và luồng thực tế

Các điểm đã đọc:

- `src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/BienDong/KhaiSinh/Index.razor(.cs)`: danh sách, bộ lọc, mở form và chi tiết.
- `KhaiSinh/Edit.razor(.cs)` và `ParentFields.razor(.cs)`: nhập tờ khai, tra hộ, điền người yêu cầu và cha/mẹ.
- `KhaiSinh/View.razor(.cs)`: đọc snapshot tờ khai đã lưu.
- `src/Service.Shared/Service.Shared.Contracts/DTOs/KhaiSinhForm.cs`: trường nhập, kiểm tra bắt buộc và ngày sinh bằng chữ.
- `src/Service.TanAn/Service.TanAn.Application/Services/PopulationService.KhaiSinh.cs`: kiểm tra nghiệp vụ và transaction tạo dữ liệu.
- `src/Service.TanAn/Service.TanAn.API/Controllers/v1/BienDongController.cs`: API tra hộ và lưu, xác thực và quyền menu khai sinh.
- `YeuCauNguoiDan`, `LichSuXuLyHoSo`, `TepDinhKem`, `CitizenRequestService`: nền tảng yêu cầu công dân có thể tái sử dụng, hiện chưa nối với khai sinh.

Luồng hiện tại:

```text
Danh sách khai sinh → Thêm mới → Tra/chọn hộ có sẵn
→ Nhập trẻ, người yêu cầu, cha/mẹ và thông tin đăng ký
→ Lưu khai sinh → Kiểm tra dữ liệu và phạm vi quản lý
→ Tạo nhân khẩu + thành viên hộ + biến động + snapshot JSON + nhật ký
→ Quay lại danh sách, xem chi tiết hoặc xuất Excel
```

`NgayPhatSinh` của biến động là ngày sinh; `NgayDangKy` lưu riêng và hiện dùng làm ngày bắt đầu thành viên hộ. Hai ngày này không thể thay thế ngày tiếp nhận, hạn giải quyết hoặc ngày trả kết quả.

## 3. Bảng đã làm và chưa làm

| Nội dung | Hiện trạng | Phần cần bổ sung |
| --- | --- | --- |
| Folder riêng, Razor và code-behind | Đã có `Index`, `Edit`, `View`, `ParentFields` trong folder KhaiSinh | Giữ cấu trúc; bổ sung các phần xử lý hồ sơ ngay trong folder |
| Danh sách khai sinh | Đã có từ khóa, từ/đến ngày, thôn, xem chi tiết, Excel; không có lọc loại ở trang đơn lẻ | Thêm danh sách hồ sơ chờ xử lý; phân trang tại máy chủ, hiện tải `int.MaxValue` rồi phân trang tại UI |
| Tra mã hộ và tự điền | Đã có hộ, địa chỉ, thành viên để chọn người yêu cầu/cha/mẹ; đổi hộ xóa dữ liệu cũ | Cho phép tiếp nhận khi chưa có hộ trong DB; mở rộng tra người trong phạm vi được cấp quyền, không bắt cha/mẹ luôn cùng hộ |
| Thông tin trẻ | Đã có tên, ngày sinh, giới tính, dân tộc, quốc tịch, quê quán, nơi sinh | Chuẩn tên khi lập tờ khai/in; xác nhận thông tin mặc định; kiểm tra theo từng trường hợp |
| Ngày sinh bằng chữ | Đã tạo tự động; chặn ngày tương lai và đăng ký trước ngày sinh | Xử lý nguồn xác định ngày sinh cho trường hợp đặc biệt; giới hạn năm hiện có không phù hợp cho mọi hồ sơ đăng ký lại |
| Cơ quan đăng ký | Có ô nhập, mặc định UBND xã Tân An | Cấu hình đơn vị có thẩm quyền, phiên bản thủ tục và căn cứ cư trú; chuỗi tên cơ quan không phải xác minh thẩm quyền |
| Cư trú | Có địa chỉ thường trú trẻ và nơi cư trú người yêu cầu/cha/mẹ | Phân biệt thường trú, tạm trú, nơi đang sinh sống, nước ngoài; không suy ra thường trú hợp pháp chỉ từ địa chỉ hộ |
| Giấy tờ người yêu cầu | Có loại, số, ngày cấp, nơi cấp | Ngày/nơi cấp hiện tùy chọn; thêm tên cụ thể khi chọn giấy tờ khác, đối chiếu giấy tờ/dữ liệu, danh mục loại giấy tờ theo quy định áp dụng |
| Quan hệ người yêu cầu | Có trường bắt buộc quan hệ với trẻ | Chọn vai trò cha/mẹ/người khác; yêu cầu chi tiết và căn cứ phù hợp theo vai trò, không tự suy ra quan hệ từ thành viên hộ |
| Nơi sinh | Có ô văn bản và hướng dẫn nhập | Chọn sinh tại cơ sở y tế/ngoài cơ sở/nước ngoài; nhập thành phần tương ứng, địa danh hiện hành và lịch sử |
| Thông tin cha/mẹ | Có thể nhập/chọn hoặc bỏ nhóm | Phân biệt chưa nhập với chưa xác định cha/mẹ; bỏ chọn nhóm chưa đủ để xử lý hồ sơ trẻ bị bỏ rơi |
| Giấy chứng sinh | Chỉ có số giấy tùy chọn | Danh mục giấy tờ theo trường hợp; chứng cứ thay thế, đối chiếu, tệp nếu cần; không để thiếu chứng cứ vẫn hoàn tất |
| Tiếp nhận và giấy hẹn | Chưa có trong khai sinh | Mã hồ sơ, ngày giờ nhận, kênh nhận, người nhận, hạn trả, phiếu tiếp nhận |
| Hướng dẫn bổ sung/từ chối | Chưa có trong khai sinh | Nội dung cụ thể, lý do, người lập, ký/xác nhận, văn bản, các lần bổ sung |
| Thẩm tra và xác minh | Chưa có trong khai sinh | Kết luận thẩm tra, chứng cứ, kết quả tra cứu; nhánh xác minh theo thủ tục đang áp dụng |
| Phê duyệt và ký | Chỉ kiểm tra quyền menu hoặc Admin khi lưu | Quyền tiếp nhận/thẩm tra/trình/phê duyệt/ký; ghi nhận người có thẩm quyền và kết quả ký |
| Sổ và giấy khai sinh | Chưa có | Tham chiếu sổ/kết quả hộ tịch chính thức, số đăng ký, người ký, ngày cấp, bản kết quả, xác nhận nội dung theo hình thức thực hiện |
| Số định danh | Cho nhập hoặc để trống; kiểm tra 12 số và trùng | Tra cứu/nhận số từ hệ thống có thẩm quyền khi có kết nối; không tự sinh số định danh chính thức |
| Trả kết quả | Chưa có | Ngày giờ, người nhận, cách nhận, kết quả đã giao, dấu vết lưu trữ |
| Lưu dữ liệu và chống lưu lặp | Có transaction, nhật ký, snapshot, `RequestId` chống gửi lặp cùng form | Chỉ ghi nhận biến động khi đăng ký hoàn tất; khóa đồng thời, chống hoàn tất hai lần; kiểm tra nghi trùng giữa các hồ sơ khác ID |
| Nền tảng hồ sơ dùng chung | Có các entity yêu cầu, lịch sử, metadata tệp và cập nhật trạng thái chung | Chưa tích hợp khai sinh; chưa có chu trình kiểm soát chuyển bước và văn bản nghiệp vụ riêng |

Các kiểm tra hiện có nằm trong `Tools/PopulationUiChecks`, `EntityModelChecks`, `AdministrationApiChecks`, `VillageScopeChecks`: đã có tình huống form/tra hộ, lưu và gửi lại, transaction, quyền API, phạm vi thôn. Lượt rà soát này đọc mã kiểm tra; không chạy lại test và không coi chúng là kiểm chứng quy trình hộ tịch chưa được xây dựng.

## 4. Những thay đổi nghiệp vụ quan trọng

1. **Lưu hồ sơ chưa tạo nhân khẩu.** Hồ sơ nháp, đang bổ sung hoặc bị từ chối không được tạo biến động dân cư. Khi đăng ký đã hoàn tất và có kết quả xác nhận, mới thực hiện ghi nhận dân cư theo lựa chọn đã thống nhất: tạo nhân khẩu mới trong hộ và biến động khai sinh, một lần duy nhất.
2. **Mã hộ hỗ trợ tự điền, không thay thế điều kiện tiếp nhận pháp lý.** Hộ chưa có trong DB không có nghĩa người dân không đủ điều kiện khai sinh. Cho nhận hồ sơ và xử lý liên kết hộ sau; kiểm tra thẩm quyền và phạm vi truy cập riêng. Cha/mẹ/người đi đăng ký có thể không cùng hộ trẻ.
3. **Khai sinh và đăng ký thường trú là hai nghiệp vụ riêng.** Tạo nhân khẩu/thành viên trong DB địa phương không xác nhận đã hoàn tất thủ tục đăng ký cư trú trên hệ thống có thẩm quyền. Lưu rõ nguồn và tình trạng đối chiếu của thông tin cư trú.
4. **Đăng ký lại không tạo người mới mặc định.** Phải tra và liên kết nhân khẩu đã có, kiểm tra điều kiện thủ tục; không dùng nguyên nút tạo trẻ hiện tại cho người trưởng thành đăng ký lại.
5. **Quyền Admin kỹ thuật không thay thế người ký có thẩm quyền.** Tách quản trị hệ thống và quyền nghiệp vụ; cấp quyền theo cán bộ/đơn vị/phân công, kiểm tra cả API và UI.
6. **Mặc định Kinh/Việt Nam không phải thông tin đã xác minh.** Trường hợp thiếu cha/mẹ hoặc có yếu tố nước ngoài phải dùng quy tắc riêng, không áp giá trị mặc định chung.
7. **Ngày sinh, ngày tiếp nhận, ngày đăng ký, ngày cấp và ngày trả là dữ liệu riêng.** Bộ lọc hồ sơ phải ghi rõ đang lọc loại ngày nào; giữ ngày phát sinh của biến động theo nghiệp vụ hiện tại.

## 5. Luồng đích đề xuất

```text
Tạo/lưu nháp → Kiểm tra hồ sơ để tiếp nhận
                    ├─ Thiếu → Lập hướng dẫn bổ sung → Nhận bổ sung → Kiểm tra lại
                    ├─ Không đủ điều kiện → Lập văn bản từ chối, lưu lý do
                    └─ Hợp lệ → Tiếp nhận, lập phiếu/hạn trả
                                   → Thẩm tra/tra cứu khi cần
                                   → Trình phê duyệt và ký theo thẩm quyền
                                   → Hoàn tất đăng ký, ghi nhận kết quả hộ tịch
                                   → Đồng bộ nhân khẩu/biến động một lần
                                   → Trả kết quả, lưu hồ sơ
```

Đây là tổ chức chức năng phần mềm. Thứ tự ký, ghi sổ, cấp kết quả và cách nhận phải bám thủ tục chính thức của từng hình thức thực hiện; không bắt cán bộ bấm qua những bước không cần thiết trong trường hợp giải quyết ngay. Cho phép một cán bộ thực hiện nhiều tác vụ nếu có đủ quyền, nhưng lưu lịch sử từng tác vụ.

Giữ trường phê duyệt với hai giá trị hiển thị **Chưa duyệt / Đã duyệt** theo yêu cầu trước đó. Thêm `BuocXuLy` riêng để biết đang nháp, tiếp nhận, bổ sung, thẩm tra, trình ký, cấp hay trả kết quả; lý do từ chối và kết quả xử lý lưu riêng. Không dùng phê duyệt hai giá trị để đại diện mọi bước của hồ sơ. `TrangThaiHoSoEnum` dùng chung hiện có cần ánh xạ tương thích, không thay đổi tùy tiện các module khác.

## 6. Kiến trúc dữ liệu và giao diện

### Dữ liệu

Đề xuất bổ sung `HoSoKhaiSinh` chuyên biệt, liên kết một-một với `YeuCauNguoiDan` khi nộp/tiếp nhận; liên kết có thể chưa có ở bản nháp. Tái sử dụng nền tảng yêu cầu thay vì tạo thêm một hệ thống tiếp nhận độc lập.

| Nhóm | Dữ liệu cần lưu |
| --- | --- |
| Nhận diện và quản lý | ID hồ sơ, mã hồ sơ, loại thủ tục, phiên bản quy tắc/mẫu, đơn vị, thôn nếu đã xác định, người phụ trách |
| Nội dung khai sinh | Tờ khai có phiên bản/snapshot; các trường cần tìm kiếm như tên, ngày sinh lưu thành cột phù hợp |
| Liên kết dân cư | Hộ, trẻ/người có sẵn, cha/mẹ, người yêu cầu; nullable khi chưa liên kết; nguồn dữ liệu và thời điểm lấy |
| Xử lý | Bước xử lý, phê duyệt, kết quả, thời điểm và người nhận/thẩm tra/duyệt/ký/trả; hạn trả theo thủ tục |
| Chứng cứ | Loại giấy tờ, nộp hay xuất trình hay tra cứu, hình thức bản, số/ngày/cơ quan cấp, đối chiếu bởi ai/khi nào; tệp có thể không có |
| Văn bản | Phiếu tiếp nhận, yêu cầu bổ sung, từ chối, kết quả kiểm tra và giấy tờ kết quả; phiên bản, người ký, thời điểm ký |
| Kết quả hộ tịch | Tham chiếu đăng ký chính thức/sổ/năm/số, ngày đăng ký/cấp, số định danh nếu được cấp, bản kết quả và nguồn xác nhận |
| Đồng bộ | `NhanKhauId`, `BienDongId`, khóa duy nhất hoàn tất theo hồ sơ, trạng thái đối soát và phiên bản để kiểm soát cập nhật đồng thời |

`LichSuXuLyHoSo` cần bổ sung hoặc đi kèm lịch sử tác vụ khai sinh để ghi từng hành động, bước trước/sau và văn bản liên quan. Hàm cập nhật trạng thái chung hiện tại chỉ đổi trạng thái và ghi audit; không đủ để bảo đảm luồng này.

Giữ `HoSoKhaiSinhJson` của biến động làm snapshot lúc ghi nhận hoàn tất, không dùng nó làm nơi chứa mọi bản nháp và mọi lần bổ sung. Có thể dùng JSON cho nội dung linh hoạt, nhưng trường lọc, khóa liên kết và lịch sử tác vụ phải truy vấn được trong DB.

Migration theo hướng thêm bảng/cột nullable và chỉ mục; giữ dữ liệu cũ đọc được. Biến động đã có không tự chuyển thành hồ sơ được phê duyệt/ký: hiển thị là dữ liệu đã ghi nhận trước khi có quy trình, có thể đối soát riêng.

### Giao diện

Giữ folder `Components/Pages/BienDong/KhaiSinh`:

```text
Index.razor / Index.razor.cs / Index.razor.css
Edit.razor / Edit.razor.cs / Edit.razor.css
View.razor / View.razor.cs
ParentFields.razor / ParentFields.razor.cs
TiepNhan.razor / TiepNhan.razor.cs          (khi tách phần tiếp nhận)
ThamTra.razor / ThamTra.razor.cs            (khi tách phần thẩm tra)
KetQua.razor / KetQua.razor.cs              (ký/cấp/trả kết quả)
```

Giao diện từng phần nằm trực tiếp trong folder; chỉ chia thêm file khi phần đó đủ lớn để cần bảo trì riêng. Không thêm menu mới cho mỗi bước.

- `Index`: đề xuất hai tab **Hồ sơ đăng ký** và **Khai sinh đã ghi nhận**; giữ từ khóa, thôn và ngày, bổ sung phê duyệt/bước xử lý ở tab hồ sơ.
- `Edit`: thêm chọn trường hợp đăng ký; các mục người yêu cầu, trẻ, cha/mẹ, chứng cứ, nơi sinh/cư trú; nút lưu nháp và chuyển tiếp nhận.
- `View`: tờ khai, giấy tờ đối chiếu, lịch sử xử lý, hạn trả và kết quả; chỉ hiện tác vụ phù hợp quyền và bước hiện tại.
- Trang tổng `/bien-dong` tiếp tục hiển thị biến động đã ghi nhận, không chứa hồ sơ nháp và không cho thêm mới.
- Ngày/nơi cấp và các thông tin bắt buộc được kiểm tra khi chuyển bước theo nguồn chứng cứ, không ép nhập thêm giấy tờ khi đã có thông tin khai thác hợp lệ.

### API và tự điền

Đề xuất API hồ sơ chuyên biệt với các thao tác: tạo/sửa nháp, đọc chi tiết, danh sách phân trang, tiếp nhận, nhận bổ sung, thẩm tra, trình ký, phê duyệt/ghi nhận ký, hoàn tất, trả kết quả, từ chối. Mỗi thao tác kiểm tra quyền, phiên bản dữ liệu, điều kiện và bước trước đó ở server; không cho client tùy ý gửi trạng thái cuối.

API lưu trực tiếp hiện có và lời gọi service từ Blazor phải được chuyển sang luồng mới để không còn đường bỏ qua thẩm tra/phê duyệt. Transaction tạo dân cư hiện tại chỉ được gọi từ tác vụ hoàn tất đã kiểm soát.

Tự điền tiếp tục lấy dữ liệu từ DB qua API/service được phân quyền, trả JSON cho hộ/người đang chọn. Không xuất toàn bộ dân cư vào một file JSON tổng. Dữ liệu tự điền là gợi ý cần đối chiếu, không phải chứng cứ từ cơ sở dữ liệu quốc gia. Chưa cần xây bộ máy template JSON tổng quát để thực hiện kế hoạch này.

Kết nối hộ tịch điện tử, dân cư quốc gia hoặc ký số chỉ triển khai khi có giao diện tích hợp và quyền truy cập thực tế. Nếu chưa có, cho nhập tham chiếu/kết quả đã được cán bộ xác nhận; bản in thử trong dự án phải thể hiện đúng tính chất mẫu thử. Không tự xem số sổ, số định danh hay bản in do ứng dụng tạo là kết quả chính thức.

## 7. Kế hoạch triển khai theo thứ tự

| Đợt | Công việc | Kết quả nghiệm thu |
| --- | --- | --- |
| 1. Chốt nghiệp vụ | Ưu tiên khai sinh lần đầu trong nước; xác định địa phương/mã thủ tục/mẫu/quyền; lập ma trận chứng cứ và chuyển bước; tách nhánh đăng ký lại, nước ngoài và đặc biệt | Tài liệu trường bắt buộc có điều kiện, quyền và thời điểm tạo dân cư được thống nhất; không hardcode chuỗi 05–05–03 ngày |
| 2. Hồ sơ và DB | Thêm thực thể/DTO/migration, snapshot có phiên bản, liên kết yêu cầu; lịch sử và khóa đồng thời; giữ dữ liệu cũ | Lưu và mở nháp; chưa tạo nhân khẩu/biến động; phân quyền theo đơn vị/phạm vi; dữ liệu cũ vẫn xem được |
| 3. Form và tiếp nhận | Hoàn thiện cư trú/nơi sinh/giấy tờ/vai trò; tự điền; đối chiếu giấy tờ; phiếu nhận, hướng dẫn bổ sung và từ chối | Cán bộ nhận được hồ sơ đầy đủ hoặc lập văn bản đúng nội dung; không yêu cầu bản sao/tải tệp không cần thiết |
| 4. Thẩm tra và phê duyệt | Kết luận thẩm tra, trình/duyệt, ghi nhận ký, kiểm soát tác vụ và sửa nội dung sau trình | Đúng người mới được thao tác; nội dung thay đổi sau duyệt phải kiểm tra/duyệt lại; đủ lịch sử người và thời gian |
| 5. Hoàn tất và trả | Ghi nhận kết quả đăng ký, tham chiếu sổ/ký/cấp; đồng bộ nhân khẩu và biến động một lần; trả kết quả, mẫu in/lưu trữ | Chỉ hồ sơ hoàn tất mới cập nhật dân cư; lỗi đồng bộ có thể thử lại; cấp/trả có dấu vết; danh sách tổng giữ hành vi hiện tại |
| 6. Mở rộng và tích hợp | Trẻ chưa xác định cha/mẹ/bị bỏ rơi, yếu tố nước ngoài, đăng ký lại, kết hợp nhận cha/mẹ/con, liên thông khi có nhu cầu và kết nối | Quy tắc/chứng cứ/thời hạn riêng từng thủ tục; đăng ký lại không sinh trùng người; tích hợp có đối soát |

Trong đợt 3, cần có phương án chứng cứ thay giấy chứng sinh cho luồng thông thường. Với nhánh đặc biệt chưa hoàn thiện, giao diện phải nhận diện và hướng dẫn xử lý phù hợp, không để hoàn tất qua form thông thường chỉ bằng cách bỏ chọn cha/mẹ.

Thời hạn, lịch làm việc, ngày nghỉ, phí/miễn phí và mẫu văn bản cấu hình theo loại thủ tục, địa phương và ngày hiệu lực. Không tính mọi hồ sơ theo tổng 13 ngày. Ngày hẹn không được tùy ý kéo dài; khi cần hẹn lại phải lưu căn cứ và văn bản theo quy trình áp dụng.

Thao tác với hệ thống ký/hộ tịch bên ngoài và DB địa phương không thể nằm trong một transaction DB duy nhất. Lưu tham chiếu và trạng thái đối soát, retry có khóa chống trùng; không giữ transaction DB mở trong khi chờ ký hoặc gọi hệ thống ngoài.

## 8. Kiểm thử và điều kiện hoàn thành

- Hồ sơ đủ đi hết luồng: chưa tạo dân cư ở nháp/tiếp nhận/thẩm tra; hoàn tất tạo đúng một người, một liên kết hộ và một biến động khi cần tạo người mới.
- Hồ sơ thiếu chứng cứ: yêu cầu bổ sung có nội dung, người lập và văn bản; bổ sung được ghi thành từng lần; từ chối không tạo dân cư.
- Giấy tờ xuất trình chỉ lưu thông tin/xác nhận khi đủ điều kiện; không bị bắt tải bản sao. Chứng cứ thay thế giấy chứng sinh đi đúng nhánh.
- Hồ sơ có căn cứ tiếp nhận nhưng không có hộ trong DB vẫn lưu được; chọn người/hộ khác phạm vi bị chặn; cha/mẹ khác hộ xử lý theo quyền truy cập và chứng cứ.
- Tự điền không ghi đè dữ liệu đã được xác nhận một cách im lặng; thay người/hộ không giữ dữ liệu cũ; mặc định quốc tịch/dân tộc phải được xác nhận.
- Không được bỏ qua bước bằng API, tự gán phê duyệt hoặc dùng Admin kỹ thuật làm người ký; kiểm tra chuyển bước và cập nhật đồng thời.
- Hai thao tác hoàn tất cùng lúc hoặc retry sau timeout không sinh trùng. Cảnh báo nghi trùng theo trẻ/cha mẹ/ngày sinh không được tự chặn nhầm cặp song sinh.
- Lỗi khi ghi dân cư/audit rollback dữ liệu địa phương; lỗi kết nối ngoài có thể đối soát và thử lại, không tạo số đăng ký/nhân khẩu mới vô hạn.
- Sau đăng ký, chỉnh sửa phải theo tác vụ có căn cứ và lịch sử; không sửa trực tiếp snapshot/kết quả đã ký.
- Các biến động cũ không có hồ sơ hoặc thông tin ký vẫn mở được và không bị gắn trạng thái phê duyệt giả.
- Danh sách phân trang từ server; bộ lọc và Excel nhất quán; thôn/đơn vị và trường ngày áp dụng được kiểm tra.
- Nhánh đăng ký lại liên kết người đã có; nhánh đặc biệt có dữ liệu/chứng cứ riêng, không lấy giá trị mặc định của form thông thường làm kết luận.

Khi triển khai đợt dữ liệu mới sẽ cần migration DB. Lượt phân tích này chỉ bổ sung tài liệu kế hoạch, chưa yêu cầu cập nhật DB.
