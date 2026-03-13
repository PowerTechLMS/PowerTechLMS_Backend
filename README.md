# Hướng dẫn chạy dự án LMS

Dự án bao gồm Backend (C# .NET 8) và Frontend (Vue 3/Vite).

## 1. Yêu cầu hệ thống

- Docker Desktop (để chạy SQL Server)
- .NET 8 SDK
- Node.js (phiên bản 20 trở lên)

## 2. Khởi động Cơ sở dữ liệu (SQL Server), dành cho máy MacOS

Mở terminal tại thư mục gốc của dự án và chạy:

```bash
docker-compose up -d
```

## 3. Chạy Backend (BE)

Mở một terminal mới, thiết lập đường dẫn và chạy Backend:

```zsh
# Thiết lập PATH cho dotnet (nếu chưa có)
export PATH="$HOME/.dotnet:$PATH"

# Di chuyển vào thư mục API và chạy
cd backend/LMS.API
dotnet run --urls "http://localhost:5100"
```

_Backend sẽ chạy tại: http://localhost:5100_

## 4. Tạo Migration mới cho EF Core

```
dotnet ef migrations add <Tên Migration mới> --project LMS.Infrastructure --startup-project LMS.API
dotnet ef database update --project LMS.Infrastructure --startup-project LMS.API
```

## 5. Khi mới vào dự án, làm sao để cài đặt các biến môi trường sử dụng (appsetting.\*)

Di chuyển vào thư mục `LMS.API`:

```powershell
cd LMS.API
```

Tạo file appsettings.json từ file mẫu. Code bên dưới chạy trong Powershell. Bạn có thể tự copy Ctrl + C và Ctrl + V bằng tay:

```
Copy-Item appsettings.Template.json appsettings.json
```

Tạo file appsettings.Development.json (nếu cần):

```
Copy-Item appsettings.Template.Development.json appsettings.Development.json
```
