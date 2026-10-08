# 📘 Hướng Dẫn Cài Đặt Chi Tiết - Nền Tảng Số Xã Tân An

Tài liệu này hướng dẫn từng bước cài đặt và vận hành hệ thống **Nền tảng số hỗ trợ quản lý biến động dân cư, an sinh xã hội và điều hành tại xã Tân An**.

---

## 💻 1. Yêu Cầu Môi Trường
- **Hệ điều hành**: Windows 10/11, Ubuntu 20.04+, hoặc macOS.
- **.NET SDK**: Phiên bản .NET 8 hoặc .NET 10.
- **Node.js**: Phiên bản 18+ (khuyên dùng Node.js 20 hoặc 24).
- **Python**: Phiên bản 3.10 trở lên.

---

## 🛠️ 2. Các Bước Cài Đặt & Chạy

### Bước 1: Khởi tạo CSDL & Backend (.NET 10 API)
1. Di chuyển vào thư mục backend:
   ```bash
   cd backend/TanAn.Api
   ```
2. Chạy ứng dụng API (Hệ thống sẽ tự động tạo CSDL `tan_an_db.sqlite` và nạp dữ liệu mô phỏng 500+ Hộ gia đình Xã Tân An):
   ```bash
   dotnet run
   ```
3. Mở trình duyệt kiểm tra Swagger API tại: `http://localhost:5000/swagger`.

---

### Bước 2: Chạy Python AI Engine
1. Di chuyển vào thư mục `ai_engine`:
   ```bash
   cd ai_engine
   ```
2. Cài đặt các thư viện cần thiết:
   ```bash
   pip install -r requirements.txt
   ```
3. Chạy Server AI Microservice:
   ```bash
   python main.py
   ```
4. AI Service sẽ sẵn sàng tại `http://localhost:8000`.

---

### Bước 3: Chạy Giao Diện Web Frontend (React Vite)
1. Di chuyển vào thư mục `frontend`:
   ```bash
   cd frontend
   ```
2. Cài đặt các gói phụ thuộc npm:
   ```bash
   npm install
   ```
3. Khởi động Vite Dev Server:
   ```bash
   npm run dev
   ```
4. Truy cập ứng dụng trên trình duyệt: `http://localhost:3000`.

---

## 3. Tài khoản và cấu hình menu

Ứng dụng không tự tạo tài khoản mẫu. Sử dụng tài khoản đã được cấp trong database. Admin mở /quan-tri-he-thong/quan-tri-menu để cấu hình menu, sau đó gán menu cho vai trò và vai trò cho người dùng. Xem docs/MENU_TREE.md.

---
