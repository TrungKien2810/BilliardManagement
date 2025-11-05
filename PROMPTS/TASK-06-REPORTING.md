### YÊU CẦU NHIỆM VỤ 06: BÁO CÁO DOANH THU

**Người thực hiện:** Team Lead

**Mục tiêu:** Xây dựng màn hình báo cáo doanh thu đơn giản theo khoảng thời gian.

**Nhiệm vụ:**

**1. Tạo Logic Báo cáo (`BLL` và `DAL`):**

* **Trong `Billiards.DAL`:**
    * Tạo `ReportRepository.cs`.
    * Tạo phương thức `public List<Invoice> GetPaidInvoices(DateTime startDate, DateTime endDate)`:
        * Dùng LINQ: `_context.Invoices`
        * `.Where(i => i.Status == "Paid" && i.EndTime >= startDate && i.EndTime <= endDate)`
        * `.Include(i => i.InvoiceDetails)` (Để lấy chi tiết)
        * `.ToList()`.

* **Trong `Billiards.BLL`:**
    * Tạo `ReportService.cs`.
    * Tạo phương thức `public List<Invoice> GetRevenueReport(DateTime startDate, DateTime endDate)` (gọi `ReportRepository`).
    * (Nâng cao): Tạo 1 ViewModel `RevenueReportViewModel` để nhóm dữ liệu (ví dụ: Tổng doanh thu, Tổng tiền giờ, Tổng tiền dịch vụ...).

**2. Xây dựng Giao diện Báo cáo (`Billiards.UI`):**

* Tạo 1 `UserControl` mới tên là `ReportsView.xaml`.
* **Thiết kế Layout:**
    * Phía trên: 2 `DatePicker` (`dpStartDate`, `dpEndDate`) và 1 `Button` (`btnRunReport`).
    * Khu vực chính: 1 `DataGrid` (`dgReports`) để hiển thị danh sách các hóa đơn đã thanh toán.
    * Phía dưới: Các `TextBlock` để hiển thị Tóm tắt (Tổng Doanh thu, Tổng Giờ, Tổng Dịch vụ).

**3. Viết Code-Behind (`ReportsView.xaml.cs`):**

* **Sự kiện `btnRunReport_Click`:**
    * Lấy `startDate` và `endDate` từ `DatePicker`.
    * Gọi `ReportService`: `var reportData = reportService.GetRevenueReport(startDate, endDate);`
    * Gán `dgReports.ItemsSource = reportData;`
    * Tính toán các số liệu tóm tắt (SUM của `TotalAmount`, `TableFee`...) và hiển thị lên các `TextBlock`.

**4. Tích hợp vào `MainWindow`:**

* Thêm `MenuItem` "Báo cáo Doanh thu" (chỉ Admin thấy).
* Khi bấm, hiển thị `ReportsView.xaml` vào khu vực nội dung.

