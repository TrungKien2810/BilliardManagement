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
    }

    private void LoadDataGrid()
    {
        try
        {
            var customers = _customerService.GetAllCustomers();
            dgCustomers.ItemsSource = customers;
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
            txtFullName.Text = _selectedCustomer.FullName ?? string.Empty;
            txtPhoneNumber.Text = _selectedCustomer.PhoneNumber ?? string.Empty;
            txtLoyaltyPoints.Text = _selectedCustomer.LoyaltyPoints.ToString();
            ValidatePhoneInput();
        }
    }

    private void btnAddNew_Click(object sender, RoutedEventArgs e)
    {
        _selectedCustomer = null;
        txtFullName.Text = string.Empty;
        txtPhoneNumber.Text = string.Empty;
        txtLoyaltyPoints.Text = "0";
        dgCustomers.SelectedItem = null;
        txtPhoneValidation.Text = string.Empty;
        txtPhoneNumber.ClearValue(TextBox.BorderBrushProperty);
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

            if (_selectedCustomer != null)
            {
                // Update existing customer - no duplicate phone check
                _selectedCustomer.FullName = string.IsNullOrWhiteSpace(txtFullName.Text) ? null : txtFullName.Text;
                _selectedCustomer.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone;
                _selectedCustomer.LoyaltyPoints = loyaltyPoints;
                _customerService.UpdateCustomer(_selectedCustomer);
                MessageBox.Show("Cập nhật khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new customer - no duplicate phone check
                var newCustomer = new Customer
                {
                    FullName = string.IsNullOrWhiteSpace(txtFullName.Text) ? null : txtFullName.Text,
                    PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
                    LoyaltyPoints = loyaltyPoints
                };
                _customerService.AddCustomer(newCustomer);
                MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadDataGrid();
            btnAddNew_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Xóa khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDataGrid();
                btnAddNew_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa khách hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}



