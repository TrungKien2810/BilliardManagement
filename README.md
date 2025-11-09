# Hệ Thống Quản Lý Quán Bida (Billiard Management System)

## 📋 Tổng Quan Dự Án

Hệ thống quản lý quán bida là một ứng dụng desktop Windows được xây dựng bằng WPF (Windows Presentation Foundation) và .NET 9.0. Hệ thống hỗ trợ quản lý toàn bộ hoạt động của một quán bida bao gồm quản lý bàn, đặt hàng, thanh toán, quản lý sản phẩm, nhân viên và khách hàng.

## 🏗️ Kiến Trúc Dự Án

Dự án được tổ chức theo mô hình 3 lớp (3-Tier Architecture):

### 1. **Billiards.DAL** (Data Access Layer)
- **Chức năng**: Lớp truy cập dữ liệu
- **Công nghệ**: Entity Framework Core 9.0 với SQL Server
- **Nhiệm vụ**:
  - Quản lý kết nối database
  - Định nghĩa các Entity Models
  - Thực hiện các thao tác CRUD thông qua Repositories
  - Cấu hình Entity Framework mapping

### 2. **Billiards.BLL** (Business Logic Layer)
- **Chức năng**: Lớp nghiệp vụ
- **Nhiệm vụ**:
  - Xử lý logic nghiệp vụ
  - Quản lý session và authentication
  - Tính toán phí bàn, hóa đơn
  - Quản lý quyền truy cập

### 3. **Billiards.UI** (User Interface Layer)
- **Chức năng**: Lớp giao diện người dùng
- **Công nghệ**: WPF (Windows Presentation Foundation)
- **Nhiệm vụ**:
  - Hiển thị giao diện người dùng
  - Xử lý tương tác người dùng
  - Hiển thị dữ liệu từ Business Layer

## 📁 Cấu Trúc Thư Mục

```
BilliardManagement/
├── Billiards.DAL/                 # Data Access Layer
│   ├── Models/                    # Entity Models
│   │   ├── Account.cs
│   │   ├── Area.cs
│   │   ├── Customer.cs
│   │   ├── Employee.cs
│   │   ├── HourlyPricingRule.cs
│   │   ├── Invoice.cs
│   │   ├── InvoiceDetail.cs
│   │   ├── Product.cs
│   │   ├── ProductCategory.cs
│   │   ├── Table.cs
│   │   └── TableType.cs
│   ├── Repositories/              # Repository Pattern
│   │   ├── AreaRepository.cs
│   │   ├── CustomerRepository.cs
│   │   ├── EmployeeRepository.cs
│   │   ├── InvoiceRepository.cs
│   │   ├── PricingRepository.cs
│   │   ├── ProductCategoryRepository.cs
│   │   ├── ProductRepository.cs
│   │   ├── TableRepository.cs
│   │   └── TableTypeRepository.cs
│   └── AppDbContext.cs            # DbContext cho Entity Framework
│
├── Billiards.BLL/                 # Business Logic Layer
│   └── Services/                  # Business Services
│       ├── AreaService.cs
│       ├── AuthorizationHelper.cs
│       ├── AuthService.cs
│       ├── BillingService.cs
│       ├── CustomerService.cs
│       ├── EmployeeService.cs
│       ├── OrderService.cs
│       ├── PricingService.cs
│       ├── ProductService.cs
│       ├── SessionManager.cs
│       ├── TableManagementService.cs
│       └── TableService.cs
│
├── Billiards.UI/                  # User Interface Layer
│   ├── Windows/                   # Các cửa sổ chính
│   │   ├── LoginWindow.xaml
│   │   ├── LoginWindow.xaml.cs
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── OrderWindow.xaml
│   │   ├── OrderWindow.xaml.cs
│   │   ├── CheckoutWindow.xaml
│   │   ├── CheckoutWindow.xaml.cs
│   │   ├── InputDialog.xaml
│   │   └── InputDialog.xaml.cs
│   ├── Views/                     # Các view quản lý
│   │   ├── CustomerManagementView.xaml
│   │   ├── CustomerManagementView.xaml.cs
│   │   ├── EmployeeManagementView.xaml
│   │   ├── EmployeeManagementView.xaml.cs
│   │   ├── PricingManagementView.xaml
│   │   ├── PricingManagementView.xaml.cs
│   │   ├── ProductManagementView.xaml
│   │   ├── ProductManagementView.xaml.cs
│   │   ├── TableManagementView.xaml
│   │   └── TableManagementView.xaml.cs
│   ├── Converters/                # Value Converters
│   │   └── StatusToBrushConverter.cs
│   ├── App.xaml
│   ├── App.xaml.cs
│   └── appsettings.json           # Cấu hình connection string
│
└── Database/                      # Database Scripts
    ├── CreateDatabase.sql         # Script tạo database và dữ liệu mẫu
    └── README.md                  # Hướng dẫn database
```

