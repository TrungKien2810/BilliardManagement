# Tài Liệu Tham Chiếu API/Services

## 📚 Mục Lục
- [AuthService](#authservice)
- [SessionManager](#sessionmanager)
- [TableService](#tableservice)
- [OrderService](#orderservice)
- [BillingService](#billingservice)
- [ProductService](#productservice)
- [CustomerService](#customerservice)
- [EmployeeService](#employeeservice)
- [AreaService](#areaservice)
- [PricingService](#pricingservice)
- [TableManagementService](#tablemanagementservice)
- [AuthorizationHelper](#authorizationhelper)

---

## AuthService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Xử lý đăng nhập và xác thực người dùng

### Methods

#### `Login(string username, string password)`
- **Mô tả**: Xác thực người dùng với username và password
- **Parameters**:
  - `username` (string): Tên đăng nhập
  - `password` (string): Mật khẩu
- **Returns**: `Account?` - Account object nếu đăng nhập thành công, null nếu thất bại
- **Exceptions**: Có thể throw exception nếu có lỗi database
- **Example**:
```csharp
var authService = new AuthService();
var account = authService.Login("admin", "admin");
if (account != null)
{
    // Đăng nhập thành công
}
```

---

## SessionManager

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Quản lý session người dùng (Singleton pattern)  
**Thread-safe**: Có (sử dụng double-check locking)

### Properties

#### `Instance` (static)
- **Type**: `SessionManager`
- **Mô tả**: Singleton instance của SessionManager
- **Thread-safe**: Có

#### `CurrentAccount`
- **Type**: `Account?`
- **Mô tả**: Tài khoản hiện tại đang đăng nhập

#### `CurrentEmployee`
- **Type**: `Employee?`
- **Mô tả**: Nhân viên hiện tại đang đăng nhập

#### `IsLoggedIn`
- **Type**: `bool`
- **Mô tả**: Kiểm tra xem có đang đăng nhập không

### Methods

#### `SetSession(Account account, Employee? employee)`
- **Mô tả**: Lưu session người dùng
- **Parameters**:
  - `account` (Account): Tài khoản
  - `employee` (Employee?): Nhân viên (có thể null)
- **Example**:
```csharp
SessionManager.Instance.SetSession(account, account.Employee);
```

#### `Logout()`
- **Mô tả**: Xóa session (đăng xuất)
- **Example**:
```csharp
SessionManager.Instance.Logout();
```

---

## TableService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Quản lý bàn và phiên chơi

### Methods

#### `GetTableMap()`
- **Mô tả**: Lấy tất cả bàn
- **Returns**: `List<Table>` - Danh sách tất cả bàn
- **Example**:
```csharp
var tableService = new TableService();
var tables = tableService.GetTableMap();
```

#### `GetTableMapByArea(int areaId)`
- **Mô tả**: Lấy bàn theo khu vực
- **Parameters**:
  - `areaId` (int): ID của khu vực
- **Returns**: `List<Table>` - Danh sách bàn trong khu vực
- **Example**:
```csharp
var tables = tableService.GetTableMapByArea(1);
```

#### `UpdateTableStatus(int tableId, string newStatus)`
- **Mô tả**: Cập nhật trạng thái bàn
- **Parameters**:
  - `tableId` (int): ID của bàn
  - `newStatus` (string): Trạng thái mới (Free, InUse, Reserved, Maintenance)
- **Example**:
```csharp
tableService.UpdateTableStatus(1, "InUse");
```

#### `StartSession(int tableId, int employeeId)`
- **Mô tả**: Mở phiên chơi mới (tạo invoice và cập nhật trạng thái bàn)
- **Parameters**:
  - `tableId` (int): ID của bàn
  - `employeeId` (int): ID của nhân viên
- **Returns**: `Invoice` - Hóa đơn mới được tạo
- **Example**:
```csharp
var invoice = tableService.StartSession(1, 1);
```

---

## OrderService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Quản lý đơn hàng và sản phẩm

### Methods

#### `GetMenuCategories()`
- **Mô tả**: Lấy tất cả danh mục sản phẩm
- **Returns**: `List<ProductCategory>` - Danh sách danh mục
- **Example**:
```csharp
var orderService = new OrderService();
var categories = orderService.GetMenuCategories();
```

#### `GetMenuProducts(int categoryId)`
- **Mô tả**: Lấy sản phẩm theo danh mục
- **Parameters**:
  - `categoryId` (int): ID của danh mục
- **Returns**: `List<Product>` - Danh sách sản phẩm
- **Example**:
```csharp
var products = orderService.GetMenuProducts(1);
```

#### `AddProductToInvoice(int invoiceId, int productId, int quantity)`
- **Mô tả**: Thêm sản phẩm vào hóa đơn
- **Parameters**:
  - `invoiceId` (int): ID của hóa đơn
  - `productId` (int): ID của sản phẩm
  - `quantity` (int): Số lượng
- **Example**:
```csharp
orderService.AddProductToInvoice(1, 1, 2);
```

#### `UpdateProductFee(int invoiceId)`
- **Mô tả**: Cập nhật phí sản phẩm trong hóa đơn
- **Parameters**:
  - `invoiceId` (int): ID của hóa đơn
- **Example**:
```csharp
orderService.UpdateProductFee(1);
```

---

## BillingService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Tính toán phí và thanh toán

### Methods

#### `CalculateTableFee(int invoiceId)`
- **Mô tả**: Tính phí bàn theo thời gian và quy tắc giá
- **Parameters**:
  - `invoiceId` (int): ID của hóa đơn
- **Returns**: `decimal` - Phí bàn (VNĐ)
- **Logic**:
  - Tính thời gian từ `StartTime` đến hiện tại
  - Áp dụng quy tắc giá theo từng khung giờ
  - Tính theo từng phút (không làm tròn lên)
  - Hỗ trợ khung giờ vượt qua nửa đêm
- **Example**:
```csharp
var billingService = new BillingService();
var tableFee = billingService.CalculateTableFee(1);
```

#### `GetInvoiceForCheckout(int tableId)`
- **Mô tả**: Lấy thông tin hóa đơn để thanh toán (tính lại phí bàn)
- **Parameters**:
  - `tableId` (int): ID của bàn
- **Returns**: `Invoice` - Hóa đơn với phí đã được tính lại
- **Throws**: `Exception` nếu không tìm thấy hóa đơn
- **Example**:
```csharp
var invoice = billingService.GetInvoiceForCheckout(1);
```

#### `FinalizeCheckout(int invoiceId, decimal discount, int? customerId)`
- **Mô tả**: Hoàn tất thanh toán (cập nhật invoice và trạng thái bàn)
- **Parameters**:
  - `invoiceId` (int): ID của hóa đơn
  - `discount` (decimal): Giảm giá (VNĐ)
  - `customerId` (int?): ID của khách hàng (có thể null)
- **Returns**: `bool` - true nếu thành công, false nếu thất bại
- **Throws**: `Exception` nếu có lỗi
- **Logic**:
  - Cập nhật discount và customerId
  - Tính lại TotalAmount
  - Cập nhật Status = "Paid"
  - Cập nhật EndTime = DateTime.Now
  - Cập nhật trạng thái bàn về "Free"
- **Example**:
```csharp
var success = billingService.FinalizeCheckout(1, 10000, null);
```

#### `GetInvoiceDetails(int invoiceId)`
- **Mô tả**: Lấy chi tiết hóa đơn (danh sách sản phẩm)
- **Parameters**:
  - `invoiceId` (int): ID của hóa đơn
- **Returns**: `List<InvoiceDetail>` - Danh sách chi tiết
- **Example**:
```csharp
var details = billingService.GetInvoiceDetails(1);
```

---

## ProductService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Quản lý sản phẩm (CRUD)

### Methods

#### `GetAllProducts()`
- **Returns**: `List<Product>` - Tất cả sản phẩm

#### `GetProductById(int id)`
- **Parameters**: `id` (int)
- **Returns**: `Product?` - Sản phẩm

#### `CreateProduct(Product product)`
- **Parameters**: `product` (Product)
- **Returns**: `Product` - Sản phẩm đã tạo

#### `UpdateProduct(Product product)`
- **Parameters**: `product` (Product)
- **Returns**: `bool` - true nếu thành công

#### `DeleteProduct(int id)`
- **Parameters**: `id` (int)
- **Returns**: `bool` - true nếu thành công

---

## CustomerService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Quản lý khách hàng (CRUD)

### Methods

#### `GetAllCustomers()`
- **Returns**: `List<Customer>` - Tất cả khách hàng

#### `GetCustomerById(int id)`
- **Parameters**: `id` (int)
- **Returns**: `Customer?` - Khách hàng

#### `CreateCustomer(Customer customer)`
- **Parameters**: `customer` (Customer)
- **Returns**: `Customer` - Khách hàng đã tạo

#### `UpdateCustomer(Customer customer)`
- **Parameters**: `customer` (Customer)
- **Returns**: `bool` - true nếu thành công

#### `DeleteCustomer(int id)`
- **Parameters**: `id` (int)
- **Returns**: `bool` - true nếu thành công

#### `GetCustomerByPhone(string phoneNumber)`
- **Parameters**: `phoneNumber` (string)
- **Returns**: `Customer?` - Khách hàng

---

## EmployeeService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Quản lý nhân viên (CRUD)

### Methods

#### `GetAllEmployees()`
- **Returns**: `List<Employee>` - Tất cả nhân viên

#### `GetEmployeeById(int id)`
- **Parameters**: `id` (int)
- **Returns**: `Employee?` - Nhân viên

#### `CreateEmployee(Employee employee)`
- **Parameters**: `employee` (Employee)
- **Returns**: `Employee` - Nhân viên đã tạo

#### `UpdateEmployee(Employee employee)`
- **Parameters**: `employee` (Employee)
- **Returns**: `bool` - true nếu thành công

#### `DeleteEmployee(int id)`
- **Parameters**: `id` (int)
- **Returns**: `bool` - true nếu thành công

---

## AreaService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Quản lý khu vực

### Methods

#### `GetAllAreas()`
- **Returns**: `List<Area>` - Tất cả khu vực

#### `GetAreaById(int id)`
- **Parameters**: `id` (int)
- **Returns**: `Area?` - Khu vực

#### `CreateArea(Area area)`
- **Parameters**: `area` (Area)
- **Returns**: `Area` - Khu vực đã tạo

#### `UpdateArea(Area area)`
- **Parameters**: `area` (Area)
- **Returns**: `bool` - true nếu thành công

#### `DeleteArea(int id)`
- **Parameters**: `id` (int)
- **Returns**: `bool` - true nếu thành công

---

## PricingService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Quản lý quy tắc giá

### Methods

#### `GetAllPricingRules()`
- **Returns**: `List<HourlyPricingRule>` - Tất cả quy tắc giá

#### `GetPricingRulesByTableType(int tableTypeId)`
- **Parameters**: `tableTypeId` (int)
- **Returns**: `List<HourlyPricingRule>` - Quy tắc giá cho loại bàn

#### `CreatePricingRule(HourlyPricingRule rule)`
- **Parameters**: `rule` (HourlyPricingRule)
- **Returns**: `HourlyPricingRule` - Quy tắc đã tạo

#### `UpdatePricingRule(HourlyPricingRule rule)`
- **Parameters**: `rule` (HourlyPricingRule)
- **Returns**: `bool` - true nếu thành công

#### `DeletePricingRule(int id)`
- **Parameters**: `id` (int)
- **Returns**: `bool` - true nếu thành công

---

## TableManagementService

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Quản lý bàn (CRUD - Admin only)

### Methods

#### `GetAllTables()`
- **Returns**: `List<Table>` - Tất cả bàn

#### `GetTableById(int id)`
- **Parameters**: `id` (int)
- **Returns**: `Table?` - Bàn

#### `CreateTable(Table table)`
- **Parameters**: `table` (Table)
- **Returns**: `Table` - Bàn đã tạo

#### `UpdateTable(Table table)`
- **Parameters**: `table` (Table)
- **Returns**: `bool` - true nếu thành công

#### `DeleteTable(int id)`
- **Parameters**: `id` (int)
- **Returns**: `bool` - true nếu thành công

---

## AuthorizationHelper

**Namespace**: `Billiards.BLL.Services`  
**Mục đích**: Kiểm tra quyền truy cập

### Methods

#### `IsAdmin()`
- **Mô tả**: Kiểm tra xem user hiện tại có phải Admin không
- **Returns**: `bool` - true nếu là Admin
- **Example**:
```csharp
if (AuthorizationHelper.IsAdmin())
{
    // Hiển thị menu Admin
}
```

#### `IsCashier()`
- **Mô tả**: Kiểm tra xem user hiện tại có phải Cashier không
- **Returns**: `bool` - true nếu là Cashier

#### `IsStaff()`
- **Mô tả**: Kiểm tra xem user hiện tại có phải Staff không
- **Returns**: `bool` - true nếu là Staff

---

## 📋 Repository Pattern

Tất cả repositories đều có các methods cơ bản:

### Common Repository Methods

#### `GetById(int id)`
- Lấy entity theo ID

#### `GetAll()`
- Lấy tất cả entities

#### `Create(T entity)`
- Tạo entity mới

#### `Update(T entity)`
- Cập nhật entity

#### `Delete(int id)`
- Xóa entity

### Specific Repository Methods

Mỗi repository có các methods đặc thù:

- **TableRepository**: `GetTablesByArea()`, `UpdateTableStatus()`
- **InvoiceRepository**: `CreateNewInvoice()`, `GetActiveInvoiceByTable()`, `GetInvoiceDetails()`
- **PricingRepository**: `GetRules()`
- **CustomerRepository**: `GetByPhoneNumber()`
- **ProductRepository**: `GetByCategory()`

---

## 🔄 Data Flow Examples

### Example 1: Mở Phiên Chơi
```csharp
// 1. User click vào bàn
var tableService = new TableService();

// 2. Start session
var invoice = tableService.StartSession(tableId, employeeId);
// - Cập nhật Table.Status = "InUse"
// - Tạo Invoice mới với StartTime = DateTime.Now

// 3. Mở OrderWindow
var orderWindow = new OrderWindow(invoice);
orderWindow.ShowDialog();
```

### Example 2: Đặt Hàng
```csharp
// 1. User chọn sản phẩm
var orderService = new OrderService();

// 2. Thêm sản phẩm vào invoice
orderService.AddProductToInvoice(invoiceId, productId, quantity);
// - Tạo InvoiceDetail
// - Cập nhật ProductFee trong Invoice

// 3. Xác nhận đơn hàng
// OrderWindow đóng và trả về DialogResult = true
```

### Example 3: Thanh Toán
```csharp
// 1. User click checkout
var billingService = new BillingService();

// 2. Lấy thông tin hóa đơn (tính lại phí bàn)
var invoice = billingService.GetInvoiceForCheckout(tableId);
// - Tính TableFee theo thời gian
// - Lấy ProductFee
// - Tính TotalAmount

// 3. Hoàn tất thanh toán
billingService.FinalizeCheckout(invoiceId, discount, customerId);
// - Cập nhật Invoice.Status = "Paid"
// - Cập nhật Invoice.EndTime = DateTime.Now
// - Cập nhật Table.Status = "Free"
```

---

## 🚨 Error Handling

Tất cả services có thể throw exceptions:

- **Database exceptions**: SQL Server connection errors
- **Validation errors**: Invalid input data
- **Business logic errors**: Invalid operations (ví dụ: bàn đã được sử dụng)

**Best Practice**: Luôn wrap service calls trong try-catch ở UI layer:

```csharp
try
{
    var result = service.DoSomething();
}
catch (Exception ex)
{
    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
}
```

---

## 📝 Notes

- Tất cả services đều có constructor không tham số (sử dụng `new AppDbContext()`)
- Services có thể nhận dependencies qua constructor (cho testing)
- Tất cả decimal values là VNĐ (Việt Nam Đồng)
- DateTime sử dụng local time
- Status strings: "Free", "InUse", "Reserved", "Maintenance" (cho Table); "Active", "Paid", "Cancelled" (cho Invoice)

---

**Lưu ý**: Tài liệu này mô tả các services và methods hiện tại. Khi có thay đổi, cần cập nhật tài liệu.

