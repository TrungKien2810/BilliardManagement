# Tài Liệu Kiến Trúc Hệ Thống

## 📐 Kiến Trúc Tổng Quan

Hệ thống được xây dựng theo mô hình **3-Layer Architecture** với các nguyên tắc:

- **Separation of Concerns**: Mỗi layer có trách nhiệm riêng biệt
- **Dependency Injection**: Các layer phụ thuộc theo chiều từ trên xuống dưới
- **Repository Pattern**: Tách biệt data access logic
- **Service Pattern**: Tách biệt business logic

```
┌─────────────────────────────────────────┐
│         UI Layer (WPF)                  │
│  - Windows, Views, Converters           │
│  - User Interactions                    │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│      Business Logic Layer (BLL)         │
│  - Services, SessionManager             │
│  - Business Rules, Validations          │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│    Data Access Layer (DAL)              │
│  - Repositories, DbContext              │
│  - Entity Models                        │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│         SQL Server Database             │
│  - Tables, Relationships                │
└─────────────────────────────────────────┘
```

## 🏛️ Chi Tiết Các Layer

### 1. Data Access Layer (DAL)

#### AppDbContext
- **Mục đích**: Quản lý kết nối database và cấu hình Entity Framework
- **Connection String**: Được set từ static property `ConnectionString`
- **Configuration**: Entity mapping được định nghĩa trong `OnModelCreating`

```csharp
public class AppDbContext : DbContext
{
    public static string? ConnectionString { get; set; }
    
    // DbSets for all entities
    public DbSet<Area> Areas { get; set; }
    public DbSet<Table> Tables { get; set; }
    // ... other DbSets
}
```

#### Models
Tất cả models đều có:
- **Primary Key**: ID (int, auto-increment) hoặc Username (string)
- **Navigation Properties**: Để truy cập related entities
- **Default Values**: Cho các trường có giá trị mặc định

#### Repositories
Mỗi repository cung cấp:
- **CRUD Operations**: Create, Read, Update, Delete
- **Specific Queries**: Các truy vấn đặc thù cho từng entity
- **DbContext Injection**: Nhận DbContext qua constructor

**Repository Pattern Benefits**:
- Tách biệt data access logic
- Dễ dàng test và mock
- Có thể thay đổi data source mà không ảnh hưởng business logic

### 2. Business Logic Layer (BLL)

#### Services
Mỗi service đại diện cho một domain logic cụ thể:

**AuthService**
- `Login(string username, string password)`: Xác thực người dùng
- Sử dụng `AppDbContext` để truy vấn Accounts

**SessionManager** (Singleton)
- `SetSession(Account, Employee)`: Lưu session
- `Logout()`: Xóa session
- `IsLoggedIn`: Kiểm tra trạng thái đăng nhập
- `CurrentAccount`, `CurrentEmployee`: Thông tin session hiện tại

**TableService**
- `GetTableMap()`: Lấy tất cả bàn
- `GetTableMapByArea(int areaId)`: Lấy bàn theo khu vực
- `StartSession(int tableId, int employeeId)`: Mở phiên chơi mới
- `UpdateTableStatus(int tableId, string status)`: Cập nhật trạng thái bàn

**OrderService**
- `GetMenuCategories()`: Lấy danh mục sản phẩm
- `GetMenuProducts(int categoryId)`: Lấy sản phẩm theo danh mục
- `AddProductToInvoice(int invoiceId, int productId, int quantity)`: Thêm sản phẩm vào hóa đơn
- `UpdateProductFee(int invoiceId)`: Cập nhật phí sản phẩm

**BillingService**
- `CalculateTableFee(int invoiceId)`: Tính phí bàn theo thời gian
- `GetInvoiceForCheckout(int tableId)`: Lấy thông tin hóa đơn để thanh toán
- `FinalizeCheckout(int invoiceId, decimal discount, int? customerId)`: Hoàn tất thanh toán
- `GetInvoiceDetails(int invoiceId)`: Lấy chi tiết hóa đơn

**ProductService, CustomerService, EmployeeService, AreaService, PricingService, TableManagementService**
- CRUD operations cho các entities tương ứng

