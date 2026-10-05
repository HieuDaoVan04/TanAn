# Đối chiếu bốn tài liệu mẫu báo cáo

## Cập nhật sau khi nhận đề cương và phiếu giao

Đã đọc toàn bộ 2 file trong Downloads: PhieuGiaoDeTai_DaoVanHieu.docx (2 trang), DaoVanHieu_ĐecuongTN_2200454.docx (5 trang), trích cả các bảng và xem tất cả trang PDF xuất từ Word. Không sửa nguồn.

Thông tin xác nhận: Đào Văn Hiếu; mã SV 2200454; lớp K4628-CNTT; khóa 46, chính quy; ngành và chuyên ngành Công nghệ thông tin; Khoa Công nghệ thông tin, Trường Đại học Công nghiệp Việt–Hung; GVHD ThS. Vũ Hùng Cường; năm 2026. Tên đề tài: Nghiên cứu và xây dựng nền tảng số hỗ trợ quản lý biến động dân cư, an sinh xã hội và điều hành tại xã Tân An trên dữ liệu mô phỏng.

Đề cương ghi thời gian dự kiến 18/08/2026–24/10/2026; ký ngày 11/09/2026 nhưng chưa có chữ ký. Phiếu giao để trống thời gian thực hiện và ngày ký; không tự điền lại bản phiếu gốc. Tiến độ trong đề cương là dự kiến, không được biến thành nhật ký công việc đã hoàn thành. Phiếu giao liệt kê 12 sản phẩm, đề cương gom thành 11 (mục cuối gộp API với báo cáo/mã nguồn/hướng dẫn), không có mâu thuẫn nội dung.

Nhiệm vụ: hộ/nhân khẩu và lịch sử; khai sinh/khai tử/chuyển đi/chuyển đến/tạm trú; hộ nghèo/cận nghèo/người cao tuổi/đối tượng chính sách; yêu cầu/hồ sơ điện tử; dashboard; dữ liệu mô phỏng; RBAC/audit; API; AI trùng lặp/bất thường/phân loại/trợ lý thủ tục; kiểm thử và đánh giá hiệu năng/độ chính xác.

Khung KTĐH K45 không phù hợp ngành và khóa thực tế. Đề xuất 3 chương giữ mẫu chung: (1) Tổng quan đề tài và cơ sở lý thuyết; (2) Phân tích và thiết kế hệ thống (yêu cầu, use case, hoạt động, kiến trúc, ERD, API, UI, dữ liệu mô phỏng, thiết kế AI, phân quyền); (3) Xây dựng, kiểm thử và đánh giá (công nghệ thực tế, các phân hệ, hình giao diện thực, triển khai AI, kiểm thử, số liệu, hạn chế). Mở đầu gồm đặt vấn đề/mục tiêu/nhiệm vụ/phạm vi/phương pháp/kết quả theo thực tế/bố cục; kết luận và hướng phát triển, TLTK, phụ lục. Đây là đề xuất từ đề cương, không phải chương mục đã được duyệt sẵn trong file.

Đã gửi câu hỏi bất đồng bộ: dùng mã nguồn D:/Đồ Án hay phiên bản sản phẩm khác. Chưa nhận trả lời ở thời điểm ghi chú. Cần chốt nguồn triển khai trước khi viết phần kết quả hoàn chỉnh. Đề cương chỉ nêu Web Full-stack, không chỉ định React hay Blazor.

Kiểm tra sơ bộ thư mục hiện có cho thấy README/HUONG_DAN_CAI_DAT mô tả cũ React+Python, nhưng mã đang dùng src/Service.UI/Service.UI.CMS.Blazor (net10.0, FluentUI, Blazor-ApexCharts), .NET API, EF Core 9.0.2, PostgreSQL/SQLite; Program.cs có InteractiveServer, cookie, Redis session, Npgsql. Không dùng README làm bằng chứng tất cả tính năng đã hoàn thành.

Bằng chứng quan trọng: src/Service.TanAn/Service.TanAn.Application/Services/AIService.cs có DetectDuplicatesAsync và DetectAnomaliesAsync trả danh sách rỗng; ClassifyRequestAsync trả hằng SuggestedCategory Xác nhận cư trú, ConfidenceScore 0.95; không phải kết quả đánh giá 95%. ChatProcedureAsync có lời gọi Gemini và thông báo dự phòng, chưa kiểm chứng tích hợp chạy thành công. Không được viết rằng mô hình đã đạt độ chính xác hoặc phần AI đã hoàn tất.

docs/ENTITY_MODEL.md có 11 entity nghiệp vụ và các quan hệ ERD; còn ghi ThanhVienHo/LichSuXuLyHoSo/TepDinhKem chưa được service cũ tự ghi. Cần kiểm tra mã mới nếu dùng dự án này, không sao chép tuyên bố cũ nguyên trạng.
Tools/DemoPopulation/README.md mô tả 11 thôn, mã DEMO và dữ liệu tổng hợp; khác README gốc 5 ấp/500 hộ/2000 người. Không dùng các con số đó như số dân thực địa phương hoặc số lượng DB đã xác minh. Không kết nối hoặc sửa DB để viết báo cáo.