## 🗄️ Cơ Sở Dữ Liệu

### Database Schema

Hệ thống sử dụng SQL Server với 11 bảng chính:

1. **Areas** - Khu vực (Tầng 1, Tầng 2, Khu VIP...)
2. **TableTypes** - Loại bàn (Bàn thường, Bàn VIP, Bàn Pro...)
3. **Tables** - Bàn bida (có trạng thái: Free, InUse, Reserved, Maintenance)
4. **ProductCategories** - Loại sản phẩm (Đồ uống, Đồ ăn, Thuốc lá...)
5. **Products** - Sản phẩm/Dịch vụ
6. **Employees** - Nhân viên
7. **Accounts** - Tài khoản đăng nhập (Admin, Cashier, Staff)
8. **Customers** - Khách hàng (có điểm tích lũy)
9. **Invoices** - Hóa đơn (trạng thái: Active, Paid, Cancelled)
10. **InvoiceDetails** - Chi tiết hóa đơn (sản phẩm đã đặt)
11. **HourlyPricingRules** - Quy tắc giá theo giờ cho từng loại bàn

### Entity Relationships

- **Tables** ← belongs to → **Areas** (Many-to-One)
- **Tables** ← belongs to → **TableTypes** (Many-to-One)
- **Tables** → **Invoices** (One-to-Many)
- **Products** ← belongs to → **ProductCategories** (Many-to-One)
- **Employees** → **Accounts** (One-to-One)
- **Employees** → **Invoices** (One-to-Many)
- **Customers** → **Invoices** (One-to-Many)
- **Invoices** → **InvoiceDetails** (One-to-Many)
- **InvoiceDetails** → **Products** (Many-to-One)
- **TableTypes** → **HourlyPricingRules** (One-to-Many)

## 🔑 Tính Năng Chính

### 1. Đăng Nhập và Phân Quyền
- **Admin**: Toàn quyền quản lý hệ thống
- **Cashier**: Quản lý đơn hàng và thanh toán
- **Staff**: Quản lý bàn và đơn hàng

### 2. Quản Lý Bàn (Table Management)
- Hiển thị sơ đồ bàn theo khu vực
- Lọc bàn theo khu vực
- Trạng thái bàn:
  - **Free**: Bàn trống
  - **InUse**: Bàn đang sử dụng
  - **Reserved**: Bàn đã được đặt trước
  - **Maintenance**: Bàn đang bảo trì
- Mở phiên chơi mới (Start Session)
- Xem đơn hàng hiện tại
- Thanh toán

### 3. Quản Lý Đơn Hàng (Order Management)
- Thêm sản phẩm vào đơn hàng
- Xem giỏ hàng
- Xác nhận đơn hàng
- Cập nhật số lượng sản phẩm

### 4. Thanh Toán (Checkout)
- Tính phí bàn theo thời gian (theo quy tắc giá theo giờ)
- Hiển thị danh sách sản phẩm đã đặt
- Tính tổng tiền (Table Fee + Product Fee - Discount)
- Áp dụng giảm giá
- Hoàn tất thanh toán và cập nhật trạng thái bàn

### 5. Quản Lý Sản Phẩm (Admin Only)
- CRUD sản phẩm
- Quản lý danh mục sản phẩm
- Quản lý tồn kho

### 6. Quản Lý Bàn (Admin Only)
- CRUD bàn
- Phân loại bàn theo khu vực và loại bàn
- Cập nhật trạng thái bàn

### 7. Quản Lý Nhân Viên (Admin Only)
- CRUD nhân viên
- Quản lý tài khoản
- Phân quyền

### 8. Quản Lý Khách Hàng (Admin Only)
- CRUD khách hàng
- Quản lý điểm tích lũy

### 9. Quản Lý Giá (Admin Only)
- Quản lý quy tắc giá theo giờ
- Thiết lập giá cho từng loại bàn theo khung giờ

## 💻 Công Nghệ Sử Dụng

