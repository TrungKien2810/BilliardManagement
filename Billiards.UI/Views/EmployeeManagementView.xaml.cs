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
    }

    #region Employees Management

    private void LoadEmployees()
    {
        try
        {
            var employees = _employeeService.GetAllEmployees();
            dgEmployees.ItemsSource = employees;
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
            txtFullName.Text = _selectedEmployee.FullName;
            txtPhoneNumber.Text = _selectedEmployee.PhoneNumber ?? string.Empty;
            txtAddress.Text = _selectedEmployee.Address ?? string.Empty;
        }
    }

    private void btnAddNewEmployee_Click(object sender, RoutedEventArgs e)
    {
        _selectedEmployee = null;
        txtFullName.Text = string.Empty;
        txtPhoneNumber.Text = string.Empty;
        txtAddress.Text = string.Empty;
        dgEmployees.SelectedItem = null;
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

            if (_selectedEmployee != null)
            {
                // Update existing employee
                _selectedEmployee.FullName = txtFullName.Text;
                _selectedEmployee.PhoneNumber = string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text;
                _selectedEmployee.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text;
                _employeeService.UpdateEmployee(_selectedEmployee);

                MessageBox.Show("Cập nhật nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new employee
                var newEmployee = new Employee
                {
                    FullName = txtFullName.Text,
                    PhoneNumber = string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text,
                    Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text
                };
                _employeeService.AddEmployee(newEmployee);

                MessageBox.Show("Thêm nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadEmployees();
            LoadAccounts();
            LoadEmployeesForAccountCombo();
            btnAddNewEmployee_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Xóa nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadEmployees();
                LoadAccounts();
                LoadEmployeesForAccountCombo();
                btnAddNewEmployee_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
        }
        else
        {
            txtAccountPasswordDisplay.Text = string.Empty;
        }
    }

    private void btnAddNewAccount_Click(object sender, RoutedEventArgs e)
    {
        _selectedAccount = null;
        cmbEmployeeForAccount.SelectedIndex = -1;
        txtAccountUsername.Text = string.Empty;
        txtAccountPasswordDisplay.Text = string.Empty;
        txtAccountPassword.Password = string.Empty;
        cmbAccountRole.SelectedIndex = -1;
        dgAccounts.SelectedItem = null;
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

            if (_selectedAccount != null)
            {
                // Update existing account
                _selectedAccount.Username = txtAccountUsername.Text;
                if (!string.IsNullOrWhiteSpace(txtAccountPassword.Password))
                {
                    _selectedAccount.Password = txtAccountPassword.Password; // In production, hash this
                }
                _selectedAccount.EmployeeID = (int)cmbEmployeeForAccount.SelectedValue;
                _selectedAccount.Role = role;
                _employeeService.UpdateAccount(_selectedAccount);
                MessageBox.Show("Cập nhật tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
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
                    Username = txtAccountUsername.Text,
                    Password = txtAccountPassword.Password, // In production, hash this
                    EmployeeID = employeeId,
                    Role = role
                };
                _employeeService.AddAccount(newAccount);
                MessageBox.Show("Thêm tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadAccounts();
            LoadEmployees();
            btnAddNewAccount_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Xóa tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAccounts();
                LoadEmployees();
                btnAddNewAccount_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion
}