Nguồn kỹ thuật sơ bộ đã tra cứu chính thức (chưa đưa vào báo cáo): Microsoft Learn Blazor render modes; EF Core Transactions; Efficient Querying; PostgreSQL 18 Constraints; OWASP Authorization Cheat Sheet. Khi soạn, chỉ dẫn chính xác URL và phạm vi dùng; không coi đề cương nhắc VNeID là bằng chứng hệ thống đã tích hợp VNeID. Không tự khẳng định thủ tục/pháp luật hiện hành khi chưa kiểm tra nguồn chính thức.

Đã đọc ngày 28/09/2026. Yêu cầu hiện tại: đọc kỹ để chuẩn bị; chờ đề cương và phiếu giao đề tài trước khi soạn báo cáo Word .doc. Không tự sáng tác dữ liệu, kết quả, nhật ký, nhận xét hoặc chữ ký. File gốc không thay đổi.

## Nguồn

Thư mục nguồn: C:/Users/ADMIN/Downloads/Mẫu 2025/Mẫu 2025/
- 1._HD_Trinh_bay_DATN_2025.doc: văn bản quy định chính; xuất bằng Word được 13 trang.
- 2025_MauBaoCao.doc: khung báo cáo 15 trang, gồm nhận xét và chỉ dẫn kiểu chữ.
- 2.Mẫu trang bìa.doc: bìa chính và bìa lót, 2 trang.
- Mẫu ĐATN K45 KTĐH 2025.docx: khung nội dung thiết kế đồ họa, 1 trang nội dung và 1 trang trắng khi Word xuất PDF.
Đã trích xuất nội dung và xem ảnh tổng hợp toàn bộ các trang PDF. Bản .docx chuyển đổi và PDF trong thư mục này chỉ là dữ liệu kiểm tra, không phải báo cáo cuối.

## Quy định cần áp dụng

- A4 210 x 297 mm, in một mặt; lề trái 3 cm, phải/trên/dưới 2 cm; khoảng cách header/footer 1,27 cm.
- Times New Roman Unicode 13; giãn dòng Multiple 1,3; căn đều hai bên, khoảng cách ký tự bình thường.
- Số trang ở giữa đầu trang, bắt đầu số 1 tại Lời nói đầu hoặc Lời cảm ơn.
- Trật tự: bìa (mẫu có bìa chính và bìa lót), phiếu giao đề tài, nhận xét GVHD, lời nói đầu/cảm ơn, nhật ký, mục lục, danh mục ký hiệu/chữ viết tắt/bảng/hình, mở đầu, các chương, kết luận, tài liệu tham khảo, phụ lục.
- Mỗi chương bắt đầu trang mới. Chương/mục dùng 1, 2, 3; tối đa 4 cấp dạng 4.2.3.1. Nhóm chia nhỏ phải có ít nhất 2 tiểu mục. Mục lục chỉ đến 3 cấp. Không gạch dưới hoặc đặt dấu hai chấm cuối tên chương/mục.
- Mẫu báo cáo ghi tiêu đề phần/chương đậm 16; tóm tắt mỗi chương khoảng 10 dòng nghiêng 13, đặt đầu chương theo mẫu.
- Mở đầu: đặt vấn đề; mục tiêu; nhiệm vụ; kết quả đạt được (phải căn cứ thực tế); bố cục đề tài.
- Bảng/hình/công thức đánh số theo chương. Tên bảng ở trên, tên hình ở dưới, canh giữa, cỡ bằng thân bài. Giữ chú thích cùng hình/bảng ngắn. Dẫn chiếu số cụ thể trong nội dung. Công thức số trong ngoặc đơn, canh phải; giải thích ký hiệu và đơn vị khi xuất hiện lần đầu.
- Viết đầy đủ rồi mới viết tắt trong ngoặc. Trên 10 chữ viết tắt bắt buộc danh mục ABC; mẫu có sẵn danh mục để điền nếu dùng.
- Trích dẫn [n], khi cần [n, tr.x-y]; nhiều tài liệu tách [3], [5] theo số tăng dần. Trích gián tiếp phải nêu rõ, không đưa nguồn gốc chưa tiếp cận vào danh mục.
- Tài liệu tham khảo chia theo ngôn ngữ và xếp ABC tên tác giả/cơ quan theo thông lệ; giữ nguyên ngôn ngữ. Sách: tác giả, tên sách in nghiêng, NXB, nơi và năm xuất bản. Bài báo: tác giả, tên bài trong ngoặc kép, tạp chí in nghiêng, tập(số), năm, trang. Hạn chế website, nhất là cá nhân. Dòng sau có thể thụt 1 cm.
- Phụ lục sau tài liệu tham khảo, ký hiệu A/B/C theo trình tự nội dung, không dài hơn phần chính.
- Tối thiểu 40 trang chỉ ghi cho thực tập tốt nghiệp, 25 cho đồ án học phần, không tính phụ lục/bìa/bìa lót. Chưa quy định riêng mức tối thiểu đồ án tốt nghiệp.