**AuthorizationHelper**
- `IsAdmin()`: Kiểm tra quyền Admin
- `IsCashier()`: Kiểm tra quyền Cashier
- `IsStaff()`: Kiểm tra quyền Staff

### 3. User Interface Layer (UI)

#### Windows
**LoginWindow**
- Đăng nhập người dùng
- Xử lý lỗi kết nối database
- Chuyển sang MainWindow sau khi đăng nhập thành công

**MainWindow**
- Hiển thị sơ đồ bàn
- Lọc bàn theo khu vực
- Xử lý click vào bàn (mở phiên, xem đơn, thanh toán)
- Context menu cho bàn đang sử dụng
- Menu Admin (chỉ hiển thị cho Admin)

**OrderWindow**
- Hiển thị danh mục và sản phẩm
- Thêm sản phẩm vào giỏ hàng
- Xác nhận đơn hàng

**CheckoutWindow**
- Hiển thị thông tin hóa đơn
- Tính phí bàn (tự động)
- Hiển thị danh sách sản phẩm
- Áp dụng giảm giá
- Hoàn tất thanh toán

#### Views (Admin Only)
- **ProductManagementView**: CRUD sản phẩm
- **TableManagementView**: CRUD bàn
- **EmployeeManagementView**: CRUD nhân viên
- **CustomerManagementView**: CRUD khách hàng
- **PricingManagementView**: CRUD quy tắc giá

#### Converters
**StatusToBrushConverter**
- Convert trạng thái bàn (Free, InUse, Reserved, Maintenance) sang màu sắc
- Sử dụng trong XAML binding

## 🔄 Luồng Dữ Liệu

### Luồng Đăng Nhập
```
User Input (LoginWindow)
    ↓
AuthService.Login()
    ↓
AppDbContext (DAL)
    ↓
Account Entity
    ↓
SessionManager.SetSession()
    ↓
MainWindow (hiển thị thông tin user)
```

### Luồng Mở Phiên Chơi
```
User Click Table (MainWindow)
    ↓
TableService.StartSession()
    ↓
TableRepository.UpdateTableStatus() → Table.Status = "InUse"
    ↓
InvoiceRepository.CreateNewInvoice() → Tạo Invoice mới
    ↓
OrderWindow (hiển thị menu)
```

### Luồng Đặt Hàng
```
User Select Products (OrderWindow)
    ↓
OrderService.AddProductToInvoice()
    ↓
InvoiceRepository.AddInvoiceDetail()
    ↓
OrderService.UpdateProductFee()
    ↓
InvoiceRepository.Update() → Cập nhật ProductFee
```

### Luồng Thanh Toán
```
User Click Checkout (MainWindow)
    ↓
BillingService.GetInvoiceForCheckout()
    ↓
BillingService.CalculateTableFee() → Tính phí theo thời gian
    ↓
CheckoutWindow (hiển thị thông tin)
    ↓
User Confirm Checkout
    ↓
BillingService.FinalizeCheckout()
    ↓
InvoiceRepository.Update() → Invoice.Status = "Paid"
    ↓
TableRepository.UpdateTableStatus() → Table.Status = "Free"
```

## 💾 Quản Lý Dữ Liệu

### Entity Relationships

#### One-to-Many
- **Area → Tables**: Một khu vực có nhiều bàn
- **TableType → Tables**: Một loại bàn có nhiều bàn
- **Table → Invoices**: Một bàn có nhiều hóa đơn
- **Employee → Invoices**: Một nhân viên tạo nhiều hóa đơn
- **Customer → Invoices**: Một khách hàng có nhiều hóa đơn
- **ProductCategory → Products**: Một danh mục có nhiều sản phẩm
- **TableType → HourlyPricingRules**: Một loại bàn có nhiều quy tắc giá
- **Invoice → InvoiceDetails**: Một hóa đơn có nhiều chi tiết
- **Product → InvoiceDetails**: Một sản phẩm xuất hiện trong nhiều chi tiết

#### One-to-One
- **Employee → Account**: Một nhân viên có một tài khoản

