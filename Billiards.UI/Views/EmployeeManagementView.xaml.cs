using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Views;

public partial class EmployeeManagementView : UserControl
{
    private readonly EmployeeService _employeeService;
    private Employee? _selectedEmployee;
    private Account? _selectedAccount;

    public EmployeeManagementView()
    {
        InitializeComponent();
        _employeeService = new EmployeeService();
        Loaded += EmployeeManagementView_Loaded;
    }

    private void EmployeeManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        LoadEmployees();
        LoadAccounts();
        LoadEmployeesForAccountCombo();
        // Khởi tạo UI về Create Mode khi load lần đầu
        UpdateUIForEmployeeCreateMode();
        UpdateUIForAccountCreateMode();
    }

    #region Employees Management

    private void LoadEmployees()
    {
        try
        {
            var employees = _employeeService.GetAllEmployees();
            dgEmployees.ItemsSource = employees;
            dgEmployees.SelectedItem = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadEmployeesForAccountCombo()
    {
        try
        {
            var employees = _employeeService.GetAllEmployees();
            cmbEmployeeForAccount.ItemsSource = employees;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedEmployee = dgEmployees.SelectedItem as Employee;
        if (_selectedEmployee != null)
        {
            // 1. Đổ dữ liệu từ nhân viên được chọn vào form
            txtFullName.Text = _selectedEmployee.FullName;
            txtPhoneNumber.Text = _selectedEmployee.PhoneNumber ?? string.Empty;
            txtAddress.Text = _selectedEmployee.Address ?? string.Empty;
            
            // Check if employee has account
            var account = _employeeService.GetAccountByEmployeeId(_selectedEmployee.ID);
            if (account != null)
            {
                chkHasAccount.IsChecked = true;
                gridAccount.IsEnabled = true;
                txtUsername.Text = account.Username;
                txtPassword.Password = string.Empty; // Don't show password
                var roleItem = cmbRole.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == account.Role);
                if (roleItem != null)
                    cmbRole.SelectedItem = roleItem;
            }
            else
            {
                chkHasAccount.IsChecked = false;
                gridAccount.IsEnabled = false;
                txtUsername.Text = string.Empty;
                txtPassword.Password = string.Empty;
                cmbRole.SelectedIndex = -1;
            }

            // 2. Thay đổi trạng thái nút (Chuyển sang Edit Mode)
            btnSaveEmployee.Content = "Cập nhật";
            btnDeleteEmployee.Visibility = Visibility.Visible;
        }
        else
        {
            chkHasAccount.IsChecked = false;
            gridAccount.IsEnabled = false;
            // Không có nhân viên nào được chọn, chuyển về Create Mode
            UpdateUIForEmployeeCreateMode();
        }
    }
    
    private void chkHasAccount_Checked(object sender, RoutedEventArgs e)
    {
        gridAccount.IsEnabled = true;
    }
    
    private void chkHasAccount_Unchecked(object sender, RoutedEventArgs e)
    {
        gridAccount.IsEnabled = false;
        txtUsername.Text = string.Empty;
        txtPassword.Password = string.Empty;
        cmbRole.SelectedIndex = -1;
    }

    private void btnAddNewEmployee_Click(object sender, RoutedEventArgs e)
    {
        // Chuyển sang Create Mode
        _selectedEmployee = null;
        dgEmployees.SelectedItem = null;

        // 1. Xóa trắng Form
        ClearEmployeeForm();

        // 2. Cập nhật UI cho Create Mode
        UpdateUIForEmployeeCreateMode();

        // 3. Tự động focus vào ô đầu tiên
        txtFullName.Focus();
    }

    private void ClearEmployeeForm()
    {
        txtFullName.Text = string.Empty;
        txtPhoneNumber.Text = string.Empty;
        txtAddress.Text = string.Empty;
        chkHasAccount.IsChecked = false;
        gridAccount.IsEnabled = false;
        txtUsername.Text = string.Empty;
        txtPassword.Password = string.Empty;
        cmbRole.SelectedIndex = -1;
    }

    private void UpdateUIForEmployeeCreateMode()
    {
        btnSaveEmployee.Content = "Lưu mới";
        btnDeleteEmployee.Visibility = Visibility.Collapsed;
    }

    private void btnSaveEmployee_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Logic chính: Kiểm tra _selectedEmployee để xác định Create hay Update
            if (_selectedEmployee == null)
            {
                // CREATE MODE: Tạo nhân viên mới
                var newEmployee = new Employee
                {
                    FullName = txtFullName.Text.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim()
                };
                _employeeService.AddEmployee(newEmployee);

                // Handle account if checkbox is checked
                if (chkHasAccount.IsChecked == true && gridAccount.IsEnabled)
                {
                    var role = (cmbRole.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Cashier";
                    
                    if (string.IsNullOrWhiteSpace(txtUsername.Text))
                    {
                        MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    if (string.IsNullOrWhiteSpace(txtPassword.Password))
                    {
                        MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Check if username already exists
                    var usernameExists = _employeeService.GetAccountByUsername(txtUsername.Text);
                    if (usernameExists != null)
                    {
                        MessageBox.Show("Tên đăng nhập đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    var newAccount = new Account
                    {
                        Username = txtUsername.Text.Trim(),
                        Password = txtPassword.Password, // In production, hash this
                        EmployeeID = newEmployee.ID,
                        Role = role
                    };
                    _employeeService.AddAccount(newAccount);
                }

                MessageBox.Show("✅ Thêm nhân viên mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // UPDATE MODE: Cập nhật nhân viên hiện tại
                _selectedEmployee.FullName = txtFullName.Text.Trim();
                _selectedEmployee.PhoneNumber = string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text.Trim();
                _selectedEmployee.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();
                _employeeService.UpdateEmployee(_selectedEmployee);

                // Handle account if checkbox is checked
                if (chkHasAccount.IsChecked == true && gridAccount.IsEnabled)
                {
                    var existingAccount = _employeeService.GetAccountByEmployeeId(_selectedEmployee.ID);
                    var role = (cmbRole.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Cashier";
                    
                    if (string.IsNullOrWhiteSpace(txtUsername.Text))
                    {
                        MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    if (existingAccount != null)
                    {
                        // Update existing account
                        existingAccount.Username = txtUsername.Text.Trim();
                        if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                        {
                            existingAccount.Password = txtPassword.Password; // In production, hash this
                        }
                        existingAccount.Role = role;
                        _employeeService.UpdateAccount(existingAccount);
                    }
                    else
                    {
                        // Create new account
                        if (string.IsNullOrWhiteSpace(txtPassword.Password))
                        {
                            MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        
                        // Check if username already exists
                        var usernameExists = _employeeService.GetAccountByUsername(txtUsername.Text);
                        if (usernameExists != null)
                        {
                            MessageBox.Show("Tên đăng nhập đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        
                        var newAccount = new Account
                        {
                            Username = txtUsername.Text.Trim(),
                            Password = txtPassword.Password, // In production, hash this
                            EmployeeID = _selectedEmployee.ID,
                            Role = role
                        };
                        _employeeService.AddAccount(newAccount);
                    }
                }
                else if (chkHasAccount.IsChecked == false)
                {
                    // Delete account if exists
                    var existingAccount = _employeeService.GetAccountByEmployeeId(_selectedEmployee.ID);
                    if (existingAccount != null)
                    {
                        _employeeService.DeleteAccount(existingAccount.Username);
                    }
                }

                MessageBox.Show("✅ Cập nhật nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadEmployees();
            LoadAccounts();
            LoadEmployeesForAccountCombo();
            btnAddNewEmployee_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnDeleteEmployee_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedEmployee == null)
        {
            MessageBox.Show("Vui lòng chọn nhân viên cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa nhân viên \"{_selectedEmployee.FullName}\"?\n\nLưu ý: Nếu nhân viên này có tài khoản, tài khoản cũng sẽ bị xóa.",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Delete account first if exists
                var account = _employeeService.GetAccountByEmployeeId(_selectedEmployee.ID);
                if (account != null)
                {
                    _employeeService.DeleteAccount(account.Username);
                }

                _employeeService.DeleteEmployee(_selectedEmployee.ID);
                MessageBox.Show("✅ Xóa nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadEmployees();
                LoadAccounts();
                LoadEmployeesForAccountCombo();
                btnAddNewEmployee_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi xóa nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion

    #region Accounts Management

    private void LoadAccounts()
    {
        try
        {
            var accounts = _employeeService.GetAllAccounts();
            dgAccounts.ItemsSource = accounts;
            dgAccounts.SelectedItem = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedAccount = dgAccounts.SelectedItem as Account;
        if (_selectedAccount != null)
        {
            // 1. Đổ dữ liệu từ tài khoản được chọn vào form
            cmbEmployeeForAccount.SelectedValue = _selectedAccount.EmployeeID;
            txtAccountUsername.Text = _selectedAccount.Username;
            
            // Hiển thị mật khẩu - thử từ object hiện tại trước, nếu không có thì load từ DB
            if (!string.IsNullOrEmpty(_selectedAccount.Password))
            {
                // Password đã có trong object từ DataGrid
                txtAccountPasswordDisplay.Text = _selectedAccount.Password;
            }
            else
            {
                // Load lại từ DB để lấy password
                try
                {
                    var accountFromDb = _employeeService.GetAccountByUsername(_selectedAccount.Username);
                    if (accountFromDb != null && !string.IsNullOrEmpty(accountFromDb.Password))
                    {
                        txtAccountPasswordDisplay.Text = accountFromDb.Password;
                        // Cập nhật password trong _selectedAccount để DataGrid cũng hiển thị
                        _selectedAccount.Password = accountFromDb.Password;
                    }
                    else
                    {
                        txtAccountPasswordDisplay.Text = string.Empty;
                    }
                }
                catch
                {
                    txtAccountPasswordDisplay.Text = string.Empty;
                }
            }
            
            txtAccountPassword.Password = string.Empty; // Clear password input for new password
            
            // Set role
            var roleItem = cmbAccountRole.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == _selectedAccount.Role);
            if (roleItem != null)
                cmbAccountRole.SelectedItem = roleItem;

            // 2. Thay đổi trạng thái nút (Chuyển sang Edit Mode)
            btnSaveAccount.Content = "Cập nhật";
            btnDeleteAccount.Visibility = Visibility.Visible;
        }
        else
        {
            txtAccountPasswordDisplay.Text = string.Empty;
            // Không có tài khoản nào được chọn, chuyển về Create Mode
            UpdateUIForAccountCreateMode();
        }
    }

    private void btnAddNewAccount_Click(object sender, RoutedEventArgs e)
    {
        // Chuyển sang Create Mode
        _selectedAccount = null;
        dgAccounts.SelectedItem = null;

        // 1. Xóa trắng Form
        ClearAccountForm();

        // 2. Cập nhật UI cho Create Mode
        UpdateUIForAccountCreateMode();

        // 3. Tự động focus vào ô đầu tiên
        cmbEmployeeForAccount.Focus();
    }

    private void ClearAccountForm()
    {
        cmbEmployeeForAccount.SelectedIndex = -1;
        txtAccountUsername.Text = string.Empty;
        txtAccountPasswordDisplay.Text = string.Empty;
        txtAccountPassword.Password = string.Empty;
        cmbAccountRole.SelectedIndex = -1;
    }

    private void UpdateUIForAccountCreateMode()
    {
        btnSaveAccount.Content = "Lưu mới";
        btnDeleteAccount.Visibility = Visibility.Collapsed;
    }

    private void btnSaveAccount_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cmbEmployeeForAccount.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAccountUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbAccountRole.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn vai trò!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var role = (cmbAccountRole.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Cashier";

            // Logic chính: Kiểm tra _selectedAccount để xác định Create hay Update
            if (_selectedAccount == null)
            {
                // CREATE MODE: Tạo tài khoản mới
                // Check if username already exists
                var existingAccount = _employeeService.GetAccountByUsername(txtAccountUsername.Text);
                if (existingAccount != null)
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if employee already has account
                var employeeId = (int)cmbEmployeeForAccount.SelectedValue;
                var existingEmployeeAccount = _employeeService.GetAccountByEmployeeId(employeeId);
                if (existingEmployeeAccount != null)
                {
                    MessageBox.Show("Nhân viên này đã có tài khoản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtAccountPassword.Password))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Add new account
                var newAccount = new Account
                {
                    Username = txtAccountUsername.Text.Trim(),
                    Password = txtAccountPassword.Password, // In production, hash this
                    EmployeeID = employeeId,
                    Role = role
                };
                _employeeService.AddAccount(newAccount);
                MessageBox.Show("✅ Thêm tài khoản mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // UPDATE MODE: Cập nhật tài khoản hiện tại
                _selectedAccount.Username = txtAccountUsername.Text.Trim();
                if (!string.IsNullOrWhiteSpace(txtAccountPassword.Password))
                {
                    _selectedAccount.Password = txtAccountPassword.Password; // In production, hash this
                }
                _selectedAccount.EmployeeID = (int)cmbEmployeeForAccount.SelectedValue;
                _selectedAccount.Role = role;
                _employeeService.UpdateAccount(_selectedAccount);
                MessageBox.Show("✅ Cập nhật tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadAccounts();
            LoadEmployees();
            btnAddNewAccount_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnDeleteAccount_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAccount == null)
        {
            MessageBox.Show("Vui lòng chọn tài khoản cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa tài khoản \"{_selectedAccount.Username}\"?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _employeeService.DeleteAccount(_selectedAccount.Username);
                MessageBox.Show("✅ Xóa tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAccounts();
                LoadEmployees();
                btnAddNewAccount_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi xóa tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion
}



