# Hướng dẫn chạy dự án LMS

Dự án bao gồm Backend (C# .NET 8) và Frontend (Vue 3/Vite).

## 1. Yêu cầu hệ thống

- Docker Desktop (để chạy SQL Server)
- .NET 8 SDK
- Node.js (phiên bản 20 trở lên)

## 2. Khởi động các dịch vụ bằng Docker

Dự án sử dụng Docker để chạy các thành phần cơ sở dữ liệu.

### 2.1 Qdrant (Vector Database) - Bắt buộc

Tất cả các máy đều cần chạy Qdrant để xử lý AI và tìm kiếm:

```bash
docker-compose up -d
```

### 2.2 SQL Server (Dành cho máy MacOS hoặc người không cài Local SQL)

Nếu bạn không cài SQL Server trực tiếp trên máy, hãy chạy:

```bash
docker-compose -f docker-compose.sqlserver.yml up -d
```

### 2.3 Quản lý Qdrant

Sau khi chạy, bạn có thể truy cập:

- **Dashboard UI**: http://localhost:6333/dashboard
- **gRPC port (nội bộ)**: 6334

Qdrant được sử dụng để lưu trữ các đoạn hội thoại, bài giảng dưới dạng vector, phục vụ cho tính năng tìm kiếm thông minh và AI Assistant.

## 3. Chạy Backend (BE)

Mở một terminal mới, thiết lập đường dẫn và chạy Backend:

```zsh
# Thiết lập PATH cho dotnet (nếu chưa có)
export PATH="$HOME/.dotnet:$PATH"

# Chạy dự án
dotnet run --project "LMS.API\LMS.API.csproj" --launch-profile "http"
```

_Backend sẽ chạy tại: http://localhost:5100_

## 4. Tạo Migration mới cho EF Core SQL Server

```
dotnet ef migrations add <Tên Migration mới> --project LMS.Infrastructure --startup-project LMS.API --context AppDbContext
dotnet ef database update --project LMS.Infrastructure --startup-project LMS.API --context AppDbContext
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

Sau đó thì điền thông tin vào trong file appsettings.json và appsettings.Development.json để chạy dự án

## 6. Cấu hình gửi Mail (Gmail)

Trong file `appsettings.Development.json` hoặc `appsettings.json`, tìm đến phần `EmailSettings` và điền thông tin:

- **SmtpHost**: Mặc định là `smtp.gmail.com`.
- **SmtpPort**: Mặc định là `587`.
- **SmtpUser**: Địa chỉ Gmail của bạn (VD: `example@gmail.com`).
- **SmtpPass**: Mật khẩu ứng dụng (App Password).
  - _Lưu ý_: Không dùng mật khẩu đăng nhập Gmail thông thường. Bạn cần bật Xác minh 2 lớp và tạo [Mật khẩu ứng dụng](https://myaccount.google.com/apppasswords).
- **FromEmail**: Địa chỉ email hiển thị khi gửi (thường trùng với `SmtpUser`).

## 7. Cài đặt CUDA

- Cập nhật phiên bản card đồ hoạ lên phiên bản mới nhất.
- Vào đường link [này](https://developer.download.nvidia.com/compute/cuda/13.2.0/local_installers/cuda_13.2.0_windows.exe) và cài đặt CUDA
- Ngoài ra, phải đảm bảo card đồ hoạ đầu tiên trong máy là card đồ hoạ NVIDIA. Để cài đặt Torch phù hợp với card NVIDIA, hãy chạy:

```
.\LMS.API\External\python_env\Scripts\pip.exe install torch==2.6.0+cu124 --index-url https://download.pytorch.org/whl/cu124
```

## Hướng dẫn cho Hosting Debian (CPU-only)

Để chạy dịch vụ gỡ băng trên Debian mà không có GPU, hãy thực hiện các bước sau:

1.  **Cài đặt các gói hệ thống cần thiết**:

    ```bash
    sudo apt update
    sudo apt install python3 python3-venv python3-pip libgomp1 ffmpeg -y
    ```

    _Lưu ý: `libgomp1` là bắt buộc để OpenMP hoạt động (thư viện song song cho CPU)._

2.  **Môi trường ảo (Venv)**:
    Hệ thống sẽ tự động tạo `venv` tại `External/python_env` khi có yêu cầu gỡ băng đầu tiên.

3.  **Tối ưu hóa dung lượng (Tùy chọn)**:
    Nếu muốn tiết kiệm dung lượng ổ cứng trên Hosting (vì bản torch mặc định rất nặng), bạn có thể cài bản torch-cpu thủ công vào venv:
    ```bash
    ./External/python_env/bin/pip install torch --index-url https://download.pytorch.org/whl/cpu --force-reinstall
    ```