- **.NET 9.0**: Framework chính
- **WPF**: Giao diện người dùng
- **Entity Framework Core 9.0**: ORM cho database
- **SQL Server**: Cơ sở dữ liệu
- **C#**: Ngôn ngữ lập trình
- **Microsoft.Extensions.Configuration**: Quản lý cấu hình

## 🚀 Hướng Dẫn Cài Đặt

### Yêu Cầu Hệ Thống

- Windows 10/11 hoặc Windows Server
- .NET 9.0 SDK
- SQL Server (SQL Server Express hoặc bản đầy đủ)
- Visual Studio 2022 hoặc IDE hỗ trợ .NET 9.0

### Các Bước Cài Đặt

1. **Clone hoặc tải dự án**
   ```bash
   git clone <repository-url>
   cd BilliardManagement
   ```

2. **Tạo Database**
   - Mở SQL Server Management Studio (SSMS)
   - Chạy file `Database/CreateDatabase.sql`
   - Script sẽ tạo database `BilliardsDB` và chèn dữ liệu mẫu

3. **Cấu hình Connection String**
   - Mở file `Billiards.UI/appsettings.json`
   - Cập nhật connection string phù hợp với SQL Server của bạn:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=BilliardsDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
     }
   }
   ```

4. **Build và Chạy**
   - Mở solution `BilliardManagement.sln` trong Visual Studio
   - Build solution (Ctrl+Shift+B)
   - Chạy project `Billiards.UI` (F5)

### Tài Khoản Test

Sau khi chạy script database, bạn có thể đăng nhập với các tài khoản sau:

| Username | Password | Role | Quyền |
|----------|----------|------|-------|
| admin | admin | Admin | Toàn quyền |
| cashier | cashier | Cashier | Quản lý đơn hàng và thanh toán |
| staff | staff | Staff | Quản lý bàn và đơn hàng |

## 📖 Hướng Dẫn Sử Dụng

### Đăng Nhập
1. Mở ứng dụng
2. Nhập username và password
3. Click "Đăng nhập"

### Quản Lý Bàn
1. Trên màn hình chính, chọn khu vực từ danh sách bên trái
2. Click vào bàn để:
   - **Bàn trống (Free)**: Mở phiên chơi mới
   - **Bàn đang dùng (InUse)**: Xem đơn hàng hoặc thanh toán
3. Right-click vào bàn đang dùng để:
   - Đặt thêm sản phẩm
   - Thanh toán

### Đặt Hàng
1. Mở OrderWindow từ bàn
2. Chọn danh mục sản phẩm
3. Click vào sản phẩm và nhập số lượng
4. Xác nhận đơn hàng

### Thanh Toán
1. Mở CheckoutWindow từ bàn
2. Kiểm tra thông tin:
   - Thời gian chơi
   - Phí bàn (tự động tính theo thời gian)
   - Danh sách sản phẩm
   - Tổng tiền
3. Nhập giảm giá (nếu có)
4. Click "Thanh toán"

### Quản Lý (Admin Only)
1. Đăng nhập với tài khoản Admin
2. Sử dụng menu Admin để truy cập:
   - Quản lý Sản phẩm
   - Quản lý Bàn
   - Quản lý Nhân viên
   - Quản lý Khách hàng
   - Quản lý Giá

## 🔧 Cấu Hình

### Connection String
File cấu hình: `Billiards.UI/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BilliardsDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Database Connection
Connection string được load từ `appsettings.json` và được set vào `AppDbContext.ConnectionString` trong `App.xaml.cs`.

## 🏛️ Kiến Trúc Chi Tiết

### Data Access Layer (DAL)

#### Models
- Tất cả models được định nghĩa trong namespace `Billiards.DAL.Models`
- Sử dụng navigation properties cho relationships
- Hỗ trợ lazy loading

#### Repositories
- Mỗi entity có một repository riêng
- Repository pattern để tách biệt data access logic
- Tất cả repositories sử dụng `AppDbContext`

#### AppDbContext
- Kế thừa từ `DbContext`
- Cấu hình Entity Framework mapping trong `OnModelCreating`
- Connection string được set từ static property

### Business Logic Layer (BLL)

#### Services
- **AuthService**: Xử lý đăng nhập
- **SessionManager**: Quản lý session (Singleton pattern)
- **TableService**: Quản lý bàn và phiên chơi
- **OrderService**: Quản lý đơn hàng
- **BillingService**: Tính toán phí và thanh toán
- **ProductService**: Quản lý sản phẩm
- **CustomerService**: Quản lý khách hàng
- **EmployeeService**: Quản lý nhân viên
- **AreaService**: Quản lý khu vực
- **PricingService**: Quản lý giá
- **TableManagementService**: Quản lý bàn (CRUD)
- **AuthorizationHelper**: Kiểm tra quyền truy cập

