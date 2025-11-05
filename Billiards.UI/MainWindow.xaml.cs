using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

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
            var tables = _tableService.GetTableMap();
            icTableMap.ItemsSource = tables;
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
                // (Sẽ gọi Module 3 - Order)
                // Tạm thời: Cập nhật trạng thái bàn
                try
                {
                    _tableService.UpdateTableStatus(table.ID, "InUse");
                    LoadTableMap(); // Tải lại sơ đồ bàn
                    MessageBox.Show($"Đã mở Bàn {table.TableName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật trạng thái bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else if (table.Status == "InUse")
        {
            // Hiển thị thông tin chi tiết (sẽ gọi Module 3 và 4)
            // Tạm thời: Hiển thị thông báo
            MessageBox.Show(
                $"Bàn {table.TableName} đang được sử dụng.\n\n(Sẽ hiển thị chi tiết Order và Thanh toán)",
                "Thông tin",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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
}
