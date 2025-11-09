using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;
using Billiards.DAL.Repositories;
using Billiards.UI.Windows;
using Billiards.UI.Views;

namespace Billiards.UI;

// Helper class for "All Areas" option
public class AreaFilterItem
{
    public int ID { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public bool IsAll { get; set; }
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly TableService _tableService;
    private readonly AreaService _areaService;

    public MainWindow()
    {
        InitializeComponent();
        _tableService = new TableService();
        _areaService = new AreaService();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Kiểm tra session
        if (!SessionManager.Instance.IsLoggedIn)
        {
            MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            OpenLoginWindow();
            return;
        }

        // Hiển thị thông tin người dùng
        UpdateUserInfo();

        // Phân quyền menu
        UpdateMenuVisibility();

        LoadAreas();
        LoadTableMap();
    }

    private void UpdateUserInfo()
    {
        var session = SessionManager.Instance;
        if (session.CurrentEmployee != null)
        {
            txtUserInfo.Text = $"Xin chào, {session.CurrentEmployee.FullName} - {session.CurrentAccount?.Role}";
        }
        else if (session.CurrentAccount != null)
        {
            txtUserInfo.Text = $"Xin chào, {session.CurrentAccount.Username} - {session.CurrentAccount.Role}";
        }
    }

    private void UpdateMenuVisibility()
    {
        // Chỉ hiển thị menu Admin nếu là Admin
        if (AuthorizationHelper.IsAdmin())
        {
            mnuAdmin.Visibility = Visibility.Visible;
            mnuReports.Visibility = Visibility.Visible;
        }
        else
        {
            mnuAdmin.Visibility = Visibility.Collapsed;
            mnuReports.Visibility = Visibility.Collapsed;
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Có thể thêm xác nhận trước khi đóng nếu cần
    }

    private void LoadAreas()
    {
        try
        {
            // Create a new service instance to ensure fresh data from database
            var areaService = new AreaService();
            var areas = areaService.GetAllAreas();
            
            // Create a list with "Tất cả" option at the beginning
            var areaList = new List<AreaFilterItem>
            {
                new AreaFilterItem { ID = 0, AreaName = "Tất cả", IsAll = true }
            };
            
            // Add real areas
            foreach (var area in areas)
            {
                areaList.Add(new AreaFilterItem { ID = area.ID, AreaName = area.AreaName, IsAll = false });
            }
            
            // Clear and set ItemsSource to force UI refresh
            lbAreas.ItemsSource = null;
            lbAreas.ItemsSource = areaList;
            
            // Select "Tất cả" by default
            if (areaList.Count > 0)
            {
                lbAreas.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách khu vực: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadTableMap()
    {
        try
        {
            // Create a new service instance to ensure fresh data from database
            var tableService = new TableService();
            var tables = tableService.GetTableMap();
            
            // Clear and set ItemsSource to force UI refresh
            icTableMap.ItemsSource = null;
            icTableMap.ItemsSource = tables;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải sơ đồ bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadTableMapByArea(int areaId)
    {
        try
        {
            // Create a new service instance to ensure fresh data from database
            var tableService = new TableService();
            var tables = tableService.GetTableMapByArea(areaId);
            
            // Clear and set ItemsSource to force UI refresh
            icTableMap.ItemsSource = null;
            icTableMap.ItemsSource = tables;
            
            // Force refresh UI
            icTableMap.UpdateLayout();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải sơ đồ bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void lbAreas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = lbAreas.SelectedItem as AreaFilterItem;
        if (selectedItem == null)
        {
            return;
        }

        if (selectedItem.IsAll)
        {
            // Show all tables
            LoadTableMap();
        }
        else
        {
            // Filter by area
            LoadTableMapByArea(selectedItem.ID);
        }
    }

    private void Table_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        var table = button.DataContext as Table;
        if (table == null) return;

        if (table.Status == "Free")
        {
            var result = MessageBox.Show(
                $"Bạn muốn mở Bàn {table.TableName}?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Lấy employee ID từ session
                    var session = SessionManager.Instance;
                    if (session.CurrentEmployee == null || session.CurrentEmployee.ID == 0)
                    {
                        MessageBox.Show("Không thể xác định nhân viên. Vui lòng đăng nhập lại.", 
                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Start session và tạo invoice mới
                    var invoice = _tableService.StartSession(table.ID, session.CurrentEmployee.ID);
                    
                    // Reload table map để cập nhật trạng thái bàn
                    RefreshCurrentTableMap();
                    
                    // Mở OrderWindow
                    var orderWindow = new OrderWindow(invoice);
                    orderWindow.ShowDialog();
                    
                    // Reload table map sau khi đóng order window
                    RefreshCurrentTableMap();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else if (table.Status == "InUse")
        {
            // Mặc định mở OrderWindow khi left-click
            // Right-click sẽ hiển thị ContextMenu với các tùy chọn
            OpenOrderWindow(table);
        }
        else if (table.Status == "Reserved")
        {
            MessageBox.Show(
                $"Bàn {table.TableName} đã được đặt trước.",
                "Thông tin",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        else if (table.Status == "Maintenance")
        {
            MessageBox.Show(
                $"Bàn {table.TableName} đang bảo trì.",
                "Thông tin",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void MenuItem_Logout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Bạn có chắc chắn muốn đăng xuất?",
            "Xác nhận đăng xuất",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            SessionManager.Instance.Logout();
            OpenLoginWindow();
        }
    }

    private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Bạn có chắc chắn muốn thoát ứng dụng?",
            "Xác nhận thoát",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }

    private void OpenLoginWindow()
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        this.Close();
    }

    private void ShowAdminView(UserControl view)
    {
        gridTableMap.Visibility = Visibility.Collapsed;
        contentAdminViews.Visibility = Visibility.Visible;
        contentAdminViews.Content = view;
    }

    private void ShowTableMap()
    {
        gridTableMap.Visibility = Visibility.Visible;
        contentAdminViews.Visibility = Visibility.Collapsed;
        contentAdminViews.Content = null;
        
        // Force reload data from database when returning to home
        RefreshTableMapData();
    }

    private void RefreshTableMapData()
    {
        // Save current area selection
        var currentSelection = lbAreas.SelectedItem as AreaFilterItem;
        int? selectedAreaId = currentSelection?.IsAll == false ? currentSelection.ID : null;
        bool wasAllSelected = currentSelection?.IsAll == true;
        
        // Clear existing data to force refresh
        icTableMap.ItemsSource = null;
        
        // Reload areas from database (we're already on UI thread)
        LoadAreas();
        
        // Restore selection or default to "Tất cả"
        if (wasAllSelected || selectedAreaId == null)
        {
            // Select "Tất cả" (first item)
            if (lbAreas.Items.Count > 0)
            {
                lbAreas.SelectedIndex = 0;
            }
            LoadTableMap();
        }
        else
        {
            // Find and select the previously selected area
            var areaList = lbAreas.ItemsSource as List<AreaFilterItem>;
            if (areaList != null)
            {
                var areaToSelect = areaList.FirstOrDefault(a => a.ID == selectedAreaId && !a.IsAll);
                if (areaToSelect != null)
                {
                    lbAreas.SelectedItem = areaToSelect;
                }
                else
                {
                    // If area not found, select "Tất cả"
                    lbAreas.SelectedIndex = 0;
                    LoadTableMap();
                }
            }
        }
    }

    private void MenuItem_ProductManagement_Click(object sender, RoutedEventArgs e)
    {
        if (!AuthorizationHelper.IsAdmin())
        {
            MessageBox.Show("Bạn không có quyền truy cập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ShowAdminView(new ProductManagementView());
    }

    private void MenuItem_TableManagement_Click(object sender, RoutedEventArgs e)
    {
        if (!AuthorizationHelper.IsAdmin())
        {
            MessageBox.Show("Bạn không có quyền truy cập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ShowAdminView(new TableManagementView());
    }

    private void MenuItem_EmployeeManagement_Click(object sender, RoutedEventArgs e)
    {
        if (!AuthorizationHelper.IsAdmin())
        {
            MessageBox.Show("Bạn không có quyền truy cập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ShowAdminView(new EmployeeManagementView());
    }

    private void MenuItem_CustomerManagement_Click(object sender, RoutedEventArgs e)
    {
        if (!AuthorizationHelper.IsAdmin())
        {
            MessageBox.Show("Bạn không có quyền truy cập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ShowAdminView(new CustomerManagementView());
    }

    private void MenuItem_PricingManagement_Click(object sender, RoutedEventArgs e)
    {
        if (!AuthorizationHelper.IsAdmin())
        {
            MessageBox.Show("Bạn không có quyền truy cập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ShowAdminView(new PricingManagementView());
    }

    private void MenuItem_Home_Click(object sender, RoutedEventArgs e)
    {
        ShowTableMap();
    }

    private Table? _currentContextTable;

    private void Table_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // Kiểm tra status của bàn, chỉ hiển thị context menu khi status là "InUse"
        var button = sender as Button;
        if (button != null)
        {
            var table = button.DataContext as Table;
            if (table == null || table.Status != "InUse")
            {
                e.Handled = true; // Ẩn context menu
                _currentContextTable = null;
            }
            else
            {
                // Lưu table hiện tại để sử dụng trong menu items
                _currentContextTable = table;
            }
        }
    }

    private Table? GetTableFromMenuItem(object sender)
    {
        // Lấy Table từ ContextMenu -> PlacementTarget (Button) -> DataContext
        try
        {
            if (sender is MenuItem menuItem)
            {
                // Cách 1: Từ PlacementTarget của ContextMenu
                var contextMenu = menuItem.Parent as ContextMenu;
                if (contextMenu?.PlacementTarget is FrameworkElement fe)
                {
                    // Tìm Button trong visual tree
                    var button = fe as Button ?? FindVisualParent<Button>(fe);
                    if (button != null)
                    {
                        return button.DataContext as Table;
                    }
                }
                
                // Cách 2: Từ Tag của MenuItem (backup)
                if (menuItem.Tag is Table tableFromTag)
                {
                    return tableFromTag;
                }
            }
        }
        catch
        {
            // Fallback: không làm gì
        }
        return null;
    }

    private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        
        if (parentObject is T parent)
        {
            return parent;
        }
        else
        {
            return FindVisualParent<T>(parentObject);
        }
    }

    private void MenuItem_OrderMore_Click(object sender, RoutedEventArgs e)
    {
        // Ưu tiên sử dụng _currentContextTable, nếu không có thì lấy từ MenuItem
        var table = _currentContextTable ?? GetTableFromMenuItem(sender);
        if (table == null) return;
        
        _currentContextTable = null; // Clear sau khi sử dụng

        try
        {
            var invoiceRepository = new InvoiceRepository();
            var activeInvoice = invoiceRepository.GetActiveInvoiceByTable(table.ID);
            
            if (activeInvoice != null)
            {
                var orderWindow = new OrderWindow(activeInvoice);
                orderWindow.ShowDialog();
                
                // Reload table map after closing order window
                RefreshCurrentTableMap();
            }
            else
            {
                MessageBox.Show(
                    $"Bàn {table.TableName} đang được sử dụng nhưng không tìm thấy hóa đơn.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MenuItem_Checkout_Click(object sender, RoutedEventArgs e)
    {
        // Ưu tiên sử dụng _currentContextTable, nếu không có thì lấy từ MenuItem
        var table = _currentContextTable ?? GetTableFromMenuItem(sender);
        if (table == null) return;
        
        _currentContextTable = null; // Clear sau khi sử dụng

        try
        {
            // Mở CheckoutWindow
            var checkoutWindow = new CheckoutWindow(table.ID);
            if (checkoutWindow.ShowDialog() == true)
            {
                // Thanh toán thành công, reload table map
                RefreshCurrentTableMap();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi thanh toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenOrderWindow(Table table)
    {
        try
        {
            var invoiceRepository = new InvoiceRepository();
            var activeInvoice = invoiceRepository.GetActiveInvoiceByTable(table.ID);
            
            if (activeInvoice != null)
            {
                var orderWindow = new OrderWindow(activeInvoice);
                orderWindow.ShowDialog();
                
                // Reload table map after closing order window
                RefreshCurrentTableMap();
            }
            else
            {
                MessageBox.Show(
                    $"Bàn {table.TableName} đang được sử dụng nhưng không tìm thấy hóa đơn.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshCurrentTableMap()
    {
        // Reload table map based on current area selection
        var selectedItem = lbAreas.SelectedItem as AreaFilterItem;
        if (selectedItem != null)
        {
            if (selectedItem.IsAll)
            {
                LoadTableMap();
            }
            else
            {
                LoadTableMapByArea(selectedItem.ID);
            }
        }
        else
        {
            LoadTableMap();
        }
    }
}
