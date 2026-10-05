# MỞ ĐẦU
## I. Đặt vấn đề
Quản lý dân cư ở cấp xã liên quan đến nhiều nhóm thông tin có quan hệ chặt chẽ: hộ gia đình, thành viên của hộ, địa bàn cư trú, biến động nhân khẩu, đối tượng an sinh và yêu cầu của người dân. Một thay đổi ở hồ sơ nhân khẩu có thể ảnh hưởng đến lịch sử thành viên, số liệu thống kê hoặc danh sách đối tượng được theo dõi. Vì vậy, bài toán không chỉ là lưu trữ biểu mẫu mà còn là duy trì sự nhất quán giữa dữ liệu hiện tại, lịch sử thay đổi và trách nhiệm xử lý.
Đề tài “Nghiên cứu và xây dựng nền tảng số hỗ trợ quản lý biến động dân cư, an sinh xã hội và điều hành tại xã Tân An trên dữ liệu mô phỏng” được triển khai trong phạm vi đồ án tốt nghiệp ngành Công nghệ thông tin. Phạm vi này cho phép xây dựng và kiểm tra các luồng nghiệp vụ bằng dữ liệu tổng hợp, đồng thời tránh đồng nhất kết quả thử nghiệm phần mềm với số liệu dân cư chính thức. Những nhận định về nhu cầu quản lý trong báo cáo được dùng để xác lập bài toán thiết kế; không thay thế kết quả khảo sát hành chính tại địa phương [1], [2].
Nền tảng được tổ chức theo hướng dùng chung dữ liệu giữa các phân hệ. Cán bộ có thể tra cứu hộ và nhân khẩu, ghi nhận biến động, quản lý đối tượng an sinh và theo dõi yêu cầu. Công tác điều hành được hỗ trợ qua các chỉ số tổng hợp. Các chức năng trí tuệ nhân tạo được nghiên cứu như công cụ hỗ trợ phát hiện và gợi ý, trong khi việc xác nhận hồ sơ và xử lý nghiệp vụ vẫn thuộc trách nhiệm của người sử dụng có thẩm quyền.
## II. Mục tiêu và nhiệm vụ của đề tài
Mục tiêu tổng quát là nghiên cứu, thiết kế và xây dựng ứng dụng Web phục vụ quản lý dân cư, an sinh xã hội và tiếp nhận yêu cầu trên dữ liệu mô phỏng của bài toán xã Tân An. Các mục tiêu cụ thể gồm tổ chức cơ sở dữ liệu quan hệ, xây dựng các dịch vụ nghiệp vụ, cung cấp giao diện quản lý, triển khai phân quyền theo vai trò và địa bàn, ghi nhận thao tác, đồng thời nghiên cứu các chức năng AI theo phiếu giao đề tài.
Nhiệm vụ nghiên cứu được chia thành các bước: phân tích đối tượng và luồng thông tin; xây dựng mô hình dữ liệu; thiết kế kiến trúc và API; hiện thực các phân hệ; xây dựng công cụ tạo dữ liệu tổng hợp; kiểm tra ràng buộc và phạm vi truy cập; đánh giá phần đã triển khai; xác định phần cần hoàn thiện. Trình tự này tạo mối liên hệ từ yêu cầu đầu vào đến kết quả kiểm chứng, hạn chế việc đánh giá hệ thống chỉ bằng số lượng màn hình.
## III. Đối tượng, phạm vi và phương pháp nghiên cứu
Đối tượng nghiên cứu là mô hình quản lý hộ gia đình, nhân khẩu, biến động, an sinh và hồ sơ điện tử cùng các giải pháp phần mềm hỗ trợ chúng. Dữ liệu nghiên cứu là dữ liệu mô phỏng. Tên địa bàn trong ứng dụng có vai trò tổ chức và phân vùng dữ liệu; những số lượng được tạo trong chương trình không được xem là kết quả điều tra dân số.
Thời gian dự kiến theo đề cương là từ ngày 18/08/2026 đến ngày 24/10/2026. Báo cáo đánh giá phiên bản mã nguồn và các kiểm tra độc lập tại ngày 28/09/2026. Do đó, các hạng mục dự kiến trong đề cương và các chức năng đã được kiểm chứng được trình bày tách biệt, đặc biệt đối với AI và đánh giá hiệu năng.
Phương pháp nghiên cứu gồm phân tích tài liệu, mô hình hóa hệ thống, thực nghiệm lập trình và kiểm thử bằng dữ liệu tổng hợp. Việc kiểm tra cấu trúc mã nguồn giúp xác định thành phần đã tồn tại; thử nghiệm trên SQLite trong bộ nhớ giúp kiểm tra một số ràng buộc và luồng nghiệp vụ mà không thay đổi cơ sở dữ liệu của ứng dụng. Hai phương pháp bổ sung cho nhau nhưng không thay thế kiểm thử tích hợp trên môi trường triển khai.
## IV. Kết quả đạt được và giới hạn đánh giá
Phiên bản hiện có đã hình thành cấu trúc ứng dụng Blazor/.NET, lớp dịch vụ nghiệp vụ, mô hình dữ liệu dân cư và an sinh, các API, cơ chế phân vùng dữ liệu theo thôn và công cụ tạo dữ liệu mô phỏng. Kiểm tra độc lập xác nhận khả năng chạy lặp công cụ sinh dữ liệu mà không thêm trùng bộ dữ liệu thử; đồng thời xác nhận một số tình huống đọc, ghi và thu hồi quyền theo địa bàn. Kết quả chi tiết được trình bày tại Chương 3.
Các chức năng phát hiện trùng lặp, phát hiện bất thường và phân loại yêu cầu chưa có kết quả thực nghiệm của mô hình hoàn chỉnh. Phần trợ lý có cấu trúc gọi dịch vụ bên ngoài nhưng chưa được đánh giá độ đúng của câu trả lời. Những nội dung này được trình bày ở mức thiết kế và trạng thái triển khai, không được dùng để khẳng định hệ thống đã đáp ứng đầy đủ tất cả mục tiêu AI.
## V. Bố cục báo cáo
Chương 1 trình bày bài toán, cơ sở lý thuyết và lựa chọn kiến trúc. Chương 2 phân tích yêu cầu, mô hình dữ liệu, luồng xử lý, API và thiết kế các chức năng AI. Chương 3 mô tả việc hiện thực, trình bày các kết quả kiểm tra, đánh giá hạn chế và đề xuất hướng hoàn thiện. Phần cuối gồm kết luận, tài liệu tham khảo và các phụ lục phục vụ kiểm tra, cài đặt và đối chiếu yêu cầu.
# CHƯƠNG 1\nTỔNG QUAN ĐỀ TÀI VÀ CƠ SỞ LÝ THUYẾT
~Chương này xác định phạm vi của nền tảng quản lý dân cư và an sinh trong bối cảnh đồ án. Các khái niệm hộ, nhân khẩu, biến động và hồ sơ được phân biệt để tránh gộp những đối tượng có vòng đời khác nhau. Trên cơ sở đó, chương trình bày cách tổ chức ứng dụng Web, dữ liệu quan hệ và các nguyên tắc kiểm soát truy cập. Phần cuối nêu vai trò hỗ trợ của AI, yêu cầu đối với dữ liệu mô phỏng và những tiêu chí dùng để đánh giá sản phẩm. Đây là cơ sở cho các quyết định thiết kế ở Chương 2 và việc đánh giá triển khai ở Chương 3.
## 1.1. Tổng quan bài toán quản lý tại cấp xã
### 1.1.1. Hộ gia đình và nhân khẩu
Trong mô hình của đề tài, hộ gia đình là đơn vị nhóm các thành viên theo quan hệ cư trú và quản lý. Mỗi hộ có một mã riêng, địa chỉ, địa bàn và thông tin chủ hộ. Nhân khẩu là bản ghi về một cá nhân trong dữ liệu mô phỏng, có họ tên, ngày sinh, giới tính, thông tin định danh nếu có và quan hệ với hộ. Một hộ có nhiều nhân khẩu; một nhân khẩu có một liên kết hộ hiện tại và có thể có nhiều giai đoạn thành viên trong lịch sử.
Việc tách hộ khỏi nhân khẩu tránh lặp lại toàn bộ thông tin địa chỉ và thông tin chung của hộ ở từng nghiệp vụ. Tuy nhiên, chỉ giữ liên kết hiện tại chưa đủ để truy vết. Khi chuyển hộ hoặc thay chủ hộ, hệ thống cần lưu trạng thái trước và sau, ngày có hiệu lực và người thao tác. Đây là lý do mô hình bổ sung bảng lịch sử thành viên thay vì ghi đè mọi thông tin cũ.
### 1.1.2. Biến động dân cư và quản lý an sinh
Biến động là sự kiện gắn với nhân khẩu như khai sinh, khai tử, tạm trú, tạm vắng, chuyển đến hoặc chuyển đi trong phạm vi mô phỏng. Mỗi sự kiện có loại, ngày phát sinh, lý do và thông tin ghi nhận. Cần phân biệt thời điểm sự kiện có hiệu lực với thời điểm bản ghi được nhập vào phần mềm, vì hai thời điểm này có thể khác nhau.
An sinh có cả đối tượng cấp hộ và đối tượng cấp cá nhân. Phân loại hộ nghèo hoặc cận nghèo gắn với hộ trong một giai đoạn; chế độ của người cao tuổi hoặc đối tượng chính sách gắn với nhân khẩu. Mức trợ cấp và lịch sử chi trả là dữ liệu nghiệp vụ của bộ mô phỏng, không phải mức hưởng được xác lập từ quy định pháp luật trong báo cáo này. Thiết kế cần giữ được căn cứ, thời hạn và lịch sử điều chỉnh để có thể mở rộng sau này.
### 1.1.3. Hồ sơ điện tử và điều hành
Hồ sơ điện tử tập hợp thông tin người gửi, loại yêu cầu, nội dung, thời gian tiếp nhận, trạng thái và cán bộ xử lý. Quy trình theo dõi hồ sơ phải trả lời được ba câu hỏi: hồ sơ đang ở bước nào, ai đang phụ trách và đã có những thay đổi gì. Chỉ lưu trạng thái mới nhất sẽ không giải thích được quá trình xử lý khi cần đối chiếu.
Dashboard cung cấp góc nhìn tổng hợp theo địa bàn, loại đối tượng và trạng thái hồ sơ. Chỉ số tổng hợp phải có định nghĩa rõ ràng về đối tượng được đếm và kỳ thống kê. Chẳng hạn, số bản ghi nhân khẩu lưu trong cơ sở dữ liệu không tự động đồng nghĩa với số người đang cư trú; cần quy định cách xử lý người đã chuyển đi hoặc đã khai tử trước khi dùng chỉ số phục vụ điều hành.
## 1.2. Phạm vi giải pháp và các nguyên tắc xây dựng
### 1.2.1. Phạm vi chức năng
Giải pháp bao gồm nhóm dân cư, nhóm biến động, nhóm an sinh, nhóm hồ sơ người dân và nhóm quản trị. Các nhóm dùng chung cơ sở dữ liệu và cơ chế định danh người dùng. Bảng 1.1 xác định mục đích và sản phẩm thông tin của từng nhóm; đây cũng là căn cứ để phân chia trách nhiệm giữa các lớp dịch vụ.
@table Bảng 1.1. Phạm vi các nhóm chức năng
Nhóm chức năng | Mục đích | Thông tin đầu ra
Dân cư | Quản lý hộ, thành viên, chủ hộ | Danh sách, chi tiết và lịch sử thành viên
Biến động | Ghi nhận sự kiện nhân khẩu | Sổ biến động và thông tin trạng thái
An sinh | Theo dõi đối tượng và chi trả | Danh sách đối tượng, lịch sử trợ cấp
Hồ sơ | Tiếp nhận và theo dõi yêu cầu | Mã hồ sơ, trạng thái, thông tin xử lý
Điều hành | Tổng hợp thông tin theo phạm vi | Chỉ số và biểu đồ thống kê
Quản trị | Quản lý người dùng, quyền, địa bàn | Quyền truy cập và nhật ký thao tác
AI hỗ trợ | Phát hiện, phân loại và gợi ý | Cảnh báo, nhãn gợi ý, câu trả lời tham khảo
@end
### 1.2.2. Ranh giới của hệ thống
Nền tảng là sản phẩm nghiên cứu trên dữ liệu mô phỏng, không được mô tả như hệ thống thay thế cơ sở dữ liệu quốc gia hoặc phần mềm cấp giấy tờ chính thức. Việc lưu thông tin một yêu cầu trong ứng dụng không tương đương với việc hồ sơ đã được cơ quan có thẩm quyền tiếp nhận theo thủ tục thực tế. Tương tự, ghi nhận chi trả trong bộ mô phỏng không thực hiện giao dịch tài chính.
Trong phạm vi đồ án, tính đúng được xem xét ở cấp mô hình dữ liệu và luồng xử lý phần mềm. Khả năng áp dụng thực tế còn phụ thuộc vào quy trình được địa phương xác nhận, nguồn dữ liệu hợp lệ, hạ tầng, cơ chế tích hợp và đánh giá vận hành. Các điều kiện đó được đặt trong hướng phát triển thay vì xem là kết quả đã đạt được.
## 1.3. Cơ sở công nghệ của ứng dụng Web
### 1.3.1. Blazor và ASP.NET Core
Blazor tổ chức giao diện theo các thành phần Razor. Chế độ Interactive Server thực hiện tương tác ở phía máy chủ, khác với mô hình tải toàn bộ mã xử lý giao diện để chạy bằng WebAssembly tại trình duyệt [5]. Dự án cấu hình dịch vụ thành phần tương tác và ánh xạ chế độ Interactive Server trong chương trình khởi động. Cách tổ chức này phù hợp với việc dùng C# cho cả thành phần giao diện và lớp nghiệp vụ.
ASP.NET Core Web API cung cấp các điểm truy cập HTTP cho dữ liệu và thao tác. Trong cấu trúc hiện có, ứng dụng giao diện và dự án API là hai thành phần riêng; một số nghiệp vụ được giao diện gọi qua dịch vụ, còn luồng quản trị có lớp vận chuyển HTTP. Vì vậy, khi mô tả kiến trúc phải thể hiện đúng các đường gọi đang có, không mặc định mọi thao tác giao diện đều đi qua một cổng API duy nhất [3].
### 1.3.2. Phân lớp và hợp đồng dữ liệu
Mã nguồn được chia thành Domain, Application, Infrastructure và các dự án giao diện/API, kèm các thư viện dùng chung. Domain chứa thực thể và kiểu dữ liệu nghiệp vụ. Application chứa giao diện dịch vụ và xử lý các ca sử dụng. Infrastructure ánh xạ thực thể, truy cập cơ sở dữ liệu và cung cấp các thành phần kỹ thuật. Các DTO xác định dữ liệu được trao đổi với giao diện hoặc API.
Việc phân lớp giúp tách thay đổi giao diện khỏi thay đổi cấu trúc lưu trữ. Tuy nhiên, tên thư mục tự nó không chứng minh kiến trúc đã hoàn toàn độc lập. Một số dịch vụ hiện còn phụ thuộc trực tiếp vào cách hoạt động của Entity Framework Core. Báo cáo sử dụng thuật ngữ kiến trúc phân lớp để mô tả cấu trúc thực tế, đồng thời xem việc giảm phụ thuộc giữa các lớp là yêu cầu bảo trì.
### 1.3.3. Entity Framework Core và cơ sở dữ liệu quan hệ
Entity Framework Core ánh xạ các lớp thực thể sang bảng quan hệ và hỗ trợ truy vấn qua LINQ. Khóa chính định danh bản ghi; khóa ngoại duy trì liên kết; ràng buộc duy nhất và kiểm tra giúp từ chối dữ liệu không hợp lệ ở tầng lưu trữ. Theo tài liệu PostgreSQL, các ràng buộc này có chức năng khác nhau và cần phối hợp khi thiết kế mô hình [9].
Trong nghiệp vụ thay chủ hộ, nhiều thay đổi phải cùng thành công hoặc cùng được hủy. Giao dịch cho phép nhóm các bước lưu thành một đơn vị nhất quán. EF Core hỗ trợ giao dịch và việc chủ động quản lý giao dịch khi một thao tác có nhiều lần lưu [7]. Thiết kế của đề tài sử dụng nguyên tắc này để tránh trạng thái có hai chủ hộ hiện hành hoặc mất liên kết lịch sử trong quá trình cập nhật.
## 1.4. Quản lý quyền và lịch sử thao tác
### 1.4.1. Quyền chức năng và phạm vi dữ liệu
Quyền chức năng xác định người dùng được mở hoặc thao tác với phân hệ nào. Phạm vi dữ liệu xác định những bản ghi người đó được truy cập, chẳng hạn các hộ thuộc thôn được phân công. Hai điều kiện phải được kiểm tra đồng thời. Một người có quyền sử dụng màn hình dân cư không mặc nhiên được đọc hoặc sửa tất cả các thôn.
Nguyên tắc từ chối mặc định và kiểm tra quyền ở phía máy chủ được OWASP khuyến nghị nhằm tránh việc chỉ dựa vào trạng thái hiển thị của giao diện [8]. Áp dụng vào đề tài, việc ẩn nút là hỗ trợ trải nghiệm; quyết định cho phép đọc hoặc ghi phải được kiểm tra lại tại dịch vụ và tầng dữ liệu. Thay đổi trực tiếp mã bản ghi trong yêu cầu cũng phải chịu cùng điều kiện kiểm soát.
### 1.4.2. Nhật ký và khả năng truy vết
Nhật ký thao tác mô tả hành động của người dùng đối với một đối tượng tại một thời điểm. Lịch sử nghiệp vụ lại mô tả diễn biến của đối tượng, chẳng hạn các giai đoạn thành viên hộ hoặc các lần xử lý hồ sơ. Hai loại lịch sử bổ sung cho nhau: nhật ký phục vụ truy vết thao tác, trong khi bảng lịch sử phục vụ truy vấn và giải thích diễn biến nghiệp vụ.
Một bản ghi nhật ký hữu ích cần chỉ ra người thực hiện đã được xác thực, hành động, loại đối tượng, mã đối tượng, thời điểm và thông tin thay đổi cần thiết. Nếu người thao tác được lấy từ trường do phía gửi tự điền thì chưa đủ cơ sở xác nhận danh tính. Khi hoàn thiện hệ thống, cần thống nhất cách lấy người thao tác từ phiên đăng nhập cho mọi đường gọi.
## 1.5. AI hỗ trợ và dữ liệu mô phỏng
### 1.5.1. Vai trò của các chức năng AI
Phát hiện trùng lặp nhằm đưa ra các cặp hồ sơ có khả năng thuộc cùng một người dù cách ghi tên hoặc địa chỉ khác nhau. Phát hiện bất thường nhằm đánh dấu những tổ hợp dữ liệu cần kiểm tra, ví dụ sự kiện có thời điểm không phù hợp với ngày sinh. Phân loại yêu cầu nhằm gợi ý nhóm xử lý dựa trên nội dung; trợ lý thủ tục nhằm hỗ trợ tra cứu thông tin bằng ngôn ngữ tự nhiên.
Các chức năng trên có mức độ rủi ro và cách đánh giá khác nhau. Một cặp hồ sơ có điểm giống nhau cao vẫn có thể là hai người khác nhau. Một câu hỏi có nhiều ý có thể không thuộc duy nhất một nhóm. Vì vậy, thiết kế phải cung cấp cơ chế xem lại, sửa nhãn và từ chối kết luận khi thiếu dữ liệu. Không tự động hợp nhất hồ sơ hoặc quyết định chế độ an sinh chỉ dựa trên gợi ý của mô hình.
### 1.5.2. Yêu cầu đối với dữ liệu thử nghiệm
Dữ liệu tổng hợp phải vừa đa dạng vừa bảo toàn ràng buộc nền tảng. Hộ cần có thành viên và chủ hộ; các khóa ngoại phải trỏ tới bản ghi tồn tại; các bản ghi thử cần có dấu hiệu nhận biết; việc chạy lại công cụ phải có quy tắc để tránh nhân đôi. Những trường hợp lỗi phục vụ kiểm thử nên nằm trong bộ riêng hoặc được gắn nhãn, không trộn với bộ hợp lệ rồi coi mọi cảnh báo là lỗi phần mềm.
Đối với AI, nhãn đúng phải được xác định độc lập với kết quả dự đoán. Bộ kiểm tra cần chứa cả trường hợp dễ và khó, trong đó có người cùng họ tên nhưng khác ngày sinh, hồ sơ thiếu định danh, tên có dấu hoặc không dấu và yêu cầu chứa nhiều chủ đề. Chỉ số đánh giá phải công bố cùng quy mô và cách chia dữ liệu; một số độ tin cậy trả về từ API không thay thế kết quả đánh giá trên bộ kiểm tra.
## 1.6. Tiêu chí đánh giá và kết luận chương
Các tiêu chí đánh giá gồm tính đúng nghiệp vụ, nhất quán dữ liệu, kiểm soát quyền, truy vết và khả năng sử dụng. Hiệu năng và chất lượng AI cần có phép đo riêng.
# CHƯƠNG 2\nPHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG
~Chương này chuyển các nhiệm vụ trong phiếu giao thành yêu cầu có thể kiểm tra. Nội dung bắt đầu từ tác nhân, nhóm chức năng và các điều kiện phi chức năng. Tiếp theo là kiến trúc phân lớp, mô hình dữ liệu và luồng xử lý của các nghiệp vụ chính. Phần API và giao diện xác định cách người dùng tương tác với dữ liệu. Thiết kế AI được trình bày như phương án cần hoàn thiện và thực nghiệm. Các ràng buộc, chỉ số và tình huống kiểm tra ở cuối chương tạo cơ sở đối chiếu với phiên bản triển khai.
## 2.1. Tác nhân và yêu cầu hệ thống
### 2.1.1. Các nhóm tác nhân
Thiết kế phân biệt quản trị viên, cán bộ cấp xã, cán bộ được giao địa bàn, người gửi yêu cầu và người xem thông tin tổng hợp. Người xem tổng hợp là một vai trò nghiệp vụ; không nhất thiết tương ứng với một giá trị enum riêng trong mã nguồn. Quyền cụ thể phải dựa trên cấu hình vai trò, menu và địa bàn của tài khoản thay vì chỉ dựa vào tên gọi chức danh.
@table Bảng 2.1. Tác nhân và phạm vi trách nhiệm dự kiến
Tác nhân | Nhiệm vụ | Giới hạn truy cập
Quản trị viên | Cấu hình tài khoản, vai trò, menu và địa bàn | Thao tác quản trị phải được ghi nhận
Cán bộ cấp xã | Quản lý và tổng hợp thông tin theo nhiệm vụ | Phụ thuộc quyền được cấp
Cán bộ thôn | Xử lý dân cư, an sinh và hồ sơ địa bàn | Các thôn được phân công
Người gửi yêu cầu | Gửi nội dung và theo dõi hồ sơ của mình | Không xem danh sách dân cư chung
Người xem điều hành | Xem số liệu tổng hợp | Ưu tiên số liệu tổng hợp, hạn chế chi tiết cá nhân
@end
### 2.1.2. Yêu cầu chức năng
Mỗi yêu cầu được gắn mã để đối chiếu với thiết kế và kết quả. Bảng 2.2 biểu diễn yêu cầu mục tiêu, không phải danh sách tính năng đã hoàn thành. Các yêu cầu liên quan AI, lịch sử hồ sơ và đánh giá hiệu năng cần được nghiệm thu bằng những bằng chứng riêng.
@table Bảng 2.2. Danh sách yêu cầu chức năng
Mã | Yêu cầu | Tiêu chí kiểm tra chính
F01 | Quản lý hộ và chủ hộ | Mã hộ duy nhất; chủ hộ thuộc hộ
F02 | Quản lý nhân khẩu | Liên kết hộ hợp lệ; tra cứu và phân trang
F03 | Theo dõi lịch sử thành viên | Không có hai giai đoạn mở cho một người
F04 | Ghi nhận biến động | Lưu loại, ngày, người ghi và trạng thái liên quan
F05 | Quản lý an sinh | Tách đối tượng, phân loại hộ và lịch sử chi trả
F06 | Tiếp nhận yêu cầu | Mã hồ sơ duy nhất; địa bàn tiếp nhận hợp lệ
F07 | Theo dõi xử lý hồ sơ | Trạng thái và lịch sử giải thích được
F08 | Thống kê điều hành | Công thức đếm rõ ràng; không trộn số mẫu
F09 | Phân quyền và địa bàn | Từ chối đọc, ghi ngoài phạm vi
F10 | Ghi nhật ký thao tác | Xác định đúng người, đối tượng và hành động
F11 | Phát hiện trùng và bất thường | Có dữ liệu gán nhãn, lý do và chỉ số đánh giá
F12 | Phân loại và trợ lý thủ tục | Có cơ chế kiểm tra gợi ý và nguồn trả lời
@end
### 2.1.3. Yêu cầu phi chức năng
Hệ thống cần xử lý lỗi đầu vào bằng thông báo rõ ràng, không để các ràng buộc cơ sở dữ liệu trở thành thông báo kỹ thuật khó hiểu với cán bộ. Danh sách phải hỗ trợ tìm kiếm và phân trang; người dùng cần thấy tổng số kết quả và điều kiện lọc đang áp dụng. Mỗi thao tác ghi phải giữ được trạng thái nhất quán hoặc báo thất bại để người dùng biết cần thực hiện lại.
Đối với dữ liệu, cần duy trì Unicode tiếng Việt, lựa chọn thống nhất cách lưu thời gian và bảo toàn số tiền bằng kiểu số thập phân. Đối với quyền, mọi thao tác phải được kiểm tra ở máy chủ và sau khi thu hồi phân công. Đối với vận hành, cấu hình kết nối và khóa dịch vụ phải được tách khỏi tài liệu bàn giao công khai; dữ liệu thử và dữ liệu sử dụng chính thức cần được phân biệt.
## 2.2. Kiến trúc tổng thể
### 2.2.1. Các thành phần và đường đi dữ liệu
Kiến trúc trong Hình 2.1 biểu diễn các thành phần có trong dự án. Trình duyệt tương tác với ứng dụng Blazor. Ứng dụng gọi các dịch vụ nghiệp vụ; các chức năng quản trị có lớp gọi API theo cấu hình. Lớp hạ tầng ánh xạ dữ liệu bằng EF Core. Cơ chế phiên làm việc sử dụng Redis; chức năng trợ lý có điểm tích hợp dịch vụ sinh nội dung bên ngoài. Mũi tên biểu thị luồng tương tác, không khẳng định các thành phần đã được triển khai trên các máy riêng biệt.
@figure architecture.png | Hình 2.1. Kiến trúc thành phần của nền tảng Tân An
Mô hình tách hai trách nhiệm: dữ liệu nghiệp vụ được lưu dài hạn trong cơ sở dữ liệu quan hệ, còn trạng thái phiên đăng nhập được quản lý bởi cơ chế phiên. Việc mất kết nối một thành phần phải được phản ánh bằng lỗi phù hợp, thay vì tự chuyển sang một tài khoản hoặc tập dữ liệu mẫu để che giấu lỗi vận hành.
### 2.2.2. Thiết kế dịch vụ và kết quả trả về
Các dịch vụ được chia theo nhóm nghiệp vụ như PopulationService, WelfareService, CitizenRequestService, DashboardService, AuditLogService và AIService. Dịch vụ nhận dữ liệu đầu vào đã có cấu trúc, thực hiện kiểm tra, thao tác cơ sở dữ liệu và trả kết quả. Với danh sách, kết quả cần gồm các phần tử của trang, tổng số phần tử, chỉ số trang và kích thước trang.
Một lỗi nghiệp vụ, chẳng hạn mã hộ đã tồn tại, cần được phân biệt với lỗi không tìm thấy bản ghi và lỗi thiếu quyền. API cũng cần duy trì cách ánh xạ nhất quán giữa loại lỗi và mã HTTP. Phiên bản hiện tại có nhiều kiểu kết quả dùng chung; việc thống nhất hợp đồng lỗi là một hạng mục bảo trì khi mở rộng tích hợp.
## 2.3. Thiết kế cơ sở dữ liệu
### 2.3.1. Nhóm dữ liệu dân cư và địa bàn
Mô hình địa bàn liên kết đơn vị quản lý, xã và thôn với hộ gia đình. Hộ được liên kết qua khóa ngoại; tên thôn là dữ liệu hiển thị, không nên là khóa nối chính. Cách này giúp việc đổi tên đơn vị không làm mất quan hệ giữa hộ và địa bàn. Hình 2.2 trình bày phần lõi dân cư và lịch sử.
@figure erd_population.png | Hình 2.2. Quan hệ dữ liệu dân cư và lịch sử thành viên
@table Bảng 2.3. Các thực thể chính của nhóm dân cư
Thực thể | Thuộc tính tiêu biểu | Ý nghĩa
ApThon | Id, Ma, Ten, GroupId, XaId | Danh mục địa bàn và liên kết quản lý
HoGiaDinh | Id, MaSoHo, ApThonId, DiaChi | Thông tin hộ và địa bàn hiện tại
NhanKhau | Id, HoTen, CCCD, NgaySinh, MaHoGiaDinh | Cá nhân và liên kết hộ hiện tại
ThanhVienHo | HoGiaDinhId, NhanKhauId, TuNgay, DenNgay, LaChuHo | Lịch sử thành viên và chủ hộ
BienDongDanCu | NhanKhauId, LoaiBienDong, NgayPhatSinh | Sự kiện liên quan nhân khẩu
PhuTrachThon | UserId, ApThonId | Phân công người dùng theo thôn
@end
Ngày kết thúc rỗng trong ThanhVienHo biểu thị giai đoạn hiện hành. Cần ngăn một người có hai giai đoạn mở và ngăn một hộ có hai chủ hộ hiện hành. Ràng buộc duy nhất có điều kiện hỗ trợ kiểm tra các trường hợp này, nhưng không tự bảo đảm mọi hộ đều luôn có chủ hộ; điều kiện tồn tại chủ hộ phải được kiểm soát trong luồng tạo và điều chỉnh hộ.
### 2.3.2. Nhóm dữ liệu an sinh và hồ sơ
PhanLoaiHo lưu phân loại theo hộ; DoiTuongAnSinh lưu trường hợp hưởng hoặc được theo dõi theo nhân khẩu; LichSuTroCap lưu từng lần chi trả. Các bảng này tránh việc gộp một khoản chi với thông tin đối tượng rồi làm mất lịch sử khi đổi mức trợ cấp. Đối với hồ sơ, YeuCauNguoiDan là đối tượng trung tâm; lịch sử xử lý và tệp đính kèm là các bảng phụ thuộc.
@figure erd_welfare.png | Hình 2.3. Quan hệ dữ liệu an sinh và hồ sơ người dân
@table Bảng 2.4. Các thực thể chính của nhóm an sinh và hồ sơ
Thực thể | Thuộc tính tiêu biểu | Quan hệ và vai trò
PhanLoaiHo | HoGiaDinhId, TuNgay, DenNgay | Phân loại hộ theo giai đoạn
DoiTuongAnSinh | NhanKhauId, LoaiDoiTuong, MucTroCapHangThang | Đối tượng theo nhân khẩu
LichSuTroCap | DoiTuongAnSinhId, ThangNam, SoTien | Các lần chi trả của đối tượng
YeuCauNguoiDan | MaYeuCau, ApThonId, NoiDung, TrangThai | Hồ sơ và địa bàn tiếp nhận
LichSuXuLyHoSo | YeuCauId, thông tin xử lý | Diễn biến nghiệp vụ của hồ sơ
TepDinhKem | YeuCauId, TenTep, DuongDanLuu, LoaiTep | Siêu dữ liệu tệp liên quan
ThongBaoThon | NguoiNhanId, ApThonId, TieuDe | Thông báo đến cán bộ được phân công
@end
Tệp đính kèm nên được quản lý bằng siêu dữ liệu và đường dẫn hoặc mã lưu trữ, thay vì mặc định ghi toàn bộ nội dung tệp vào từng bản ghi hồ sơ. Tên tệp do người dùng gửi không phải là bằng chứng về loại nội dung. Luồng tải lên hoàn chỉnh cần kiểm tra dung lượng, loại tệp và quyền tải xuống; mô hình bảng chỉ mới giải quyết phần lưu quan hệ.
### 2.3.3. Ràng buộc và tính toàn vẹn
@table Bảng 2.5. Ràng buộc dữ liệu cần duy trì
Đối tượng | Điều kiện | Mục đích
Mã hộ, mã hồ sơ | Duy nhất | Không lẫn hai đối tượng khi tra cứu
Định danh không rỗng | Không trùng trong phạm vi mô hình | Ngăn lưu lặp định danh
Thành viên hộ | Một giai đoạn mở cho mỗi người | Xác định hộ hiện hành
Chủ hộ | Tối đa một chủ hộ hiện hành mỗi hộ | Tránh mâu thuẫn vai trò
Ngày lịch sử | Ngày kết thúc không trước ngày bắt đầu | Bảo toàn thứ tự thời gian
Trợ cấp | Mức hưởng không âm; khoản chi dương | Ngăn giá trị tiền không hợp lệ
Khóa ngoại | Bản ghi được tham chiếu phải tồn tại | Tránh dữ liệu mồ côi
Xóa dữ liệu cha | Hạn chế khi còn lịch sử hoặc dữ liệu con | Bảo toàn khả năng truy vết
@end
Các kiểm tra về định dạng và quy tắc thời gian phức tạp cần thực hiện thêm trong dịch vụ. Ví dụ, hai giai đoạn đã đóng vẫn có thể chồng lấn dù không vi phạm quy tắc chỉ có một giai đoạn mở. Trước khi thêm một giai đoạn mới, cần so sánh khoảng hiệu lực với các giai đoạn của cùng nhân khẩu. Đối với lịch sử có độ chính xác đến ngày, cần quy ước rõ ngày kết thúc là bao gồm hay không bao gồm ngày đó.
## 2.4. Thiết kế các luồng nghiệp vụ chính
### 2.4.1. Tạo hộ gia đình và xác lập chủ hộ
Luồng bắt đầu khi cán bộ có quyền nhập mã hộ, địa bàn, địa chỉ và thông tin người làm chủ hộ. Hệ thống kiểm tra mã hộ chưa tồn tại, địa bàn hợp lệ, ngày sinh và dữ liệu bắt buộc. Sau đó tạo hộ, tạo nhân khẩu chủ hộ và tạo giai đoạn thành viên tương ứng. Các bản ghi liên quan phải được lưu nhất quán trước khi thông báo thành công.
Trong trường hợp tên địa bàn không nằm trong danh mục, hệ thống cần yêu cầu người dùng chọn lại thay vì tạo một giá trị tự do. Nếu lưu thất bại do trùng mã hoặc trùng định danh, giao diện phải giữ các trường đã nhập để cán bộ sửa. Người dùng không nên phải nhập lại toàn bộ hồ sơ chỉ vì một trường không hợp lệ.
### 2.4.2. Thay đổi chủ hộ
Thay chủ hộ là nghiệp vụ thay vai trò của thành viên trong cùng hộ. Điều kiện trước là người được chọn thuộc hộ đang xử lý và cán bộ có quyền đối với hộ. Hệ thống đóng giai đoạn vai trò cũ, cập nhật quan hệ hiển thị và mở giai đoạn mới. Nếu một bước thất bại, giao dịch phải được hủy để tránh lưu một nửa thay đổi.
@figure head_change.png | Hình 2.4. Luồng xử lý thay đổi chủ hộ
Trong Hình 2.4, bước xác nhận không chỉ là thao tác giao diện. Máy chủ vẫn phải kiểm tra lại quan hệ hộ và quyền khi nhận yêu cầu. Ngày hiệu lực cần được kiểm soát để tránh đóng một giai đoạn bằng thời điểm sớm hơn ngày bắt đầu. Đối với yêu cầu chọn lại chính chủ hộ hiện tại, có thể trả về kết quả không thay đổi để tránh tạo lịch sử thừa.
### 2.4.3. Ghi nhận biến động nhân khẩu
Mỗi loại biến động cần một tập điều kiện và hành động tương ứng. Chuyển đi có thể thay trạng thái cư trú; chuyển đến có thể cần xác lập liên kết hộ; khai tử cần được xét cùng các quan hệ an sinh đang hoạt động. Thiết kế phải xác định rõ thay đổi nào chỉ ghi sự kiện và thay đổi nào có hiệu lực lên dữ liệu hiện hành.
Để dễ kiểm thử, một ca biến động nên gồm thông tin trước xử lý, sự kiện đầu vào, điều kiện hợp lệ, dữ liệu được ghi và trạng thái sau xử lý. Các thông tin liên quan đến quyết định hành chính hoặc giấy tờ ngoài hệ thống được biểu diễn như dữ liệu tham chiếu trong phạm vi mô phỏng; không suy diễn rằng phần mềm tự có thẩm quyền phê duyệt.
### 2.4.4. Tiếp nhận và xử lý yêu cầu
Khi tiếp nhận, hệ thống kiểm tra địa bàn, tạo mã hồ sơ, lưu thông tin người gửi, nội dung và trạng thái mới tiếp nhận. Sau đó thông báo được chuyển đến cán bộ được phân công địa bàn. Khi xử lý, cán bộ xem hồ sơ, ghi nội dung phản hồi và lựa chọn chuyển trạng thái theo quyền. Thiết kế mục tiêu cần lưu từng lần chuyển trạng thái cùng người thực hiện.
@table Bảng 2.6. Luồng trạng thái hồ sơ ở mức thiết kế
Trạng thái | Sự kiện chuyển | Yêu cầu kiểm soát
Mới tiếp nhận | Phân công hoặc bắt đầu xử lý | Cán bộ có quyền đối với địa bàn
Đang xử lý | Bổ sung thông tin hoặc hoàn tất xem xét | Ghi rõ nội dung và người thực hiện
Đã phê duyệt | Kết thúc xử lý trong mô phỏng | Lưu thời gian, kết quả và lịch sử
Từ chối | Kết luận không đáp ứng điều kiện | Có lý do để người gửi theo dõi
@end
Bảng 2.6 là mô hình luồng mục tiêu. Khi hiện thực, các chuyển trạng thái phải được kiểm tra bằng bảng điều kiện; không chấp nhận mọi giá trị trạng thái chỉ vì chúng tồn tại trong enum. Trường hợp cần bổ sung thông tin có thể được biểu diễn bằng ghi chú hoặc mở rộng trạng thái sau khi thống nhất yêu cầu nghiệp vụ.
### 2.4.5. Theo dõi an sinh và chi trả
Luồng quản lý an sinh bắt đầu từ việc xác định đúng đối tượng, loại chính sách mô phỏng và ngày bắt đầu theo dõi. Mỗi lần ghi chi trả phải liên kết đối tượng, kỳ chi, số tiền và người thực hiện. Việc cho phép nhiều đợt trong cùng tháng đòi hỏi có mã giao dịch hoặc cơ chế nhận diện yêu cầu lặp; chỉ so sánh tháng là chưa đủ để phân biệt chi bổ sung với nhập trùng.
Khi một đối tượng ngừng hoạt động, lịch sử cũ vẫn cần được giữ. Tác vụ tổng hợp phải làm rõ đang thống kê đối tượng còn hoạt động hay toàn bộ đối tượng đã từng được theo dõi. Tương tự, tổng số tiền đã ghi chi không phải là tổng mức trợ cấp tháng nhân với số đối tượng nếu có thay đổi kỳ hưởng hoặc nhiều đợt chi.
## 2.5. Thiết kế API và giao diện
### 2.5.1. API nghiệp vụ
Các tuyến trong Bảng 2.7 được đối chiếu với controller của phiên bản mã nguồn. Chúng mô tả hợp đồng HTTP hiện có, chưa chứng minh mọi tuyến đều đã được kiểm thử qua máy chủ. Ngoài nhóm này, dự án có API quản trị riêng cho danh mục, người dùng, vai trò và menu.
@table Bảng 2.7. Một số API nghiệp vụ hiện có
Phương thức | Đường dẫn | Chức năng
GET | /api/v1/HoGiaDinh | Tìm kiếm và phân trang hộ
GET | /api/v1/HoGiaDinh/{id} | Xem chi tiết hộ
POST | /api/v1/HoGiaDinh | Tạo hộ gia đình
GET, POST | /api/v1/NhanKhau | Danh sách và tạo nhân khẩu
PUT | /api/v1/NhanKhau/{id} | Cập nhật thông tin nhân khẩu
GET, POST | /api/v1/BienDong | Tra cứu và ghi biến động
GET, POST | /api/v1/Welfare | Danh sách và tạo đối tượng an sinh
POST | /api/v1/Welfare/tro-cap | Ghi nhận chi trả
GET, POST | /api/v1/CitizenRequest | Tra cứu và tạo yêu cầu
PUT | /api/v1/CitizenRequest/status | Cập nhật trạng thái yêu cầu
GET | /api/v1/Dashboard/overview | Lấy thông tin tổng hợp
GET | /api/v1/AuditLog | Tra cứu nhật ký
@end
Các tham số phân trang cần được kiểm tra trước khi truy vấn: chỉ số trang dương, kích thước trang trong giới hạn cấu hình và thứ tự sắp xếp ổn định. Kết quả không tìm thấy trong danh sách là tập rỗng, khác với việc không tìm thấy một bản ghi theo mã cụ thể. Khi xuất dữ liệu, phạm vi truy cập phải giống với phạm vi người dùng được phép xem trên màn hình.
### 2.5.2. Tổ chức giao diện
Giao diện được tổ chức quanh bàn làm việc và menu chức năng. Các danh sách nên có ô tìm kiếm, bộ lọc địa bàn hoặc loại đối tượng, phân trang và hành động theo từng dòng. Màn hình chi tiết hộ kết hợp thông tin chung và danh sách thành viên; thao tác thay chủ hộ cần được tách khỏi việc sửa văn bản họ tên để người dùng hiểu đúng ý nghĩa nghiệp vụ.
Thông báo thành công chỉ xuất hiện sau khi máy chủ xác nhận lưu. Với thao tác có ảnh hưởng lớn đến lịch sử hoặc trạng thái, giao diện cần trình bày tên đối tượng và thay đổi dự kiến trước khi người dùng thực hiện. Những lỗi nghiệp vụ nên được đặt gần trường liên quan; các lỗi kết nối cần giữ dữ liệu đang nhập và cho phép thử lại.
### 2.5.3. Truy vấn và phân trang
Truy vấn danh sách nên lọc trước khi đếm và phân trang, đồng thời chỉ chọn những thuộc tính cần hiển thị. Tài liệu EF Core nhấn mạnh việc giới hạn dữ liệu trả về, chọn cột phù hợp và đánh giá tác động của chỉ mục đối với truy vấn [6]. Trong đề tài, đây là cơ sở để dùng phép chiếu sang DTO và tránh tải toàn bộ danh sách trước khi chia trang tại giao diện.
Thứ tự sắp xếp phải có tiêu chí phân biệt ổn định khi nhiều bản ghi có cùng ngày tạo hoặc họ tên. Có thể dùng Id làm khóa phụ cho việc sắp xếp. Đối với các trang rất sâu, cần đánh giá phương án phân trang theo khóa thay vì mặc định giữ cách bỏ qua nhiều bản ghi; lựa chọn cuối cùng phải dựa trên phép đo với quy mô dữ liệu dự kiến.
## 2.6. Thiết kế các chức năng AI hỗ trợ
### 2.6.1. Phát hiện hồ sơ có khả năng trùng lặp
Phương án đề xuất gồm chuẩn hóa dữ liệu, tạo tập ứng viên và chấm điểm tương đồng. Chuẩn hóa giữ nguyên bản gốc để hiển thị nhưng tạo thêm biểu diễn phục vụ so sánh, như viết thường, chuẩn hóa khoảng trắng và tách thành phần ngày sinh. Không nên xóa dấu tiếng Việt khỏi dữ liệu gốc vì điều đó làm mất thông tin tên.
Tập ứng viên có thể được giới hạn theo năm sinh hoặc thành phần tên để giảm số cặp phải so sánh. Điểm tương đồng kết hợp thông tin họ tên, ngày sinh, địa chỉ và định danh khi có. Định danh chính xác là tín hiệu mạnh nhưng không được dùng để khẳng định dữ liệu luôn đúng. Ngưỡng phân loại cần được chọn trên bộ phát triển và kiểm tra trên tập độc lập; các cặp nằm gần ngưỡng được đưa vào danh sách chờ cán bộ xem xét.
### 2.6.2. Phát hiện dữ liệu bất thường
Giai đoạn đầu có thể dùng luật minh bạch: ngày sinh nằm sau ngày kiểm tra, sự kiện trước ngày sinh, khoản chi không dương, giai đoạn kết thúc trước khi bắt đầu hoặc quan hệ hộ không tồn tại. Mỗi cảnh báo cần có mã luật, đối tượng, lý do và trạng thái xử lý. Một bản ghi bị cảnh báo chưa đồng nghĩa với một sai phạm; cán bộ cần kiểm tra dữ liệu gốc trong phạm vi nghiệp vụ.
Phương pháp học máy phát hiện bất thường có thể được bổ sung khi đã có bộ dữ liệu và mục tiêu rõ ràng. Việc đưa một mô hình vào hệ thống chỉ có ý nghĩa khi so sánh được với đường cơ sở dùng luật, đo được tỷ lệ cảnh báo sai và giải thích được tác động đến công việc. Thiết kế ưu tiên cảnh báo có thể kiểm tra trước khi tối ưu số lượng cảnh báo.
### 2.6.3. Phân loại yêu cầu và trợ lý tra cứu
Phân loại yêu cầu cần một tập nhãn phù hợp với nhóm hồ sơ được tiếp nhận. Mỗi bản ghi huấn luyện hoặc kiểm tra gồm nội dung, nhãn và nguyên tắc gán nhãn. Yêu cầu ngoài phạm vi, quá ngắn hoặc chứa nhiều nội dung phải có cách xử lý riêng; không bắt buộc mọi câu đều được gán một nhãn với độ tin cậy cao.
Đối với trợ lý tra cứu, phương án hoàn thiện là quản lý tập tài liệu thủ tục có phiên bản, ngày hiệu lực và nguồn; truy xuất nội dung phù hợp rồi tạo câu trả lời gắn với tài liệu đó. Nếu không tìm thấy căn cứ, trợ lý cần thông báo chưa đủ thông tin và hướng người dùng đến cán bộ phụ trách. Đây là thiết kế mục tiêu; lời gọi mô hình sinh văn bản hiện có chưa thực hiện đầy đủ quy trình truy xuất và kiểm chứng nguồn này.
### 2.6.4. Chỉ số đánh giá AI
Với bài toán phát hiện trùng lặp, TP là số cặp trùng được phát hiện đúng, FP là số cặp không trùng bị cảnh báo và FN là số cặp trùng bị bỏ sót. Precision đo độ đúng của các cảnh báo; Recall đo khả năng tìm đủ cặp trùng; F1 cân bằng hai đại lượng. Khi mẫu số bằng không, cần ghi rõ cách xử lý trong chương trình đánh giá thay vì trả về một giá trị tùy ý.
@equation Precision = TP / (TP + FP)                                       (2.1)
@equation Recall = TP / (TP + FN)                                           (2.2)
@equation F1 = 2 × Precision × Recall / (Precision + Recall)          (2.3)
Với phân loại nhiều nhóm, cần báo cáo kết quả theo từng nhãn và trung bình giữa các nhãn, bởi tỷ lệ đúng chung có thể che khuất nhóm ít mẫu. Với trợ lý, cần đánh giá độ đúng theo nguồn, mức đầy đủ, khả năng từ chối câu hỏi ngoài phạm vi và việc không tạo ra thủ tục không có căn cứ. Những chỉ số này mới là cơ sở xác định chất lượng; trường ConfidenceScore trong một phản hồi riêng lẻ không phải độ chính xác của toàn mô hình.
## 2.7. Thiết kế kiểm thử và kết luận chương
Kiểm thử được chia thành ràng buộc dữ liệu, dịch vụ nghiệp vụ, quyền truy cập, HTTP/giao diện, hiệu năng và AI. Mỗi ca kiểm tra phải mô tả trạng thái ban đầu, thao tác, kết quả mong đợi và bằng chứng kết quả thực tế. Bộ dữ liệu thử cần có thể tạo lại; tên, mã hộ và định danh thử phải được phân biệt với dữ liệu thực.
Thiết kế đã xác lập các đối tượng và quan hệ chính, đồng thời chỉ ra những điều kiện không thể suy ra từ việc có một bảng hoặc một màn hình. Chương 3 sẽ đối chiếu thiết kế này với mã nguồn, các công cụ kiểm tra và kết quả thực nghiệm ở thời điểm đánh giá.
# CHƯƠNG 3\nXÂY DỰNG, KIỂM THỬ VÀ ĐÁNH GIÁ HỆ THỐNG
~Chương này trình bày phiên bản triển khai được đánh giá tại ngày 28/09/2026. Cấu trúc dự án, công nghệ và các dịch vụ được đối chiếu trực tiếp với mã nguồn. Các thử nghiệm độc lập tập trung vào tính toàn vẹn dữ liệu, khả năng tạo dữ liệu lặp lại và giới hạn truy cập theo thôn. Kết quả được công bố cùng môi trường và giới hạn của phép thử. Những phần AI chưa hoàn chỉnh và các điểm cần sửa trong thống kê, lịch sử nghiệp vụ được phân tích riêng. Qua đó, chương xác định mức độ đáp ứng hiện tại và thứ tự hoàn thiện trước khi nghiệm thu toàn hệ thống.
## 3.1. Môi trường và cấu trúc triển khai
### 3.1.1. Công nghệ sử dụng
Các dự án giao diện và hạ tầng hiện có đặt TargetFramework là net10.0. Giao diện sử dụng Blazor, bộ thành phần Fluent UI và thư viện biểu đồ Blazor-ApexCharts. Hạ tầng khai báo Entity Framework Core 9.0.2, các trình cung cấp PostgreSQL/SQLite và thư viện kết nối Redis. Bảng 3.1 ghi phiên bản được khai báo trong dự án, không suy diễn đó là phiên bản mới nhất hoặc là kết quả xác nhận mọi gói đều tương thích trong mọi môi trường [3].
@table Bảng 3.1. Công nghệ được khai báo trong phiên bản đánh giá
Thành phần | Công nghệ | Vai trò
Nền tảng ứng dụng | .NET 10, ASP.NET Core | Chạy giao diện và API
Giao diện | Blazor Interactive Server | Xử lý tương tác phía máy chủ
Thành phần giao diện | Fluent UI 4.11.5 | Các điều khiển và bố cục
Biểu đồ | Blazor-ApexCharts 3.5.0 | Biểu diễn chỉ số tổng hợp
Ánh xạ dữ liệu | Entity Framework Core 9.0.2 | Truy vấn và lưu thực thể
Cơ sở dữ liệu | Npgsql EF Core 9.0.2; SQLite provider 9.0.2 | Kết nối PostgreSQL và thử nghiệm SQLite
Phiên làm việc | StackExchange.Redis 2.8.22 | Hỗ trợ lưu và kiểm tra phiên
Xuất dữ liệu | EPPlus 8.7.1 | Tạo tệp bảng tính
@end
README và một số tài liệu cài đặt trong dự án còn mô tả kiến trúc React/Python trước đó. Phiên bản được đánh giá trong chương này căn cứ vào các tệp dự án và mã hiện hành. Việc đồng bộ tài liệu cài đặt với cấu trúc thực tế là cần thiết để người nhận mã nguồn có thể dựng lại môi trường đúng cách.
### 3.1.2. Tổ chức mã nguồn
@table Bảng 3.2. Các thành phần mã nguồn phục vụ triển khai
Thành phần | Trách nhiệm | Ví dụ nội dung
Service.TanAn.Domain | Mô hình miền | Hộ, nhân khẩu, biến động, an sinh, hồ sơ
Service.TanAn.Application | Dịch vụ nghiệp vụ | PopulationService, WelfareService, AIService
Service.TanAn.Infrastructure | Lưu trữ và hạ tầng | TanAnDbContext, cấu hình, migrations
Service.TanAn.API | Giao tiếp HTTP | Controller nghiệp vụ và quản trị
Service.UI.CMS.Blazor | Giao diện sử dụng | Bàn làm việc, danh sách, biểu mẫu
Service.Shared.Contracts | Dữ liệu trao đổi | DTO và biểu mẫu yêu cầu
Service.Shared.Commons | Thành phần dùng chung | Kết quả, phân trang, hỗ trợ xác thực
Tools | Công cụ kiểm tra và dữ liệu thử | DemoPopulation, VillageScopeChecks
@end
Cấu hình khởi động của giao diện đăng ký chế độ Interactive Server và cơ chế xác thực cookie. Phần quản trị có ApiServiceTransport gửi yêu cầu đến địa chỉ API được cấu hình và truyền mã phiên theo cơ chế TanAnSession. Lớp vận chuyển kiểm tra đường dẫn trước khi gọi và chuyển lỗi kết nối thành thông báo có cấu trúc. Các nghiệp vụ còn lại cần được rà soát theo từng luồng thay vì giả định tất cả đều dùng cùng một cơ chế vận chuyển.
## 3.2. Hiện thực các phân hệ nghiệp vụ
### 3.2.1. Hộ gia đình và nhân khẩu
PopulationService cung cấp tìm kiếm, lấy chi tiết, tạo hộ, tạo và cập nhật nhân khẩu. Danh sách hộ hỗ trợ từ khóa, địa bàn và mã đơn vị; truy vấn sử dụng AsNoTracking, đếm kết quả trước khi phân trang và chọn dữ liệu cần thiết vào DTO. Danh sách nhân khẩu sắp xếp theo họ tên rồi Id để có thứ tự ổn định khi nhiều người trùng tên.
Luồng tạo hộ đã kiểm tra mã hộ trùng, ngày sinh chủ hộ và địa bàn hoạt động. Hệ thống tạo đồng thời bản ghi hộ, nhân khẩu chủ hộ và giai đoạn thành viên. Luồng thêm thành viên từ chối việc tự đặt vai trò chủ hộ và hướng sang chức năng chọn chủ hộ. Cách xử lý này phân biệt rõ thêm người với thay đổi vai trò trong hộ.
SetChuHoAsync sử dụng giao dịch, kiểm tra người được chọn thuộc hộ, đóng giai đoạn cũ và tạo giai đoạn mới. Luồng đồng thời cập nhật tên và định danh chủ hộ trên bản ghi hộ. Đây là điểm đã được đưa vào công cụ kiểm tra phạm vi thôn. Tuy nhiên, việc quản lý lịch sử theo ngày vẫn cần quy ước nhất quán về ranh giới ngày và các trường hợp thay đổi nhiều lần trong cùng ngày.
### 3.2.2. Biến động dân cư
Dịch vụ hiện có chức năng tra cứu và tạo sự kiện biến động, lưu loại, nhân khẩu, ngày phát sinh, nơi đến hoặc đi, lý do và người ghi nhận. Trong phương thức tạo được kiểm tra, nhánh tạm vắng và chuyển đi có cập nhật trạng thái nhân khẩu tương ứng. Các loại còn lại có thể được ghi thành sự kiện, nhưng chưa có đủ logic đồng bộ trạng thái và các quan hệ liên quan trong cùng phương thức.
Do đó, cần phân biệt chức năng lưu sự kiện với quy trình hoàn chỉnh cho từng loại biến động. Trước nghiệm thu, cần bổ sung ca thử cho khai sinh, khai tử, chuyển đến và tạm trú; đối chiếu trạng thái trước/sau cùng lịch sử thành viên và các đối tượng an sinh liên quan. Không nên đánh giá một nghiệp vụ đã hoàn tất chỉ vì loại đó xuất hiện trong danh sách chọn.
### 3.2.3. An sinh xã hội
WelfareService đã có truy vấn theo từ khóa và loại đối tượng, tạo đối tượng an sinh và ghi lịch sử trợ cấp. Thông tin hiển thị được lấy qua liên kết đến nhân khẩu và hộ; kết quả trả về bao gồm mức trợ cấp, ngày bắt đầu, trạng thái và ghi chú. Nhật ký được gọi sau các thao tác tạo đối tượng hoặc ghi chi trả.
Mô hình có PhanLoaiHo để biểu diễn phân loại cấp hộ, trong khi một số luồng hiện tại vẫn dùng các giá trị loại đối tượng cũ cho hộ nghèo/cận nghèo. Cần thống nhất nguồn dữ liệu trước khi công bố số hộ thuộc từng nhóm. Việc chuyển dữ liệu phải được thiết kế và kiểm tra, tránh đếm số người trong danh sách đối tượng như số hộ nghèo.
Đối với chi trả, mã nguồn đã lưu kỳ, số tiền và người ghi nhận nhưng chưa chứng minh có cơ chế chống gửi lặp giao dịch. Hướng hoàn thiện là sử dụng mã giao dịch hoặc khóa xử lý lặp và kiểm tra trạng thái đối tượng tại thời điểm chi. Các ràng buộc số tiền ở cơ sở dữ liệu bảo vệ một phần dữ liệu, không thay thế toàn bộ điều kiện nghiệp vụ.
### 3.2.4. Hồ sơ và thông báo theo địa bàn
CitizenRequestService kiểm tra thôn tiếp nhận trước khi tạo hồ sơ. Mã hồ sơ được kết hợp từ thời gian và một phần mã ngẫu nhiên; ràng buộc duy nhất tại cơ sở dữ liệu vẫn cần được giữ. Hệ thống tạo thông báo cho các cán bộ thôn đã được phân công và có trạng thái tài khoản được duyệt. Chức năng tìm kiếm hỗ trợ mã hồ sơ, tên người gửi, định danh và loại yêu cầu.
Phương thức cập nhật trạng thái hiện thay đổi trạng thái, ghi chú, cán bộ xử lý và thời điểm cập nhật, sau đó gọi nhật ký. Trong luồng được kiểm tra chưa có bước ghi tương ứng vào LichSuXuLyHoSo. Bởi vậy, nhật ký thao tác hiện có chưa đồng nghĩa với việc lịch sử nghiệp vụ của từng hồ sơ đã được hiện thực đầy đủ.
Ngoài lịch sử, cần kiểm tra tính hợp lệ của từng chuyển trạng thái và lấy người xử lý từ danh tính máy chủ. Các trường do phía gửi truyền lên chỉ nên chứa nội dung nghiệp vụ được phép sửa; danh tính cán bộ phải được xác định độc lập để bảo toàn khả năng truy vết.
### 3.2.5. Dashboard và báo cáo thống kê
DashboardService đã truy vấn số hộ, số bản ghi nhân khẩu, giới tính, đối tượng an sinh, trạng thái hồ sơ và nhóm biến động. Tuy nhiên, một số nhánh hiện trả về số mặc định khi truy vấn không có dữ liệu: chẳng hạn danh sách biến động mẫu, số hộ nghèo/cận nghèo hoặc số hồ sơ đã duyệt. Những giá trị này phục vụ hiển thị mẫu và không thể dùng làm số liệu thực nghiệm của đề tài.
Trước khi nghiệm thu thống kê, cần loại bỏ hoặc tách rõ chế độ dữ liệu mẫu; trường hợp không có dữ liệu phải trả về số 0 hoặc trạng thái chưa có dữ liệu theo định nghĩa chỉ số. Cần bổ sung bộ lọc kỳ báo cáo và quy định loại nhân khẩu được tính. Một chỉ số mang tên theo tháng phải thực sự áp dụng điều kiện thời gian, không chỉ nhóm tất cả sự kiện theo loại.
## 3.3. Phân quyền, phiên làm việc và truy vết
### 3.3.1. Phân vùng dữ liệu theo thôn
TanAnDbContext.Scope.cs áp dụng bộ lọc cho các nhóm dữ liệu dân cư, an sinh và hồ sơ theo người dùng và thôn được giao. Quyền cấp xã và quản trị được xét từ tài khoản; cán bộ thôn được giới hạn bởi bảng phân công. Việc kiểm soát không dừng ở đọc: trước khi lưu, mã kiểm tra cả giá trị hiện có trong cơ sở dữ liệu lẫn giá trị dự kiến thay đổi.
Cách kiểm tra hai phía giúp ngăn trường hợp người dùng gắn một đối tượng thuộc thôn khác rồi đổi địa bàn sang thôn mình để vượt quyền. Khi phân công bị thu hồi, các truy vấn và lần lưu tiếp theo phải bị từ chối theo phạm vi mới. Kết quả công cụ VillageScopeChecks tại mục 3.5 cung cấp bằng chứng cho các tình huống này trong môi trường SQLite.
### 3.3.2. Phiên và quyền chức năng
Giao diện có cơ chế đăng nhập bằng cookie, kiểm tra phiên và quản lý menu theo tài khoản. API quản trị có luồng sử dụng mã phiên, còn các API cũ có cơ chế xác thực riêng. Điều này đòi hỏi kiểm thử quyền trên từng tuyến và từng cách truy cập, đặc biệt khi người dùng mở trực tiếp URL hoặc gọi HTTP mà không qua giao diện.
Trong chương này, kết quả kiểm thử dịch vụ hoặc DbContext không được dùng để khẳng định toàn bộ API đã an toàn. Các thử nghiệm HTTP cần xác nhận từ chối người chưa đăng nhập, người sai quyền, phiên hết hạn và phiên bị thu hồi. Quyền menu và bộ lọc địa bàn cần hoạt động đồng bộ với danh tính thật của đường gọi đang dùng.
### 3.3.3. Nhật ký thao tác
Các dịch vụ có lời gọi ghi nhật ký ở nhiều thao tác tạo và cập nhật. Một số nhánh dùng dữ liệu người thao tác truyền qua tham số hoặc biểu mẫu. Để nhật ký phục vụ kiểm tra trách nhiệm, cần chuẩn hóa việc lấy danh tính, thời gian và thông tin thay đổi. Cũng cần xem xét trường hợp dữ liệu nghiệp vụ lưu thành công nhưng bước ghi nhật ký thất bại.
Phương án hoàn thiện là đặt dữ liệu nghiệp vụ và nhật ký liên quan trong cùng giao dịch khi cùng dùng một cơ sở dữ liệu, hoặc dùng cơ chế bảo đảm gửi và ghi lại sự kiện nếu hai thành phần tách rời. Việc chọn phương án cần cân đối mức độ phức tạp và yêu cầu truy vết của đồ án.
## 3.4. Trạng thái triển khai các chức năng AI
### 3.4.1. Phát hiện trùng lặp, bất thường và phân loại
Trong AIService, hai phương thức DetectDuplicatesAsync và DetectAnomaliesAsync hiện trả về danh sách rỗng sau một khoảng chờ. Đây là điểm nối chức năng, chưa có thuật toán phân tích dữ liệu. Danh sách cảnh báo rỗng không được diễn giải thành kết luận không có hồ sơ trùng hoặc bất thường trong cơ sở dữ liệu.
ClassifyRequestAsync hiện trả một nhóm yêu cầu cố định và ConfidenceScore bằng 0,95. Giá trị được gán trong mã không phụ thuộc nội dung đầu vào, nên không phải độ chính xác 95% của mô hình. Chức năng cần được thay bằng bộ phân loại có dữ liệu kiểm tra và cơ chế xử lý trường hợp không chắc chắn trước khi được tính là hoàn thành nhiệm vụ AI.
### 3.4.2. Trợ lý tra cứu thủ tục
ChatProcedureAsync có cấu trúc gọi API sinh nội dung và xử lý một số nhánh lỗi. Khi không có khóa cấu hình, phương thức trả lời chào và danh sách gợi ý định sẵn. Khi có cấu hình, nội dung câu hỏi được gửi đến dịch vụ bên ngoài. Trong lần đánh giá này không gọi dịch vụ AI và không sử dụng khóa truy cập của ứng dụng.
Phiên bản hiện có chưa có bước truy xuất tài liệu thủ tục, kiểm tra nguồn hay đánh giá tập câu hỏi chuẩn. Vì thế, kết quả mới xác nhận sự tồn tại của cấu trúc tích hợp trong mã nguồn. Để hoàn thiện, cần cấu hình mô hình còn khả dụng, kiểm tra lỗi dịch vụ, giới hạn dữ liệu gửi đi, bổ sung kho tài liệu có nguồn và thực nghiệm độ đúng của câu trả lời.
## 3.5. Kiểm thử độc lập và kết quả
### 3.5.1. Phạm vi và môi trường thử nghiệm
Các kiểm tra được chạy trên máy Windows ngày 28/09/2026 bằng công cụ .NET có sẵn trong dự án. DemoPopulation được chạy với tham số --self-test, sử dụng SQLite trong bộ nhớ. VillageScopeChecks và EntityModelChecks cũng tạo cơ sở dữ liệu trong bộ nhớ; bước sinh cấu trúc PostgreSQL của EntityModelChecks không mở kết nối đến máy chủ PostgreSQL.
Lệnh chạy dùng --no-restore và BuildProjectReferences=false, tái sử dụng các bản dựng tham chiếu đang có. Vì vậy, kết quả phản ánh các công cụ và bản dựng được sử dụng tại lần chạy, không phải bằng chứng của một quy trình dựng sạch toàn bộ mã nguồn. Nhật ký đầu ra được lưu để truy lại kết quả. Không có phép đo hiệu năng hệ thống hoặc thử nghiệm tải đồng thời trong phạm vi này.
### 3.5.2. Tạo dữ liệu mô phỏng và chạy lặp
Công cụ DemoPopulation ở chế độ tự kiểm tra tạo 11 địa bàn với 3 hộ mỗi địa bàn, tổng cộng 33 hộ, 132 nhân khẩu và 33 chủ hộ. Lần chạy tiếp theo trên cùng cơ sở dữ liệu kiểm tra vẫn giữ 33 hộ và 132 nhân khẩu, số hộ mới thêm bằng 0. Đây là kết quả kiểm tra tính lặp của công cụ, không phải số hộ hay số dân thực tế của xã Tân An.
@table Bảng 3.3. Kết quả kiểm tra dữ liệu mô phỏng
Chỉ tiêu | Lần tạo thứ nhất | Lần tạo lại
Số địa bàn trong bộ thử | 11 | 11
Tổng số hộ mô phỏng | 33 | 33
Tổng số nhân khẩu mô phỏng | 132 | 132
Số chủ hộ | 33 | 33
Số hộ được thêm mới | 33 | 0
Trạng thái công cụ | Hoàn thành | Hoàn thành, không nhân đôi hộ
@end
Công cụ cũng kiểm tra lọc theo đơn vị, truy vấn nhân khẩu, phân trang và đổi tên đơn vị. Với một đơn vị có 3 hộ, trang thứ nhất kích thước 2 trả 2 hộ, trang thứ hai trả 1 hộ và hai trang không trùng nhau. Truy vấn một mã đơn vị không có dữ liệu trả tập rỗng. Sau khi đổi tên đơn vị, quan hệ và số hộ được giữ nhờ liên kết bằng khóa.
### 3.5.3. Kiểm tra quyền theo địa bàn
VillageScopeChecks dựng hai địa bàn và một cán bộ chỉ được phân công một địa bàn. Công cụ thử truy vấn các nhóm dân cư, an sinh và hồ sơ; truy cập trực tiếp mã bản ghi thuộc địa bàn khác; chèn dữ liệu ngoài phạm vi; gắn đối tượng để cập nhật vượt phạm vi; cập nhật dữ liệu của địa bàn được giao; và thu hồi phân công.
@table Bảng 3.4. Kết quả công cụ kiểm tra phạm vi thôn
Tình huống | Kết quả mong đợi | Kết quả chạy
Đọc dân cư, an sinh, hồ sơ | Chỉ thấy địa bàn được giao | Đạt
Tra cứu Id ngoài địa bàn | Không lấy được bản ghi | Đạt
Chèn hộ sang thôn khác | Từ chối ghi | Đạt
Gắn đối tượng ngoài phạm vi rồi sửa | Từ chối cập nhật | Đạt
Cập nhật hộ thuộc địa bàn | Lưu được thay đổi | Đạt
Tạo hộ và chọn lại chủ hộ | Duy trì một chủ hộ hiện hành | Đạt
Chọn chủ hộ từ hộ khác | Từ chối lựa chọn | Đạt
Tạo hồ sơ tại thôn được giao | Có thông báo đến cán bộ | Đạt
Thu hồi phân công | Lần đọc và ghi tiếp theo bị giới hạn | Đạt
@end
Kết quả đạt trong Bảng 3.4 chỉ áp dụng cho các đường xử lý mà công cụ gọi trong môi trường thử. Công cụ không mở trình duyệt, không kiểm tra mọi API và không đo độ trễ giữa các máy. Khi triển khai, cần bổ sung kiểm tra đầu cuối để xác nhận danh tính từ phiên được truyền đúng xuống các cơ chế đã kiểm tra.
### 3.5.4. Ràng buộc dữ liệu và kiểm tra tổng hợp
EntityModelChecks đã đi qua các kiểm tra từ chối định danh trùng, cho phép nhiều người chưa có định danh, bảo toàn dữ liệu con khi xóa hộ, thứ tự ngày lịch sử, giới hạn giai đoạn hiện hành và chủ hộ, khóa ngoại, số tiền trợ cấp âm, mã hồ sơ trùng và bảo toàn tệp khi xóa hồ sơ. Công cụ cũng sinh được câu lệnh tạo cấu trúc PostgreSQL mà không kết nối máy chủ.
Sau các kiểm tra quản trị và trạng thái phê duyệt, chương trình dừng tại ca tạo hộ với thông báo “Nhập ngày sinh hợp lệ của chủ hộ”. Bộ dữ liệu của ca này không cung cấp ngày sinh chủ hộ theo điều kiện mới của dịch vụ. Kết quả toàn bộ công cụ là không đạt, dù các kiểm tra trước điểm dừng đã in thông báo đạt. Cần cập nhật dữ liệu đầu vào của ca thử và chạy lại trước khi kết luận toàn bộ bộ kiểm tra thành công.
@table Bảng 3.5. Tổng hợp trạng thái các công cụ đã chạy
Công cụ | Phạm vi | Kết quả tổng thể
DemoPopulation --self-test | Tạo dữ liệu, lặp lại, lọc, phân trang | Đạt; mã thoát 0
VillageScopeChecks | Đọc, ghi, chủ hộ và thu hồi quyền theo thôn | Đạt; mã thoát 0
EntityModelChecks | Ràng buộc, quản trị và nghiệp vụ tổng hợp | Chưa đạt; dừng ở ca tạo hộ, mã thoát 1
@end
Trong quá trình chạy, bộ công cụ còn phát cảnh báo phụ thuộc NU1903 đối với SQLitePCLRaw.lib.e_sqlite3 2.1.10. Cảnh báo này cần được xử lý bằng kiểm tra bản phụ thuộc và thử lại sau khi cập nhật phù hợp; việc phép thử nghiệp vụ đi qua không xóa bỏ cảnh báo của môi trường xây dựng.
## 3.6. Đánh giá kết quả theo yêu cầu đề tài
### 3.6.1. Các kết quả có cơ sở kiểm chứng
Nhóm kết quả rõ nhất là cấu trúc dữ liệu quan hệ, các dịch vụ dân cư, công cụ dữ liệu tổng hợp và cơ chế giới hạn theo địa bàn. Dữ liệu thử có khả năng tái tạo; các ca truy cập ngoài địa bàn và thu hồi phân công đã được kiểm tra độc lập. Các kết quả này đóng góp vào yêu cầu quản lý nhất quán và phân chia trách nhiệm trong mô hình cấp xã.
Mã nguồn đã có các thành phần an sinh, hồ sơ, dashboard, nhật ký và API, nhưng mức độ kiểm chứng khác nhau giữa các nhóm. Việc đánh giá cần giữ sự khác biệt đó thay vì tổng hợp tất cả thành một tuyên bố đã hoàn thành toàn bộ nền tảng.
@table Bảng 3.6. Đối chiếu yêu cầu và mức độ kiểm chứng
Nhóm yêu cầu | Bằng chứng hiện có | Phần cần hoàn thiện
Hộ và nhân khẩu | Dịch vụ, mô hình, kiểm tra tạo hộ/chủ hộ theo thôn | Kiểm tra HTTP và toàn bộ tình huống chỉnh sửa
Biến động | Lưu sự kiện; cập nhật tạm vắng/chuyển đi | Hoàn chỉnh tác động của từng loại biến động
An sinh | Dịch vụ đối tượng và lịch sử trợ cấp | Thống nhất phân loại cấp hộ, chống giao dịch lặp
Hồ sơ | Tiếp nhận, trạng thái, thông báo | Luật chuyển trạng thái và lịch sử xử lý đầy đủ
Phân quyền | Bộ lọc và kiểm tra ghi theo thôn; công cụ đạt | Kiểm tra đầu cuối trên mọi tuyến API
Dashboard | Dịch vụ tổng hợp và biểu đồ trong dự án | Loại bỏ số mẫu, xác định kỳ và công thức đếm
AI | Giao diện dịch vụ và mã tích hợp trợ lý | Thuật toán, dữ liệu gán nhãn, thực nghiệm
Hiệu năng | Cơ chế lọc và phân trang | Đo p50/p95, tải đồng thời, tài nguyên
@end
### 3.6.2. Hạn chế của phép thử và sản phẩm
SQLite trong bộ nhớ thuận tiện để kiểm tra nhanh nhưng khác PostgreSQL về kiểu dữ liệu, thực thi truy vấn và hành vi đồng thời. Sinh được cấu trúc PostgreSQL chưa chứng minh migration đã chạy thành công trên cơ sở dữ liệu triển khai. Cần kiểm thử trên một bản dữ liệu độc lập của môi trường mục tiêu, đặc biệt với ràng buộc, transaction và các bộ lọc theo quyền.
Việc tái sử dụng bản dựng tham chiếu cũng giới hạn khả năng khẳng định mọi thay đổi mã nguồn đều đã được biên dịch lại. Bước nghiệm thu tiếp theo nên thực hiện dựng sạch trong thư mục đầu ra riêng, ghi lại phiên bản mã nguồn, cấu hình và kết quả. Với AI, chưa có bộ gán nhãn và số liệu đánh giá nên chưa đủ cơ sở kết luận đạt mục tiêu nghiên cứu về độ chính xác.
### 3.6.3. Kế hoạch đánh giá hiệu năng
Phép đo hiệu năng cần xác định số hộ, số nhân khẩu, số hồ sơ, cấu hình máy chủ, phiên bản cơ sở dữ liệu và số người dùng đồng thời. Các tình huống nên gồm tìm hộ theo mã, tìm nhân khẩu theo tên, phân trang, xem thống kê và tạo hồ sơ. Mỗi tình huống phải có các lần khởi động trước, thời gian đo và cách tổng hợp riêng để tránh chỉ lấy một thời điểm thuận lợi.
Các chỉ số dự kiến gồm thời gian phản hồi trung vị p50, phân vị p95, tỷ lệ lỗi và mức sử dụng tài nguyên. Cần so sánh trước/sau khi bổ sung chỉ mục hoặc sửa truy vấn trên cùng điều kiện. Báo cáo hiện không gán các giá trị thời gian hay tỷ lệ lỗi khi chưa thực hiện phép đo này.
## 3.7. Hướng hoàn thiện và kết luận chương
Thứ tự ưu tiên trước hết là đồng bộ dữ liệu kiểm thử với quy tắc nghiệp vụ, dựng sạch và chạy lại các công cụ. Tiếp theo là hoàn thiện trạng thái biến động, lịch sử hồ sơ, thống kê thuần dữ liệu và tính nhất quán của danh tính ghi nhật ký. Sau đó kiểm tra đầu cuối các màn hình/API trên môi trường độc lập và thu thập ảnh minh chứng từ đúng phiên bản chạy.
Đối với AI, cần chọn từng bài toán để hoàn thiện tuần tự: xây dựng bộ dữ liệu kiểm tra, triển khai đường cơ sở, đo chất lượng và tích hợp cơ chế người dùng xác nhận. Việc tích hợp một mô hình trả lời tự do không đủ để đáp ứng yêu cầu trợ lý thủ tục có căn cứ. Khả năng sử dụng thực tế chỉ nên được đánh giá sau khi hoàn thành kiểm thử nghiệp vụ, quyền truy cập và kiểm tra nguồn thông tin.
Kết quả chương cho thấy phiên bản hiện có đã giải quyết được một phần nền tảng dữ liệu và phân quyền, đồng thời còn các hạng mục cụ thể chưa đạt mức nghiệm thu toàn hệ thống. Những hạn chế đã được chỉ ra bằng mã nguồn và kết quả kiểm tra, tạo cơ sở cho kế hoạch hoàn thiện trong thời gian còn lại của đồ án.
# KẾT LUẬN
Đề tài nghiên cứu bài toán tổ chức nền tảng số hỗ trợ quản lý dân cư, biến động, an sinh xã hội và yêu cầu người dân tại xã Tân An trên dữ liệu mô phỏng. Báo cáo đã hệ thống hóa các đối tượng nghiệp vụ, xác lập yêu cầu, xây dựng mô hình dữ liệu và phân tích kiến trúc triển khai bằng Blazor/.NET. Các giải pháp phân vùng dữ liệu, lịch sử thành viên, dịch vụ nghiệp vụ và API được trình bày gắn với cấu trúc mã nguồn.
Về kết quả kiểm chứng, công cụ dữ liệu mô phỏng tạo được bộ thử gồm 33 hộ và 132 nhân khẩu, giữ nguyên số lượng khi chạy lặp, đồng thời đi qua các kiểm tra lọc và phân trang. Công cụ quyền theo thôn đi qua các tình huống đọc, ghi và thu hồi phân công đã thiết kế. Bộ kiểm tra tổng hợp còn dừng ở một ca tạo hộ cần cập nhật dữ liệu kiểm thử, nên chưa thể kết luận toàn bộ hệ thống đã vượt qua kiểm thử.
Các chức năng AI chưa có đủ thuật toán và thực nghiệm để khẳng định hoàn thành. Bên cạnh đó, thống kê còn giá trị mẫu, một số tác động của biến động chưa đầy đủ và lịch sử xử lý hồ sơ cần được nối vào luồng nghiệp vụ. Đây là những công việc cụ thể phải giải quyết trước khi nghiệm thu sản phẩm hoàn chỉnh, cùng với kiểm tra tích hợp, ảnh minh chứng giao diện và đánh giá hiệu năng.
Hướng phát triển của đề tài là hoàn thiện tính đúng của nghiệp vụ trước khi mở rộng chức năng; thống nhất cơ chế quyền và truy vết; triển khai các phép đo có thể tái lập; và bổ sung AI có dữ liệu gán nhãn, căn cứ tra cứu và cơ chế người dùng kiểm soát. Các kết quả trên dữ liệu mô phỏng là cơ sở thử nghiệm phần mềm, không thay thế việc thẩm định nghiệp vụ và điều kiện triển khai tại đơn vị sử dụng.
# TÀI LIỆU THAM KHẢO
## Tiếng Việt
[1] Đào Văn Hiếu, Đề cương đồ án tốt nghiệp kỹ sư về nền tảng số hỗ trợ quản lý biến động dân cư, an sinh xã hội và điều hành tại xã Tân An trên dữ liệu mô phỏng, tài liệu đề cương, Trường Đại học Công nghiệp Việt–Hung, 2026.
[2] Đào Văn Hiếu, Phiếu giao đề tài đồ án tốt nghiệp, mã sinh viên 2200454, tài liệu đề tài, Trường Đại học Công nghiệp Việt–Hung, 2026.
[3] Đào Văn Hiếu, Mã nguồn và tài liệu kỹ thuật dự án Tân An, các thư mục src, Tools và docs, phiên bản đối chiếu tại thư mục làm việc ngày 28/09/2026.
[4] Khoa Công nghệ thông tin, Quy định và hướng dẫn trình bày báo cáo đồ án học phần, thực tập tốt nghiệp, đồ án tốt nghiệp và các biểu mẫu kèm theo, Trường Đại học Công nghiệp Việt–Hung, Hà Nội, 2025.
## Tiếng Anh
[5] Microsoft, ASP.NET Core Blazor render modes, Microsoft Learn, https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0, truy cập ngày 28/09/2026.
[6] Microsoft, Efficient Querying, Entity Framework Core Documentation, https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying, truy cập ngày 28/09/2026.
[7] Microsoft, Transactions, Entity Framework Core Documentation, https://learn.microsoft.com/en-us/ef/core/saving/transactions, truy cập ngày 28/09/2026.
[8] OWASP Foundation, Authorization Cheat Sheet, OWASP Cheat Sheet Series, https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html, truy cập ngày 28/09/2026.
[9] PostgreSQL Global Development Group, Constraints, PostgreSQL 18 Documentation, https://www.postgresql.org/docs/18/ddl-constraints.html, truy cập ngày 28/09/2026.
# PHỤ LỤC A\nTỪ ĐIỂN DỮ LIỆU RÚT GỌN
Phụ lục này liệt kê các trường tiêu biểu của mô hình. Trường Id và khóa ngoại dạng Guid dùng cho liên kết; những mã hiển thị như MaSoHo và MaYeuCau phục vụ tra cứu nghiệp vụ. Tính duy nhất, tính bắt buộc và quy tắc kiểm tra phải được đọc cùng cấu hình thực thể và lớp dịch vụ, không chỉ dựa vào kiểu dữ liệu của thuộc tính.
@table Bảng A.1. Một số trường của hộ và nhân khẩu
Bảng và trường | Kiểu trong mô hình | Ý nghĩa
HoGiaDinh.Id | Guid | Khóa định danh hộ
HoGiaDinh.MaSoHo | string | Mã hộ dùng trong ứng dụng
HoGiaDinh.ApThonId | Guid? | Liên kết đến thôn
HoGiaDinh.TenChuHo | string | Thông tin chủ hộ phục vụ hiển thị
NhanKhau.Id | Guid | Khóa định danh nhân khẩu
NhanKhau.MaHoGiaDinh | Guid | Hộ hiện tại
NhanKhau.HoTen | string | Họ tên của cá nhân mô phỏng
NhanKhau.CCCD | string | Định danh; bộ thử dùng giá trị đánh dấu
NhanKhau.NgaySinh | DateTime | Ngày sinh
NhanKhau.TrangThai | enum | Trạng thái nhân khẩu
ThanhVienHo.TuNgay | DateOnly | Ngày bắt đầu giai đoạn
ThanhVienHo.DenNgay | DateOnly? | Ngày kết thúc; rỗng khi đang mở
ThanhVienHo.LaChuHo | bool | Đánh dấu vai trò chủ hộ
@end
@table Bảng A.2. Một số trường của an sinh và hồ sơ
Bảng và trường | Ý nghĩa | Lưu ý nghiệp vụ
DoiTuongAnSinh.NhanKhauId | Nhân khẩu được theo dõi | Phải tồn tại nhân khẩu
DoiTuongAnSinh.MucTroCapHangThang | Mức trợ cấp mô phỏng | Không phải quy định mức hưởng thực tế
LichSuTroCap.ThangNam | Kỳ chi theo tháng/năm | Cần phân biệt các đợt chi
LichSuTroCap.SoTien | Giá trị khoản chi | Phải dương theo ràng buộc mô hình
YeuCauNguoiDan.MaYeuCau | Mã hồ sơ | Có kiểm soát duy nhất
YeuCauNguoiDan.ApThonId | Địa bàn tiếp nhận | Dùng cho phân công và giới hạn dữ liệu
YeuCauNguoiDan.TrangThai | Trạng thái xử lý | Cần kiểm tra chuyển trạng thái hợp lệ
TepDinhKem.DuongDanLuu | Vị trí lưu tệp | Kiểm tra quyền khi truy cập
ThongBaoThon.NguoiNhanId | Cán bộ nhận thông báo | Liên kết với phân công địa bàn
@end
# PHỤ LỤC B\nDANH SÁCH CA KIỂM THỬ BỔ SUNG
Các ca trong phụ lục là kế hoạch kiểm thử tiếp theo, chưa phải kết quả đã chạy. Khi thực hiện, cần ghi người kiểm tra, phiên bản mã nguồn, dữ liệu đầu vào, thời gian và bằng chứng cho từng ca. Kết quả mong đợi cần được điều chỉnh nếu quy trình nghiệp vụ được giảng viên hoặc đơn vị sử dụng xác nhận khác với thiết kế hiện tại.
@table Bảng B.1. Các ca kiểm thử nghiệp vụ và quyền cần bổ sung
Mã | Tình huống | Kết quả mong đợi
T01 | Tạo hộ thiếu ngày sinh chủ hộ | Từ chối, nêu trường cần sửa
T02 | Tạo hộ đầy đủ thông tin và địa bàn hợp lệ | Có hộ, nhân khẩu và một chủ hộ hiện hành
T03 | Hai yêu cầu đồng thời tạo cùng mã hộ | Chỉ một bản ghi được lưu
T04 | Thay chủ hộ hai lần trong cùng ngày | Lịch sử và vai trò vẫn nhất quán
T05 | Ghi biến động khai tử | Trạng thái và quan hệ liên quan theo quy tắc đã chốt
T06 | Chuyển hộ của nhân khẩu | Hộ hiện tại và lịch sử thay đổi trong cùng giao dịch
T07 | Gửi lặp một khoản chi trợ cấp | Không ghi trùng cùng giao dịch
T08 | Chuyển hồ sơ sang trạng thái không hợp lệ | Từ chối và không thay đổi lịch sử
T09 | Cán bộ sửa Id bản ghi sang thôn khác | Không đọc hoặc ghi được dữ liệu ngoài phạm vi
T10 | Xuất Excel khi bị giới hạn thôn | Chỉ xuất dữ liệu được phép
T11 | Phiên hết hạn trong lúc mở màn hình | Thao tác tiếp theo bị từ chối
T12 | Cơ sở dữ liệu không có biến động | Dashboard không tự sinh số liệu mẫu
T13 | Xem thống kê theo một kỳ xác định | Chỉ đếm bản ghi thuộc kỳ đó
T14 | Lỗi ghi nhật ký sau cập nhật nghiệp vụ | Có cơ chế bảo toàn truy vết đã thiết kế
@end
@table Bảng B.2. Các ca kiểm tra AI dự kiến
Mã | Nhóm trường hợp | Tiêu chí
A01 | Hai hồ sơ cùng người, tên khác cách viết | Phát hiện được và giải thích tín hiệu
A02 | Hai người cùng họ tên | Không tự hợp nhất
A03 | Hồ sơ thiếu định danh | Cho phép kết quả chưa chắc chắn
A04 | Ngày sự kiện trước ngày sinh | Cảnh báo có lý do rõ ràng
A05 | Yêu cầu có nhiều chủ đề | Gợi ý phù hợp hoặc yêu cầu làm rõ
A06 | Câu hỏi thủ tục ngoài kho tài liệu | Không tạo câu trả lời có vẻ chắc chắn
A07 | Dịch vụ AI mất kết nối | Báo lỗi có kiểm soát, không làm mất hồ sơ
A08 | Câu trả lời tra cứu có nguồn | Người dùng truy được tài liệu căn cứ
@end
# PHỤ LỤC C\nHƯỚNG DẪN KIỂM TRA VÀ BÀN GIAO
## C.1. Chuẩn bị môi trường
Môi trường cần bộ SDK phù hợp với TargetFramework net10.0 và các phụ thuộc được khai báo trong dự án. Tạo cấu hình riêng cho cơ sở dữ liệu thử nghiệm, dịch vụ phiên và địa chỉ API. Không chép khóa AI, mật khẩu hoặc chuỗi kết nối chứa thông tin bí mật vào báo cáo. Các giá trị này được cấp bằng cơ chế cấu hình của môi trường sử dụng.
Trước khi chạy ứng dụng với cơ sở dữ liệu đã có, cần kiểm tra trạng thái migration và sao lưu theo quy trình của môi trường đó. Không sử dụng lệnh tạo hoặc cập nhật dữ liệu thử trên cơ sở dữ liệu đang dùng mà chưa xác định tác động. Những lệnh trong mục C.2 chỉ phục vụ kiểm tra độc lập trong bộ nhớ theo cấu trúc hiện có.
## C.2. Chạy lại các công cụ kiểm tra
Từ thư mục gốc dự án, chạy công cụ dữ liệu bằng lệnh: dotnet run --project Tools/DemoPopulation/DemoPopulation.csproj -- --self-test. Chế độ này tạo SQLite trong bộ nhớ, kiểm tra việc sinh dữ liệu lặp, phạm vi đơn vị và phân trang. Không thay --self-test bằng --apply khi mục đích chỉ là kiểm tra độc lập.
Kiểm tra phạm vi thôn bằng lệnh: dotnet run --project Tools/VillageScopeChecks/VillageScopeChecks.csproj. Kiểm tra tổng hợp bằng lệnh: dotnet run --project Tools/EntityModelChecks/EntityModelChecks.csproj. Trong lần kiểm tra tiếp theo cần dựng lại các dự án tham chiếu và xử lý ca thử tạo hộ đã nêu ở mục 3.5.4. Lưu toàn bộ đầu ra cùng mã thoát, không chỉ lưu các dòng PASS.
## C.3. Cấu trúc bàn giao
Theo hướng dẫn trình bày, bộ dữ liệu bàn giao có thể tổ chức thành các thư mục Thesis, Pdf, Resource và Source cùng tệp giới thiệu. Thesis chứa báo cáo Word; Pdf chứa bản xuất PDF tương ứng; Resource chứa tài liệu tham khảo được phép cung cấp và hướng dẫn phụ thuộc; Source chứa mã nguồn, công cụ dữ liệu mô phỏng và công cụ kiểm tra [4].
Tệp giới thiệu cần nêu tác giả, tên đề tài, phiên bản, cấu trúc thư mục, điều kiện cài đặt và cách chạy. Các ảnh minh chứng giao diện phải lấy từ đúng phiên bản sản phẩm được bàn giao. Nhật ký thực hiện phải phản ánh công việc đã làm; nhận xét và chữ ký do giảng viên, bộ môn và khoa thực hiện theo biểu mẫu.