### User Interface Layer (UI)

#### Windows
- **LoginWindow**: Đăng nhập
- **MainWindow**: Màn hình chính (sơ đồ bàn)
- **OrderWindow**: Đặt hàng
- **CheckoutWindow**: Thanh toán
- **InputDialog**: Dialog nhập liệu

#### Views
- **ProductManagementView**: Quản lý sản phẩm
- **TableManagementView**: Quản lý bàn
- **EmployeeManagementView**: Quản lý nhân viên
- **CustomerManagementView**: Quản lý khách hàng
- **PricingManagementView**: Quản lý giá

#### Converters
- **StatusToBrushConverter**: Convert trạng thái bàn sang màu sắc

## 🔐 Bảo Mật

### Authentication
- Đăng nhập bằng username/password
- Session được quản lý bởi `SessionManager` (Singleton)
- Kiểm tra session khi mở MainWindow

### Authorization
- Phân quyền theo Role (Admin, Cashier, Staff)
- Menu Admin chỉ hiển thị cho Admin
- Kiểm tra quyền trước khi truy cập các chức năng quản lý

### Lưu Ý Bảo Mật
- ⚠️ **Password chưa được hash**: Hiện tại password được lưu trữ dạng plain text. Cần implement password hashing (BCrypt, Argon2, etc.) trong production.
- ⚠️ **Connection string**: Cần bảo vệ connection string, không commit vào repository công khai.

## 🧪 Dữ Liệu Mẫu

Sau khi chạy `CreateDatabase.sql`, hệ thống sẽ có:

- **3 Khu vực**: Tầng 1, Tầng 2, Khu VIP
- **3 Loại bàn**: Bàn thường, Bàn VIP, Bàn Pro
- **7 Bàn**: Bàn 01-05, VIP 01-02
- **4 Loại sản phẩm**: Đồ uống, Đồ ăn, Thuốc lá, Khác
- **10 Sản phẩm mẫu**: Coca Cola, Pepsi, Bia, Mì tôm, etc.
- **3 Nhân viên**: Với tài khoản admin, cashier, staff
- **Quy tắc giá**: Theo khung giờ cho từng loại bàn

## 🐛 Xử Lý Lỗi

- Tất cả exceptions được catch và hiển thị message box
- SQL exceptions được xử lý riêng với thông báo hướng dẫn
- Validation input được thực hiện ở UI layer
- Database constraints được định nghĩa trong Entity Framework

## 📝 Ghi Chú Phát Triển

### Tính Năng Tính Phí Bàn
- Tính phí theo từng phút (không làm tròn lên)
- Hỗ trợ nhiều khung giờ trong ngày
- Hỗ trợ khung giờ vượt qua nửa đêm (22:00 - 08:00)
- Giá được tính dựa trên loại bàn và khung giờ

### Session Management
- Session được lưu trong memory (Singleton)
- Session sẽ mất khi đóng ứng dụng
- Cần đăng nhập lại khi mở lại ứng dụng

### Database Updates
- Sử dụng Entity Framework Migrations (có thể được thêm sau)
- Hiện tại database được tạo bằng SQL script
- Có thể sử dụng `dotnet ef migrations` để tạo migrations

## 🚧 Tính Năng Có Thể Mở Rộng

1. **Password Hashing**: Implement BCrypt hoặc Argon2
2. **Reports**: Báo cáo doanh thu, báo cáo theo ngày/tháng
3. **Loyalty Program**: Tích điểm và đổi quà
4. **Reservations**: Đặt bàn trước
5. **Multi-language**: Hỗ trợ nhiều ngôn ngữ
6. **Print Receipt**: In hóa đơn
7. **Backup/Restore**: Sao lưu và khôi phục dữ liệu
8. **Audit Log**: Ghi log các thao tác quan trọng
9. **Real-time Updates**: Cập nhật trạng thái bàn real-time
10. **Mobile App**: Ứng dụng di động cho khách hàng

## 📄 License

[Thêm thông tin license nếu có]

## 👥 Tác Giả

[Thêm thông tin tác giả]

## 📞 Liên Hệ

[Thêm thông tin liên hệ nếu cần]

---

**Lưu ý**: Đây là tài liệu tổng quan về dự án. Để biết thêm chi tiết về từng component, vui lòng xem comments trong code.

