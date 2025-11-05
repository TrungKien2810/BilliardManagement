using System.Windows;
using Billiards.BLL.Services;
using Billiards.UI;

namespace Billiards.UI
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text;
            var password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var authService = new AuthService();
                var account = authService.Login(username, password);

                if (account != null)
                {
                    // Lưu session
                    SessionManager.Instance.SetSession(account, account.Employee);

                    // Mở MainWindow
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    
                    // Đóng LoginWindow
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Sai thông tin đăng nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtPassword.Password = string.Empty;
                    txtUsername.Focus();
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                string errorMessage = "Không thể kết nối đến SQL Server!\n\n";
                errorMessage += "Vui lòng kiểm tra:\n";
                errorMessage += "1. SQL Server đã được cài đặt và đang chạy\n";
                errorMessage += "2. Database 'BilliardsDB' đã được tạo (chạy file CreateDatabase.sql)\n";
                errorMessage += "3. Connection string trong appsettings.json đúng với instance SQL Server của bạn\n\n";
                errorMessage += $"Chi tiết lỗi: {sqlEx.Message}";
                
                MessageBox.Show(errorMessage, "Lỗi kết nối Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

