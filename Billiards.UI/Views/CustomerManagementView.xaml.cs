using System;
using System.Windows;
using System.Windows.Controls;
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
        }
    }

    private void btnAddNew_Click(object sender, RoutedEventArgs e)
    {
        _selectedCustomer = null;
        txtFullName.Text = string.Empty;
        txtPhoneNumber.Text = string.Empty;
        txtLoyaltyPoints.Text = "0";
        dgCustomers.SelectedItem = null;
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!int.TryParse(txtLoyaltyPoints.Text, out int loyaltyPoints) || loyaltyPoints < 0)
            {
                MessageBox.Show("Điểm tích lũy không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedCustomer != null)
            {
                // Update existing customer
                _selectedCustomer.FullName = string.IsNullOrWhiteSpace(txtFullName.Text) ? null : txtFullName.Text;
                _selectedCustomer.PhoneNumber = string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text;
                _selectedCustomer.LoyaltyPoints = loyaltyPoints;
                _customerService.UpdateCustomer(_selectedCustomer);
                MessageBox.Show("Cập nhật khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Check if phone number already exists (if provided)
                if (!string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
                {
                    var existingCustomer = _customerService.GetCustomerByPhoneNumber(txtPhoneNumber.Text);
                    if (existingCustomer != null)
                    {
                        MessageBox.Show("Số điện thoại đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Add new customer
                var newCustomer = new Customer
                {
                    FullName = string.IsNullOrWhiteSpace(txtFullName.Text) ? null : txtFullName.Text,
                    PhoneNumber = string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text,
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



