# Project Instructions

## Yêu cầu chung

- Tuyệt đối không viết comment và log trong code trong bất cứ trường hợp nào.
- Tên biến bắt buộc phải sử dụng tiếng Anh.
- Luôn luôn sử dụng cú pháp C# mới nhất.
- Không bao giờ được tự động cài thư viện cho các dự án, trừ trường hợp tôi yêu cầu. Nếu bạn muốn cài thêm thư viện, phải thông báo qua cho tôi.
- Không bao giờ được đổi nội dung các file appsetting.\*.json trừ trường hợp tôi yêu cầu. Nếu bạn muốn thay đổi, phải thông báo qua cho tôi.
- Tất cả phản hồi đều phải được viết bằng tiếng Việt, kể cả Task, Plan.

## Formatting

- Luôn ưu tiên việc thay thế việc gọi tên định danh đầy đủ (fully qualified name) bằng cách import namespace (using) ở đầu tệp tin.
- Chèn một dòng mới trước dấu ngoặc nhọn mở của bất kỳ khối mã nào (ví dụ: sau `if`, `for`, `while`, `foreach`, `using`, `try`, v.v.).
- Đảm bảo rằng câu lệnh return cuối cùng của một phương thức nằm trên một dòng riêng.
- Sử dụng khớp mẫu (pattern matching) và biểu thức switch bất cứ khi nào có thể.
- Sử dụng `nameof` thay vì chuỗi ký tự khi tham chiếu đến tên thành viên.

## Toàn vẹn kiểu dữ liệu và xử lý kiểu dữ liệu null.

- Luôn mặc định biến là non-nullable (không được null). Chỉ dùng ? khi thực sự có logic nghiệp vụ cho phép giá trị đó trống, hoặc trong trường Entity EF Core không phải là khoá chính.
- Chặn đứng tại cửa ngõ (Boundary Validation): Không tin tưởng vào annotation khi nhận dữ liệu từ bên ngoài (API, Database, JSON). Tại các Entry Points (Public Methods, Constructors, Controllers), phải dùng ArgumentNullException.ThrowIfNull để bảo vệ hệ thống ngay lập tức.
- Cấm sử dụng == null hoặc != null. Bắt buộc dùng is null hoặc is not null để tránh các lỗi logic do nạp chồng toán tử (operator overloading).
- Ưu tiên dùng từ khóa required hoặc Constructor để đảm bảo đối tượng luôn đủ dữ liệu. Tuyệt đối không dùng default! để "lừa" trình biên dịch.
- Tuyệt đối không dùng toán tử null-forgiving (!). Việc dùng ! là biểu hiện của sự lười biếng trong xử lý logic. Nếu bạn tin chắc nó không null, hãy chứng minh bằng code check null hoặc gán giá trị mặc định (??).
- Dù kiểu dữ liệu được khai báo là string, nhưng nếu nó đến từ một bản ghi Database cũ hoặc một API bên thứ ba, nó vẫn có thể là null. Phải kiểm tra tính toàn vẹn trước khi xử lý.

## Build và Test

- Mỗi khi có sự thay đổi trong code chạy dự án, bắt buộc phải build và test dự án.
- Trước khi bắt đầu Build và Test, phải thực hiện đóng sạch (Force Stop) tất cả các tiến trình dotnet đang vận hành trên hệ thống. Việc này nhằm giải phóng hoàn toàn các tệp tin đang bị khóa (File Locking) và làm trống bộ nhớ đệm của trình biên dịch.
- Kiểm tra biên dịch: Chạy câu lệnh dotnet build để kiểm tra xem có cảnh báo và lỗi cú pháp hay không.

## Tuyệt đối không tự ý Commit Dự án

Tuyệt đối không tự ý commit và push code lên trên git cho dự án.
