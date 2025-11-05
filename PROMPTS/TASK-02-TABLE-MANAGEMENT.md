### YÊU CẦU NHIỆM VỤ 02: QUẢN LÝ BÀN (MAIN DASHBOARD)

**Người thực hiện:** Member 2

**Mục tiêu:** Xây dựng màn hình Sơ đồ Bàn chính (Main Dashboard), hiển thị trạng thái và cho phép bắt đầu/chi tiết bàn.

**Nhiệm vụ:**

**1. Tạo Logic Lấy Dữ liệu Bàn (`BLL` và `DAL`):**

* **Trong `Billiards.DAL`:**
    * Tạo `TableRepository.cs`.
    * Tạo phương thức `public List<Table> GetAllTables()` để lấy tất cả bàn (có thể `Include` cả `Area` và `TableType`).
    * Tạo phương thức `public void UpdateTableStatus(int tableId, string newStatus)` (Dùng để đổi trạng thái bàn sang "InUse" hoặc "Free").

* **Trong `Billiards.BLL`:**
    * Tạo `TableService.cs`.
    * Tạo phương thức `public List<Table> GetTableMap()` (gọi `TableRepository.GetAllTables()`).

**2. Thiết kế Giao diện `MainWindow.xaml` (`Billiards.UI`):**

* Mở `MainWindow.xaml` (do Team Lead tạo).
* **Bố cục (Layout):**
    * Bên trái (Sidebar): Dùng 1 `StackPanel` hoặc `ListBox` để hiển thị danh sách `Areas` (Tầng 1, Tầng 2, VIP...). (Sẽ làm chức năng lọc sau).
    * Khu vực chính (Center): Dùng 1 `ItemsControl` tên là `icTableMap`.

* **Thiết kế `ItemsControl` (Sơ đồ bàn):**
    * Thay đổi `ItemsControl.ItemsPanel` thành `WrapPanel` để các bàn tự động xếp hàng.
    * Tạo `ItemsControl.ItemTemplate` (Một `DataTemplate`).
    * Bên trong `DataTemplate`, dùng 1 `Button` (hoặc `Border` bọc `TextBlock`) để hiển thị mỗi bàn.
        * Nội dung `Button`: Hiển thị `TableName` (ví dụ: "Bàn 01").
        * Kích thước (ví dụ: `Width="100"`, `Height="80"`, `Margin="5"`).

**3. Binding Dữ liệu và Màu sắc Trạng thái:**

* **Trong `MainWindow.xaml.cs` (code-behind):**
    * Tạo phương thức `void LoadTableMap()`:
        * Gọi `TableService` từ BLL: `var tableService = new TableService();`
        * `icTableMap.ItemsSource = tableService.GetTableMap();`
    * Gọi `LoadTableMap()` khi cửa sổ được tải (Window_Loaded).

* **Tạo Bộ chuyển đổi (Converter):**
    * Tạo 1 class `StatusToBrushConverter.cs` (kế thừa `IValueConverter`).
    * Logic `Convert()`:
        * `if (value == "Free") return Brushes.Green;`
        * `if (value == "InUse") return Brushes.Red;`
        * `if (value == "Reserved") return Brushes.Blue;`
        * `return Brushes.Gray;`

* **Áp dụng Converter:**
    * Trong `App.xaml` (hoặc `MainWindow.Resources`), khai báo converter.
    * Trong `DataTemplate` của `Button` bàn, binding `Background` của Button:
        `Background="{Binding Status, Converter={StaticResource StatusToBrushConverter}}"`

**4. Xử lý Sự kiện Click vào Bàn:**

* Trong `DataTemplate` của `Button`, thêm sự kiện `Click="Table_Click"`.
* **Trong `MainWindow.xaml.cs`:**
    * Viết hàm `Table_Click(object sender, RoutedEventArgs e)`:
    * `var button = (Button)sender;`
    * `var table = (Table)button.DataContext;` (Lấy object `Table` đang được bind).
    * **Logic:**
        * `if (table.Status == "Free")`:
            * Hiển thị `MessageBox.Show($"Bạn muốn mở Bàn {table.TableName}?", "Xác nhận", MessageBoxButton.YesNo)`.
            * Nếu Yes:
                * (Sẽ gọi Module 3 - Order)
                * (Tạm thời): Gọi `tableService.UpdateTableStatus(table.ID, "InUse");`
                * `LoadTableMap();` (Tải lại sơ đồ bàn).
        * `else if (table.Status == "InUse")`:
            * Hiển thị thông tin chi tiết (sẽ gọi Module 3 và 4).
            * (Tạm thời): `MessageBox.Show($"Bàn {table.TableName} đang được sử dụng.");`

