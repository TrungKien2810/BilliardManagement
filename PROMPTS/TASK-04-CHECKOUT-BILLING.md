### YÊU CẦU NHIỆM VỤ 04: THANH TOÁN & HÓA ĐƠN

**Người thực hiện:** Member 2 hoặc 3

**Mục tiêu:** Xây dựng Màn hình (Popup) Thanh toán, tính tiền giờ, áp dụng giảm giá, và đóng hóa đơn.

**Nhiệm vụ:**

**1. Tạo Logic Tính tiền & Thanh toán (`BLL` và `DAL`):**

* **Trong `Billiards.DAL`:**
    * Tạo `PricingRepository.cs`:
        * Tạo `public List<HourlyPricingRule> GetRules(int tableTypeId)`.
    * Sửa `InvoiceRepository.cs`:
        * Tạo `public Invoice GetActiveInvoiceByTable(int tableId)` (Tìm `Invoice` có `Status == "Active"` và `TableID`).
        * Tạo `public List<InvoiceDetail> GetInvoiceDetails(int invoiceId)`.

* **Trong `Billiards.BLL`:**
    * Tạo `BillingService.cs`:
    * Tạo `public decimal CalculateTableFee(int invoiceId)`:
        * Lấy `Invoice` (để có `StartTime` và `TableID` -> `TableTypeID`).
        * `EndTime` = `DateTime.Now`.
        * Tổng thời gian = `EndTime - StartTime`.
        * Lấy `PricingRules` từ `PricingRepository`.
        * **Logic tính tiền phức tạp:** Dựa trên tổng thời gian và các khung giờ (`HourlyPricingRules`) để tính ra `TableFee`.
    * Tạo `public Invoice GetInvoiceForCheckout(int tableId)`:
        * Lấy `activeInvoice` từ `InvoiceRepository`.
        * Cập nhật `invoice.TableFee = CalculateTableFee(invoice.ID)`.
        * Lấy `invoice.ProductFee` (đã có từ Task 03).
        * Tính `invoice.TotalAmount = invoice.TableFee + invoice.ProductFee`.
        * Trả về `invoice`.
    * Tạo `public bool FinalizeCheckout(int invoiceId, decimal discount, int? customerId)`:
        * Lấy `Invoice`.
        * Cập nhật `Discount`, `CustomerID`, `Status="Paid"`, `EndTime=Now`, `TotalAmount` (tính lại).
        * Gọi `tableRepository.UpdateTableStatus(invoice.TableID, "Free")`.
        * Lưu `SaveChanges()`.

**2. Xây dựng Giao diện `CheckoutWindow.xaml` (`Billiards.UI`):**

* Tạo một `Window` mới tên là `CheckoutWindow.xaml`.
* **Bố cục:**
    * `TextBlock` hiển thị Giờ vào, Giờ ra, Tổng thời gian.
    * `TextBlock` hiển thị Tiền giờ (`txtTableFee`).
    * `DataGrid` (`dgProducts`) hiển thị `InvoiceDetails`.
    * `TextBlock` hiển thị Tổng tiền dịch vụ (`txtProductFee`).
    * `TextBlock` hiển thị Tổng cộng (`txtSubTotal`).
    * `TextBox` (`txtDiscount`) cho phép nhập Giảm giá.
    * `TextBox` (`txtCustomerPhone`) để tìm kiếm `Customer`.
    * `TextBlock` (lớn, in đậm) hiển thị TỔNG THANH TOÁN (`txtTotalAmount`).
    * `Button` ("Xác nhận & In Hóa đơn") (`btnCheckout`).

**3. Binding Dữ liệu và Logic `CheckoutWindow`:**

* **Trong `CheckoutWindow.xaml.cs`:**
    * Tạo biến `private Invoice currentInvoice;`
    * Constructor nhận `int tableId`: `public CheckoutWindow(int tableId)`.
    * Khi `Window_Loaded`:
        * Gọi `BillingService.GetInvoiceForCheckout(tableId)` và gán vào `currentInvoice`.
        * Binding dữ liệu của `currentInvoice` lên các `TextBlock` và `DataGrid`.
    * Sự kiện `txtDiscount_LostFocus` (hoặc `TextChanged`):
        * Tính toán lại `txtTotalAmount`.
    * Sự kiện `btnCheckout_Click`:
        * Lấy `discount` từ `txtDiscount`.
        * (Nâng cao: Lấy `customerId` từ `txtCustomerPhone` - có thể bỏ qua).
        * Gọi `billingService.FinalizeCheckout(currentInvoice.ID, discount, null)`.
        * Nếu `true`:
            * (Sau này sẽ gọi máy in).
            * `MessageBox.Show("Thanh toán thành công!");`
            * `this.Close();`

**4. Kết nối từ `MainWindow`:**

* Sửa `Table_Click` trong `MainWindow.xaml.cs` (Task 02):
* Khi `if (table.Status == "InUse")`:
    * Thay vì `MessageBox`, hiển thị 1 menu (ví dụ: `ContextMenu`) với các lựa chọn: "Order thêm" (mở Task 03) và "Thanh toán".
    * Khi bấm "Thanh toán":
        * `CheckoutWindow checkout = new CheckoutWindow(table.ID);`
        * `checkout.ShowDialog();`
        * `LoadTableMap();` (Tải lại Sơ đồ bàn, bàn này sẽ chuyển sang màu xanh).

