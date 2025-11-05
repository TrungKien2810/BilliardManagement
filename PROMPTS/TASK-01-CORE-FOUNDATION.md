### YÊU CẦU NHIỆM VỤ 01: CORE & FOUNDATION

**Người thực hiện:** Team Lead

**Mục tiêu:** Dựng khung sườn dự án (Solution), kết nối CSDL và xây dựng màn hình Đăng nhập.

**Nhiệm vụ:**

**1. Dựng cấu trúc Solution:**

* Tạo 1 Solution (`Billiards.sln`).
* Bên trong, tạo 3 Project:
    1.  `Billiards.UI` (Loại: WPF App)
    2.  `Billiards.BLL` (Loại: Class Library)
    3.  `Billiards.DAL` (Loại: Class Library)
* Thiết lập References (Project References):
    * `UI` tham chiếu đến `BLL`.
    * `BLL` tham chiếu đến `DAL`.

**2. Cấu hình Data Access Layer (`Billiards.DAL`):**

* Cài đặt các gói NuGet: `Microsoft.EntityFrameworkCore.SqlServer` và `Microsoft.EntityFrameworkCore.Tools`.
* Tạo thư mục `Models` (hoặc `Entities`).
* Tạo 11 file class C# (POCO) tương ứng với 11 bảng CSDL trong `PROMPT-CONTEXT.md`. (Ví dụ: `Table.cs`, `Product.cs`, `Invoice.cs`...).
* Tạo file `AppDbContext.cs` kế thừa từ `DbContext`.
* Trong `AppDbContext.cs`:
    * Tạo 11 `DbSet<>` cho 11 model.
    * Viết (hoặc yêu cầu AI viết) phương thức `OnModelCreating` để cấu hình các mối quan hệ (Foreign Keys, Primary Keys) bằng Fluent API.

**3. Cấu hình Kết nối (Connection String):**

* Trong project `Billiards.UI`, tạo file `appsettings.json`.
* Thêm Connection String vào file này. (Ví dụ: `"Server=.\\SQLEXPRESS;Database=BilliardsDB;Trusted_Connection=True;TrustServerCertificate=True;"`)
* Trong `App.xaml.cs` (của `UI`) hoặc trong `AppDbContext.cs` (dùng `OnConfiguring`), hãy đọc chuỗi kết nối này.

**4. Xây dựng Logic Đăng nhập (`Billiards.BLL`):**

* Tạo thư mục `Services`.
* Tạo file `AuthService.cs`.
* Tạo phương thức `public Account Login(string username, string password)`:
    * Phương thức này gọi `AppDbContext` (từ `DAL`).
    * Tìm `Account` dựa trên `username`.
    * (Tạm thời) So sánh `password` (chưa cần hash). Nếu đúng, trả về object `Account`. Nếu sai, trả về `null`.

**5. Xây dựng Giao diện Đăng nhập (`Billiards.UI`):**

* Tạo 1 Window mới tên là `LoginWindow.xaml` và set nó làm cửa sổ khởi động (StartupUri trong `App.xaml`).
* Thiết kế giao diện `LoginWindow.xaml`:
    * 1 `TextBox` cho Username (`txtUsername`).
    * 1 `PasswordBox` cho Password (`txtPassword`).
    * 1 `Button` ("Đăng nhập") (`btnLogin`).
* Trong file `LoginWindow.xaml.cs` (code-behind):
    * Viết sự kiện `btnLogin_Click`.
    * Gọi `AuthService` từ BLL: `var authService = new AuthService();`
    * Gọi `var account = authService.Login(txtUsername.Text, txtPassword.Password);`
    * Nếu `account != null`:
        * Mở `MainWindow` (tạo 1 `MainWindow.xaml` tạm thời).
        * Đóng `LoginWindow`.
    * Nếu `account == null`:
        * Hiển thị `MessageBox.Show("Sai thông tin đăng nhập!");`