### Database Transactions
- Entity Framework tự động quản lý transactions
- Mỗi `SaveChanges()` là một transaction
- Nếu có lỗi, transaction sẽ rollback

### Concurrency
- Hiện tại chưa có xử lý concurrency
- Có thể thêm optimistic concurrency control bằng cách sử dụng `RowVersion`

## 🔐 Bảo Mật

### Authentication Flow
1. User nhập username/password
2. `AuthService.Login()` kiểm tra trong database
3. Nếu đúng, `SessionManager.SetSession()` lưu session
4. Session được lưu trong memory (Singleton)

### Authorization Flow
1. `SessionManager.CurrentAccount` chứa thông tin user
2. `AuthorizationHelper` kiểm tra Role
3. UI ẩn/hiện menu dựa trên Role
4. Services kiểm tra quyền trước khi thực hiện operations

### Security Considerations
- ⚠️ Password chưa được hash (cần implement)
- ⚠️ Session lưu trong memory (mất khi đóng app)
- ⚠️ Connection string trong appsettings.json (cần bảo vệ)

## 🧪 Testing Strategy

### Unit Testing
- Test Services với mock repositories
- Test Business logic
- Test Calculations (BillingService)

### Integration Testing
- Test Repository với test database
- Test Services với real repositories
- Test End-to-end workflows

### UI Testing
- Test Windows và Views
- Test User interactions
- Test Navigation flow

## 🚀 Performance Considerations

### Database
- Indexes trên foreign keys
- Indexes trên các trường thường query (Status, PhoneNumber)
- Lazy loading cho navigation properties

### Caching
- Session được cache trong memory
- Có thể cache danh sách sản phẩm, danh mục
- Có thể cache pricing rules

### Optimizations
- Sử dụng `Include()` để eager load khi cần
- Sử dụng `AsNoTracking()` cho read-only queries
- Batch operations khi có thể

## 📦 Dependencies

### NuGet Packages
- **Microsoft.EntityFrameworkCore.SqlServer** (9.0.10): Entity Framework Core cho SQL Server
- **Microsoft.EntityFrameworkCore.Tools** (9.0.10): EF Core tools
- **Microsoft.Extensions.Configuration** (9.0.10): Configuration management
- **Microsoft.Extensions.Configuration.Json** (9.0.10): JSON configuration provider

### Project Dependencies
```
Billiards.UI
    └── Billiards.BLL
            └── Billiards.DAL
```

## 🔧 Configuration

### Connection String
- Được load từ `appsettings.json`
- Set vào `AppDbContext.ConnectionString` trong `App.xaml.cs`
- Có thể override bằng environment variables

### Entity Framework Configuration
- Fluent API trong `AppDbContext.OnModelCreating()`
- Table names, column names, constraints
- Relationships và foreign keys
- Default values

## 📝 Best Practices

### Code Organization
- Mỗi class trong file riêng
- Namespace theo layer
- Clear naming conventions

### Error Handling
- Try-catch ở UI layer
- Throw exceptions từ BLL
- Log errors (có thể thêm sau)

### Validation
- Input validation ở UI layer
- Business rules validation ở BLL
- Database constraints ở DAL

### Documentation
- XML comments cho public APIs
- Code comments cho complex logic
- README và architecture docs

## 🔄 Future Improvements

1. **Dependency Injection**: Sử dụng DI container (Microsoft.Extensions.DependencyInjection)
2. **Unit of Work Pattern**: Quản lý transactions tốt hơn
3. **CQRS**: Tách read và write operations
4. **Event Sourcing**: Track changes
5. **Microservices**: Tách thành các services riêng biệt
6. **API Layer**: RESTful API cho web/mobile
7. **Real-time**: SignalR cho real-time updates
8. **Caching**: Redis cho distributed caching
9. **Logging**: Serilog hoặc NLog
10. **Monitoring**: Application Insights hoặc similar

---

**Lưu ý**: Tài liệu này mô tả kiến trúc hiện tại của hệ thống. Khi hệ thống phát triển, tài liệu cần được cập nhật.

