### YÊU CẦU NHIỆM VỤ 03: QUẢN LÝ ORDER & KHO

**Người thực hiện:** Member 3

**Mục tiêu:** Xây dựng Màn hình (Popup) Order, cho phép thêm dịch vụ vào hóa đơn và tự động trừ kho.

**Nhiệm vụ:**

**1. Cập nhật Logic Bắt đầu Chơi (`BLL` và `DAL`):**

* **Trong `Billiards.DAL`:**
    * Tạo `InvoiceRepository.cs`.
    * Tạo phương thức `public Invoice CreateNewInvoice(int tableId, int employeeId)`:
        * Tạo object `Invoice` mới (Status="Active", `StartTime`=Now, `TableID`=tableId...).
        * `_context.Invoices.Add(newInvoice);`
        * `_context.SaveChanges();`
        * Trả về `newInvoice` (kèm `ID` vừa tạo).

* **Trong `Billiards.BLL` (Sửa `TableService`):**
    * Sửa lại phương thức `StartSession(int tableId, int employeeId)`:
        * Gọi `tableRepository.UpdateTableStatus(tableId, "InUse");`
        * Gọi `invoiceRepository.CreateNewInvoice(tableId, employeeId);` (Cần lấy `employeeId` từ lúc đăng nhập).

**2. Tạo Logic Lấy Menu và Order (`BLL` và `DAL`):**

* **Trong `Billiards.DAL`:**
    * Tạo `ProductRepository.cs`.
    * Tạo `public List<ProductCategory> GetCategories()`.
    * Tạo `public List<Product> GetProductsByCategory(int categoryId)`.

* **Trong `Billiards.BLL`:**
    * Tạo `OrderService.cs`.
    * Tạo `public List<ProductCategory> GetMenuCategories()`.
    * Tạo `public List<Product> GetMenuProducts(int categoryId)`.
    * Tạo `public void AddProductToInvoice(int invoiceId, int productId, int quantity)`:
        * Kiểm tra `ProductRepository` xem `StockQuantity` có đủ không.
        * Nếu đủ:
            * Tạo `InvoiceDetail` mới và lưu vào CSDL (hoặc cập nhật `Quantity` nếu đã tồn tại).
            * Cập nhật `Products.StockQuantity` (trừ kho).
            * Cập nhật `Invoices.ProductFee` (tổng tiền dịch vụ).

**3. Xây dựng Giao diện `OrderWindow.xaml` (`Billiards.UI`):**

* Tạo một `Window` mới tên là `OrderWindow.xaml` (đây là popup).
* **Bố cục 3 cột:**
    * **Cột 1 (Trái):** `ListBox` tên `lbCategories` (hiển thị `ProductCategories`).
    * **Cột 2 (Giữa):** `ItemsControl` (hoặc `ListBox`) tên `lbProducts` (hiển thị `Products`). Dùng `WrapPanel` tương tự Sơ đồ bàn.
    * **Cột 3 (Phải):** `DataGrid` tên `dgCart` (hiển thị các món vừa chọn - `InvoiceDetails` tạm thời).

* **Trong `OrderWindow.xaml.cs`:**
    * Tạo 1 biến `private Invoice currentInvoice;`
    * Constructor nhận vào `Invoice`: `public OrderWindow(Invoice invoice)`.
    * Khi `Window_Loaded`:
        * Gọi `OrderService` để tải `lbCategories`.
    * Sự kiện `lbCategories_SelectionChanged`:
        * Lấy `categoryId` đã chọn.
        * Tải `lbProducts` (gọi `orderService.GetMenuProducts(categoryId)`).
    * Sự kiện `Click` vào Product (trong `lbProducts`):
        * Hỏi số lượng (dùng 1 `InputBox` đơn giản).
        * Thêm vào `dgCart` (chỉ là 1 `List<>` tạm thời).
    * Sự kiện `btnConfirm_Click` (Nút "Xác nhận Order"):
        * Duyệt `List<>` tạm thời (trong `dgCart`).
        * Gọi `orderService.AddProductToInvoice(...)` cho từng món.
        * `this.Close();`

**4. Kết nối từ `MainWindow`:**

* Sửa `Table_Click` trong `MainWindow.xaml.cs` (Task 02):
* Khi `if (table.Status == "Free")` và Yes:
    * `var invoice = tableService.StartSession(table.ID, currentEmployee.ID);` (Lưu `currentEmployee` khi đăng nhập).
    * `LoadTableMap();`
    * `OrderWindow orderPopup = new OrderWindow(invoice);`
    * `orderPopup.ShowDialog();`
* Khi `if (table.Status == "InUse")`:
    * (Cần logic lấy `Invoice` đang "Active" của bàn đó).
    * `OrderWindow orderPopup = new OrderWindow(activeInvoice);`
    * `orderPopup.ShowDialog();`

