using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Views;

public partial class CustomerManagementView : UserControl
{
    private readonly CustomerService _customerService;
    private Customer? _selectedCustomer;

    public CustomerManagementView()
    {
        InitializeComponent();
        _customerService = new CustomerService();
        Loaded += CustomerManagementView_Loaded;
    }

    private void CustomerManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        LoadDataGrid();
        // Khởi tạo UI về Create Mode khi load lần đầu
        UpdateUIForCreateMode();
    }

    private void LoadDataGrid()
    {
        try
        {
            var customers = _customerService.GetAllCustomers();
            dgCustomers.ItemsSource = customers;
            dgCustomers.SelectedItem = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách khách hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedCustomer = dgCustomers.SelectedItem as Customer;
        if (_selectedCustomer != null)
        {
            // 1. Đổ dữ liệu từ khách hàng được chọn vào form
            txtFullName.Text = _selectedCustomer.FullName ?? string.Empty;
            txtPhoneNumber.Text = _selectedCustomer.PhoneNumber ?? string.Empty;
            txtLoyaltyPoints.Text = _selectedCustomer.LoyaltyPoints.ToString();
            ValidatePhoneInput();

            // 2. Thay đổi trạng thái nút (Chuyển sang Edit Mode)
            btnSave.Content = "Cập nhật";
            btnDelete.Visibility = Visibility.Visible;
        }
        else
        {
            // Không có khách hàng nào được chọn, chuyển về Create Mode
            UpdateUIForCreateMode();
        }
    }

    private void btnAddNew_Click(object sender, RoutedEventArgs e)
    {
        // Chuyển sang Create Mode
        _selectedCustomer = null;
        dgCustomers.SelectedItem = null;

        // 1. Xóa trắng Form
        ClearForm();

        // 2. Cập nhật UI cho Create Mode
        UpdateUIForCreateMode();

        // 3. Tự động focus vào ô đầu tiên
        txtFullName.Focus();
    }

    private void ClearForm()
    {
        txtFullName.Text = string.Empty;
        txtPhoneNumber.Text = string.Empty;
        txtLoyaltyPoints.Text = "0";
        txtPhoneValidation.Text = string.Empty;
        txtPhoneNumber.ClearValue(TextBox.BorderBrushProperty);
    }

    private void UpdateUIForCreateMode()
    {
        btnSave.Content = "Lưu mới";
        btnDelete.Visibility = Visibility.Collapsed;
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reuse live validation before saving
            if (!ValidatePhoneInput())
            {
                return;
            }

            // Validate phone number format (optional field but must be valid if provided)
            var phone = txtPhoneNumber.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(phone))
            {
                // Vietnamese common format: 0 + 9-10 digits (total 10-11)
                var phoneRegex = new Regex(@"^(0\d{9,10})$");
                if (!phoneRegex.IsMatch(phone))
                {
                    MessageBox.Show("Số điện thoại không hợp lệ! Vui lòng nhập dạng 0xxxxxxxxx (10-11 số).", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (!int.TryParse(txtLoyaltyPoints.Text, out int loyaltyPoints) || loyaltyPoints < 0)
            {
                MessageBox.Show("Điểm tích lũy không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Logic chính: Kiểm tra _selectedCustomer để xác định Create hay Update
            if (_selectedCustomer == null)
            {
                // CREATE MODE: Tạo khách hàng mới
                var newCustomer = new Customer
                {
                    FullName = string.IsNullOrWhiteSpace(txtFullName.Text) ? null : txtFullName.Text.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
                    LoyaltyPoints = loyaltyPoints
                };
                _customerService.AddCustomer(newCustomer);
                MessageBox.Show("✅ Thêm khách hàng mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // UPDATE MODE: Cập nhật khách hàng hiện tại
                _selectedCustomer.FullName = string.IsNullOrWhiteSpace(txtFullName.Text) ? null : txtFullName.Text.Trim();
                _selectedCustomer.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
                _selectedCustomer.LoyaltyPoints = loyaltyPoints;
                _customerService.UpdateCustomer(_selectedCustomer);
                MessageBox.Show("✅ Cập nhật khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadDataGrid();
            btnAddNew_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ValidatePhoneInput()
    {
        var phone = txtPhoneNumber.Text?.Trim();

        // Reset when empty (phone is optional)
        if (string.IsNullOrWhiteSpace(phone))
        {
            txtPhoneValidation.Text = string.Empty;
            txtPhoneNumber.ClearValue(TextBox.BorderBrushProperty);
            return true;
        }

        // Format check only - removed duplicate check as per requirement
        var phoneRegex = new Regex(@"^(0\d{9,10})$");
        if (!phoneRegex.IsMatch(phone))
        {
            txtPhoneValidation.Text = "Số điện thoại không hợp lệ. Dạng đúng: 0xxxxxxxxx (10-11 số).";
            txtPhoneValidation.Foreground = Brushes.OrangeRed;
            txtPhoneNumber.BorderBrush = Brushes.OrangeRed;
            return false;
        }

        // OK - only format validation, no duplicate check
        txtPhoneValidation.Text = "Số điện thoại hợp lệ.";
        txtPhoneValidation.Foreground = Brushes.Green;
        txtPhoneNumber.BorderBrush = Brushes.Green;
        return true;
    }

    private void txtPhoneNumber_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidatePhoneInput();
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCustomer == null)
        {
            MessageBox.Show("Vui lòng chọn khách hàng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa khách hàng \"{_selectedCustomer.FullName ?? "Không tên"}\"?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _customerService.DeleteCustomer(_selectedCustomer.ID);
                MessageBox.Show("✅ Xóa khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDataGrid();
                btnAddNew_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi xóa khách hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}



