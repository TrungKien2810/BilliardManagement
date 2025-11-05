### YÊU CẦU NHIỆM VỤ 05: CÁC MÀN HÌNH QUẢN TRỊ (ADMIN)

**Người thực hiện:** Member 4

**Mục tiêu:** Xây dựng các màn hình CRUD (Thêm, Sửa, Xóa) cho các đối tượng quản lý (Dịch vụ, Nhân viên, Bàn...).

**Nhiệm vụ:**

Đây là công việc lặp đi lặp lại. Hãy áp dụng **mẫu (pattern)** sau cho TẤT CẢ các mục quản lý:

**Mẫu CRUD Pattern:**

1.  **Tạo `UserControl` (trong `UI`):** (ví dụ: `ProductManagementView.xaml`).

2.  **Thiết kế Layout:**
    * Chia làm 2 cột.
    * **Cột Trái:** 1 `DataGrid` (ví dụ: `dgProducts`) để hiển thị danh sách.
    * **Cột Phải:** 1 `Grid` (ví dụ: `formProduct`) chứa các `TextBox` (cho Tên, Giá...) và các `ComboBox` (cho `Category`) để nhập liệu.
    * Bên dưới form: 3 `Button` ("Thêm mới", "Lưu thay đổi", "Xóa").

3.  **Tạo Repository (trong `DAL`):** (ví dụ: `ProductRepository.cs`).
    * `GetAll()`
    * `GetById(id)`
    * `Add(Product product)`
    * `Update(Product product)`
    * `Delete(int id)`

4.  **Tạo Service (trong `BLL`):** (ví dụ: `ProductService.cs`).
    * Các phương thức gọi thẳng xuống Repository tương ứng.

5.  **Viết Code-Behind (trong `UI` - `UserControl.xaml.cs`):**
    * Tạo phương thức `LoadDataGrid()`: Gọi `ProductService.GetAll()` và gán vào `dgProducts.ItemsSource`.
    * Sự kiện `Window_Loaded`: Gọi `LoadDataGrid()`.
    * Sự kiện `dgProducts_SelectionChanged`: Lấy `selectedProduct` và hiển thị thông tin lên các `TextBox` trong form.
    * Sự kiện `btnAddNew_Click`: Xóa trắng các `TextBox`.
    * Sự kiện `btnSave_Click`:
        * Lấy dữ liệu từ `TextBox`.
        * Kiểm tra xem `selectedProduct` có tồn tại không.
        * Nếu có (đang sửa): Gọi `productService.Update(...)`.
        * Nếu không (đang thêm mới): Gọi `productService.Add(...)`.
        * Gọi `LoadDataGrid()` để làm mới danh sách.
    * Sự kiện `btnDelete_Click`:
        * Lấy `selectedProduct`.
        * Hỏi xác nhận.
        * Gọi `productService.Delete(selectedProduct.ID)`.
        * Gọi `LoadDataGrid()`.

**Các màn hình cần tạo (áp dụng pattern trên):**

1.  **Quản lý Dịch vụ & Loại dịch vụ** (`Products` & `ProductCategories`).
2.  **Quản lý Bàn, Khu vực, Loại bàn** (`Tables`, `Areas`, `TableTypes`).
3.  **Quản lý Nhân viên & Tài khoản** (`Employees` & `Accounts`).
4.  **Quản lý Khách hàng** (`Customers`).
5.  **Quản lý Giá giờ** (`HourlyPricingRules`).

**Tích hợp vào `MainWindow`:**

* Trong `MainWindow.xaml`, thêm 1 `Menu` (hoặc `TabControl`).
* Thêm các `MenuItem` ("Quản lý Dịch vụ", "Quản lý Bàn"...).
* Khi bấm vào `MenuItem`, hiển thị `UserControl` tương ứng (ví dụ: `ProductManagementView`) vào 1 khu vực nội dung (ContentArea) trong `MainWindow`.
* (Lưu ý: Chỉ hiển thị các Menu này nếu `Role` của `Account` đăng nhập là "Admin").

