using System;
using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;
using Billiards.DAL.Repositories;
using Billiards.UI.Windows;

namespace Billiards.UI;

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
            var areas = _areaService.GetAllAreas();
            lbAreas.ItemsSource = areas;
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
            // Clear ItemsSource trước để force reload
            icTableMap.ItemsSource = null;
            
            var tables = _tableService.GetTableMap();
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
        // Chức năng lọc theo Area sẽ được thực hiện sau
        // Tạm thời load lại toàn bộ bàn
        LoadTableMap();
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
                    LoadTableMap(); // Tải lại sơ đồ bàn
                    
                    // Mở OrderWindow
                    var orderWindow = new OrderWindow(invoice);
                    orderWindow.ShowDialog();
                    
                    // Reload table map sau khi đóng order window
                    LoadTableMap();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else if (table.Status == "InUse")
        {
            // Hiển thị ContextMenu với 2 lựa chọn: Order thêm và Thanh toán
            // ContextMenu sẽ được hiển thị tự động khi right-click
            // Left-click sẽ mặc định mở OrderWindow
            try
            {
                var invoiceRepository = new InvoiceRepository();
                var activeInvoice = invoiceRepository.GetActiveInvoiceByTable(table.ID);
                
                if (activeInvoice != null)
                {
                    // Mặc định mở OrderWindow khi left-click
                    var orderWindow = new OrderWindow(activeInvoice);
                    orderWindow.ShowDialog();
                    
                    LoadTableMap();
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

    private void Table_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // ContextMenu sẽ tự động hiển thị khi right-click
    }

    private void MenuItem_Order_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem menuItem && menuItem.Tag is Table table)
        {
            try
            {
                var invoiceRepository = new InvoiceRepository();
                var activeInvoice = invoiceRepository.GetActiveInvoiceByTable(table.ID);
                
                if (activeInvoice != null)
                {
                    var orderWindow = new OrderWindow(activeInvoice);
                    orderWindow.ShowDialog();
                    LoadTableMap();
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
    }

    private void MenuItem_Checkout_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem menuItem && menuItem.Tag is Table table)
        {
            try
            {
                var checkoutWindow = new CheckoutWindow(table.ID);
                var result = checkoutWindow.ShowDialog();
                
                if (result == true)
                {
                    // Thanh toán thành công, reload table map
                    // Đợi một chút để đảm bảo DB đã commit
                    System.Threading.Thread.Sleep(100);
                    LoadTableMap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thanh toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                // Reload lại table map ngay cả khi có lỗi
                LoadTableMap();
            }
        }
    }
}