## Khung nội dung K45 KTĐH

Chương 1 tổng quan đối tượng thiết kế: khái niệm, phân loại, vai trò/chức năng, thực trạng trong và ngoài nước.
Chương 2 ngôn ngữ đồ họa trong thiết kế: màu sắc, chữ, mảng, nét, bố cục; chất liệu/kết cấu nếu thiết kế bao bì; chuẩn nếu sử dụng; ứng dụng của đồ án thiết kế.
Chương 3 ứng dụng: công ty (lịch sử, cơ cấu liên quan, thực trạng thiết kế), công cụ đã học, ứng dụng đồ án (ý tưởng và thể hiện).
Tên chương/mục cụ thể phải đối chiếu đề cương được duyệt và phiếu giao đề tài khi nhận được.

## Chỗ lệch và cách xử lý dự kiến

Đây là quyết định biên tập đề xuất, không phải thứ bậc văn bản được tài liệu tuyên bố. Ưu tiên quy định viết rõ cho định dạng; mẫu K45 cho nội dung ngành; mẫu báo cáo cho thứ tự/phần bổ sung; mẫu bìa cho bố cục. Đề cương và phiếu giao sẽ xác định thông tin thật và phạm vi.
- Quy định không Header/Footer nhưng đòi số trang đầu trang: hiểu là không thêm tiêu đề/chữ trang trí, chỉ số trang; cần nêu đây là cách dung hòa.
- Mẫu báo cáo dùng CHƯƠNG I/II/III; hướng dẫn cấm số La Mã cho chương: dùng CHƯƠNG 1/2/3.
- Mẫu số trang từ phiếu giao (lời cảm ơn đang số 4); hướng dẫn từ lời cảm ơn số 1: theo hướng dẫn.
- Mẫu K45 thực tế dùng Letter 21,59 x 27,94 cm và lề 2,54 cm: lấy khung nội dung, chuyển A4 và lề theo quy định.
- Styles mẫu báo cáo còn Normal .VnTime 12, Heading Arial: không giữ các giá trị này; chuẩn hóa Times New Roman Unicode theo quy định.
- Mẫu bìa chứa tên người giả, đề tài phần mềm quản lý bán hàng, đồ án học phần, chuyên ngành CNTT, năm 2025. Thay bằng dữ liệu phiếu giao; chưa tự kết luận tên khoa/chuyên ngành chính thức.
- Nhật ký mẫu có ngày 7/11/2025 chỉ là dữ liệu mẫu. Không dùng làm ngày thực tế.
- Các chữ IPSEC/IPV4/IPV6/UFS/WAN, website Linux/Quantrimang và tài liệu mạng không mặc nhiên là tài liệu/thuật ngữ của đồ án.
- Cách ghi năm tài liệu tham khảo trong mẫu khác hướng dẫn: theo phần hướng dẫn chi tiết.
- Hướng dẫn dẫn đến Phụ lục E nhưng phần thực tế là A. Không sao chép lỗi này.

## Nộp và bảo vệ theo tài liệu nguồn

Nộp 1 quyển GVHD và 1 USB/CD với TT/ĐATN; nhận xét đóng kèm; có yêu cầu 1 USB/CD mỗi lớp. Dữ liệu gồm Readme, Thesis (.doc), Pdf (.pdf), Resource, Source. Có quy định riêng về sửa sau bảo vệ trong tối đa 1 tuần và nộp thư viện bìa cứng chữ nhũ; thời hạn cụ thể theo thông báo. Trình bày không quá 10 phút hoặc theo yêu cầu GV. Báo cáo tiến độ hàng tuần. Điều khoản phát triển module khi dùng mã nguồn mở nằm trong hướng dẫn CNTT; không tự áp vào đề tài đồ họa không liên quan.

## Bước tiếp theo

Nhận đề cương và phiếu giao; trích chính xác tên đề tài, người làm, mã SV, lớp/khóa, khoa/chuyên ngành, GVHD, thời gian, nhiệm vụ. Nhận thêm hình sản phẩm và dữ liệu thực tế khi cần. Soạn .doc thật bằng chuyển đổi Word, không đổi đuôi .docx. Cập nhật mục lục/số hình/bảng/trang và kiểm tra trực quan bản .doc sau chuyển đổi trước giao.
Word COM chạy trong sandbox báo lỗi logon; chạy escalated đọc được. Word thường lỗi RPC lúc Quit sau Close, nhưng dữ liệu trích xuất/xuất PDF đã thành công. Dùng mở riêng từng file. Python runtime bundled có docx, pypdfium2, pypdf, PIL; không có fitz/olefile. Không có bundled LibreOffice trong kết quả dependency loader. Công cụ chuẩn render_docx.py chưa được dùng vì nguồn .doc và môi trường Windows; PDF kiểm tra được xuất trực tiếp bằng Word.
