# Hướng Dẫn Chạy Dự Án

## 1. Yêu cầu môi trường

- Cài đặt **.NET 8 SDK**: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- Môi trường dev đã cấu hình sẵn SQLite (`umbraco/Data/Umbraco.sqlite.db`), không cần cài đặt SQL Server.

---

## 2. Các bước chạy dự án

### Bước 1: Tạo file cấu hình `.env`

Sao chép file `.env.example` thành `.env`:

- **Windows (PowerShell):**
  ```powershell
  Copy-Item .env.example .env
  ```
- **macOS / Linux:**
  ```bash
  cp .env.example .env
  ```

*(File `.env` chứa tài khoản quản trị ban đầu của Umbraco, bạn có thể chỉnh sửa nếu cần:)*
```env
Umbraco__CMS__Unattended__InstallUnattended=true
Umbraco__CMS__Unattended__UnattendedUserName=Admin
Umbraco__CMS__Unattended__UnattendedUserEmail=admin@example.vn
Umbraco__CMS__Unattended__UnattendedUserPassword=DoiMatKhauNay_123
```

### Bước 2: Chạy dự án

**Cách 1: Sử dụng Terminal / Command Line**
```bash
dotnet restore
dotnet build
dotnet run
```

**Cách 2: Sử dụng Visual Studio / JetBrains Rider**
1. Mở file solution `HcmPortal.sln`.
2. Chọn profile chạy: **Umbraco.Web.UI** (hoặc **IIS Express**).
3. Nhấn **F5** (hoặc **Ctrl + F5**) để chạy.

---

## 3. Truy cập hệ thống

- **Giao diện Website:**
  - `https://localhost:44305` hoặc `http://localhost:10133`

- **Trang quản trị (Umbraco Backoffice):**
  - `https://localhost:44305/umbraco`
  - **Tài khoản đăng nhập mặc định:**
    - Email: `admin@example.vn`
    - Mật khẩu: `DoiMatKhauNay_123`

---

## 4. Reset lại dữ liệu (Khi cần thiết)

Nếu muốn xoá và để hệ thống tự tạo lại toàn bộ dữ liệu mẫu sạch từ đầu:
1. Dừng ứng dụng.
2. Xóa database SQLite:
   ```powershell
   Remove-Item "umbraco\Data\Umbraco.sqlite.db*" -Force
   Remove-Item "umbraco\Data\TEMP" -Recurse -Force
   ```
3. Chạy lại lệnh:
   ```bash
   dotnet run
   ```
