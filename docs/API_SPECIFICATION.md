# 🔌 API Specification - Nền Tảng Số Xã Tân An (v1.0)

Tài liệu tả bộ RESTful APIs của hệ thống Quản lý Dân cư, An sinh Xã hội & Điều hành Xã Tân An.

---

## 1. Auth APIs (`/api/v1/Auth`)
- **POST** `/api/v1/Auth/login`
  - Body: `{ "username": "admin", "password": "admin123" }`
  - Response: Token JWT, FullName, Role, ApThon.
- **POST** `/api/v1/Auth/register`
  - Body: `{ "username": "...", "password": "...", "fullName": "...", "role": "NguoiDan" }`

---

## 2. Dân Cư APIs (`/api/v1/HoGiaDinh` & `/api/v1/NhanKhau`)
- **GET** `/api/v1/HoGiaDinh` (Query params: `keyword`, `apThon`, `pageIndex`, `pageSize`)
- **GET** `/api/v1/HoGiaDinh/{id}`
- **POST** `/api/v1/HoGiaDinh`
- **GET** `/api/v1/NhanKhau` (Query params: `keyword`, `apThon`, `pageIndex`, `pageSize`)
- **GET** `/api/v1/NhanKhau/{id}`
- **POST** `/api/v1/NhanKhau`
- **PUT** `/api/v1/NhanKhau/{id}`

---

## 3. Biến Động Dân Cư APIs (`/api/v1/BienDong`)
- **GET** `/api/v1/BienDong`
- **POST** `/api/v1/BienDong` (Body: `{ "loaiBienDong": "KhaiSinh|KhaiTu|TamTru|TamVang|ChuyenDen|ChuyenDi", "nhanKhauId": "...", "lyDo": "..." }`)

---

## 4. An Sinh Xã Hội APIs (`/api/v1/Welfare`)
- **GET** `/api/v1/Welfare` (Query params: `keyword`, `loaiDoiTuong`)
- **POST** `/api/v1/Welfare`
- **POST** `/api/v1/Welfare/tro-cap` (Lập sổ chi trả trợ cấp hàng tháng)

---

## 5. Dịch Vụ Công Điện Tử APIs (`/api/v1/CitizenRequest`)
- **GET** `/api/v1/CitizenRequest`
- **POST** `/api/v1/CitizenRequest` (Nộp hồ sơ trực tuyến)
- **PUT** `/api/v1/CitizenRequest/status` (Phê duyệt/Từ chối hồ sơ)

---

## 6. AI Engine APIs (`/api/v1/AI`)
- **GET** `/api/v1/AI/duplicates` (Phát hiện trùng lặp mờ Fuzzy matching)
- **GET** `/api/v1/AI/anomalies` (Nhận diện dữ liệu bất thường Anomaly detection)
- **POST** `/api/v1/AI/classify-request` (Gợi ý phân loại theo từ khóa, có xử lý nhiều nhóm/chưa xác định; yêu cầu đăng nhập)
- **GET** `/api/v1/AI/status` (Trạng thái cấu hình trợ lý; yêu cầu đăng nhập)
- **POST** `/api/v1/AI/procedure-chatbot` (Trợ lý có ngữ cảnh, nguồn hướng dẫn, chế độ Gemini/nội bộ; yêu cầu đăng nhập)
- **POST** `/api/v1/AI/chatbot` (Alias của `procedure-chatbot`)

Chi tiết cấu hình, dữ liệu và giới hạn: [Trợ lý AI](AI_ASSISTANT.md).

---

## 7. Dashboard & Audit Log APIs (`/api/v1/Dashboard` & `/api/v1/AuditLog`)
- **GET** `/api/v1/Dashboard/overview`
- **GET** `/api/v1/AuditLog`
