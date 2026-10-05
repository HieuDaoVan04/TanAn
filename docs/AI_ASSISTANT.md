# Trợ lý AI Tân An

Trang `/ai-chatbot` cung cấp hội thoại và gợi ý nhóm yêu cầu. Khung trợ lý nổi xuất hiện trên các trang khác với người dùng có quyền menu trợ lý, hoặc Admin. Hai giao diện dùng chung thành phần `AssistantChat` và dịch vụ `IAIService`.

## Phạm vi đã triển khai

- Tra cứu 14 nhóm: khai sinh, khai tử, tạm trú, tạm vắng, thường trú, xác nhận cư trú, chuyển đến, chuyển đi, hộ nghèo/cận nghèo, người cao tuổi, trợ cấp an sinh, tiếp nhận hồ sơ, tiến độ hồ sơ, hộ và nhân khẩu.
- Chuẩn hóa tiếng Việt có/không dấu và tìm cụm từ có ranh giới từ.
- Hội thoại giữ tối đa 6 cặp hỏi/đáp làm ngữ cảnh; câu hỏi tiếp nối có thể dùng chủ đề đã hỏi. Hội thoại chỉ giữ trong bộ nhớ thành phần, không ghi xuống cơ sở dữ liệu và được xóa khi mở hội thoại mới/rời trang.
- Trả lời kèm tên nguồn, phiên bản và ngày cập nhật. Nguồn hiện là hướng dẫn sử dụng bộ mô phỏng nằm tại `ProcedureKnowledgeBase.cs`, phiên bản 1.0 ngày 02/10/2026; không phải tài liệu thủ tục pháp lý đã thẩm định.
- Tích hợp Gemini với tài liệu truy xuất đặt trong system instruction và lịch sử hội thoại. Model, khóa và thời gian chờ đọc từ cấu hình phía máy chủ; khóa gửi qua `x-goog-api-key`, không nằm trong URL hoặc trả về giao diện.
- Khi không có khóa, ngoài phạm vi, Gemini lỗi HTTP/mạng, trả dữ liệu không hợp lệ, bị chặn hoặc hết thời gian chờ: dùng hướng dẫn nội bộ và nêu rõ chế độ trả lời.
- Phân loại theo luật từ khóa, hiển thị giải thích, điểm khớp và nhóm liên quan; yêu cầu mơ hồ hoặc nhiều nhóm cần cán bộ xác nhận. `ConfidenceScore` là điểm của luật, không phải độ chính xác mô hình hoặc xác suất đã hiệu chỉnh.
- Gửi bằng nút hoặc Enter, khóa gửi lặp khi đang xử lý, tự cuộn, câu hỏi gợi ý, hội thoại mới, hiển thị lỗi và bố cục cho màn hình nhỏ.

## Cấu hình Gemini

Hệ thống hoạt động bằng hướng dẫn nội bộ ngay cả khi chưa cấu hình Gemini. Để bật gọi mô hình, đặt biến môi trường trước khi khởi động **ứng dụng Blazor** (dịch vụ được gọi trực tiếp trên máy chủ Blazor) và **API** nếu dùng API độc lập:

```powershell
$env:Gemini__ApiKey = 'KHOA_CUA_BAN'
$env:Gemini__Model = 'gemini-3.8-flash'
$env:Gemini__TimeoutSeconds = '30'
dotnet run --project src/Service.UI/Service.UI.CMS.Blazor/Service.UI.CMS.Blazor.csproj
```

Có thể dùng `GEMINI_API_KEY` hoặc cấu hình cũ `AI:ApiKey`; ưu tiên `Gemini:ApiKey`. Không lưu khóa thật vào mã nguồn. Timeout giới hạn 5–60 giây. Trạng thái “đã cấu hình” chỉ xác nhận có khóa, không đảm bảo khóa hợp lệ hoặc nhà cung cấp đang phục vụ.

Model mặc định hiện là `gemini-3.8-flash`; có thể thay bằng model được cấp quyền cho tài khoản. Tham khảo [danh mục model](https://ai.google.dev/gemini-api/docs/models) và [REST generateContent](https://ai.google.dev/gemini-api/docs/generate-content/text-generation) của Google. Hạn mức và chi phí phụ thuộc model/tài khoản, không gán cố định mức miễn phí.

## API

Các endpoint trợ lý yêu cầu đăng nhập theo cơ chế xác thực của API:

| Phương thức | Đường dẫn | Nội dung |
| --- | --- | --- |
| GET | `/api/v1/AI/status` | Có cấu hình Gemini, tên model, số nhóm và phiên bản nguồn; không trả khóa |
| POST | `/api/v1/AI/procedure-chatbot` | Trả lời câu hỏi có ngữ cảnh |
| POST | `/api/v1/AI/chatbot` | Alias tương thích tài liệu cũ |
| POST | `/api/v1/AI/classify-request` | Gợi ý nhóm yêu cầu theo nội dung |

Ví dụ nội dung yêu cầu hội thoại:

```json
{
  "question": "Cần chuẩn bị gì?",
  "history": [
    { "role": "user", "text": "Đăng ký tạm trú" },
    { "role": "model", "text": "Chuẩn bị thông tin người đăng ký và chỗ ở." }
  ]
}
```

Giới hạn câu hỏi/nội dung phân loại/mỗi lượt lịch sử: 4.000 ký tự. Lịch sử tối đa 12 lượt, bắt đầu bằng `user`, luân phiên `model`, kết thúc bằng `model`. Dữ liệu sai trả HTTP 400; chưa xác thực trả 401. Kết quả bọc trong `ApiResult<T>`. `mode` gồm `knowledge-base`, `gemini`, `knowledge-base-fallback`.

## Kiểm tra

```powershell
dotnet run --project Tools/AssistantChecks/AssistantChecks.csproj
```

Bộ kiểm tra dùng hướng dẫn nội bộ, HTTP nhà cung cấp giả lập, host API cục bộ và render Blazor. Kiểm tra phân loại đủ nhóm, tiếng Việt không dấu, mơ hồ/nhiều nhóm, đổi chủ đề, lịch sử sai, hủy yêu cầu, khóa trong header, Gemini lỗi/bị chặn và xác thực/validation API. Không cần khóa thật và không thay đổi cơ sở dữ liệu.

## Giới hạn và mở rộng

Trợ lý không truy vấn hồ sơ cá nhân, không xác nhận số liệu dân cư, không tạo/phê duyệt hồ sơ và không tự xác lập điều kiện hưởng trợ cấp. Việc tích hợp Gemini chưa thay thế đánh giá chất lượng câu trả lời trên dữ liệu nghiệp vụ đã thẩm định; system instruction không bảo đảm tuyệt đối mô hình tuân thủ nguồn. Muốn dùng cho thủ tục thực tế, cần thay hướng dẫn mô phỏng bằng tài liệu được duyệt có nguồn, ngày hiệu lực, cơ chế cập nhật và bộ đánh giá. Phát hiện trùng lặp/bất thường thuộc phân hệ riêng, không được tính là hoàn thành trong thay đổi trợ lý này.
