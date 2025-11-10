using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Billiards.BLL.Services;
using Billiards.DAL.Models;
using Billiards.UI.Windows;

namespace Billiards.UI.Views;

// Helper class for "All" option in ComboBox (duplicate definition for OrderManagementView)
public class OrderFilterItem
{
    public int ID { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsAll { get; set; }
}

public partial class OrderManagementView : UserControl
{
    private readonly OrderManagementService _orderManagementService;
    private readonly CustomerService _customerService;
    private readonly EmployeeService _employeeService;
    private readonly TableManagementService _tableService;

    public OrderManagementView()
    {
        InitializeComponent();
        _orderManagementService = new OrderManagementService();
        _customerService = new CustomerService();
        _employeeService = new EmployeeService();
        _tableService = new TableManagementService();
        Loaded += OrderManagementView_Loaded;
    }

    private void OrderManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        // Set default date range: last 7 days
        dpEndDate.SelectedDate = DateTime.Now;
        dpStartDate.SelectedDate = DateTime.Now.AddDays(-7);

        // Load filter data
        LoadFilterData();

        LoadOrders();
    }

    private void LoadFilterData()
    {
        try
        {
            // Load Tables
            var tables = _tableService.GetAllTables();
            var tableList = new List<OrderFilterItem>
            {
                new OrderFilterItem { ID = 0, DisplayName = "Tất cả", IsAll = true }
            };
            foreach (var table in tables)
            {
                tableList.Add(new OrderFilterItem { ID = table.ID, DisplayName = table.TableName, IsAll = false });
            }
            cmbTableFilter.ItemsSource = tableList;
            cmbTableFilter.SelectedIndex = 0;

            // Load Customers
            var customers = _customerService.GetAllCustomers();
            var customerList = new List<OrderFilterItem>
            {
                new OrderFilterItem { ID = 0, DisplayName = "Tất cả", IsAll = true }
            };
            foreach (var customer in customers)
            {
                var displayName = string.IsNullOrWhiteSpace(customer.FullName) 
                    ? $"Khách hàng #{customer.ID}" 
                    : customer.FullName;
                customerList.Add(new OrderFilterItem { ID = customer.ID, DisplayName = displayName, IsAll = false });
            }
            cmbCustomerFilter.ItemsSource = customerList;
            cmbCustomerFilter.SelectedIndex = 0;

            // Load Employees
            var employees = _employeeService.GetAllEmployees();
            var employeeList = new List<OrderFilterItem>
            {
                new OrderFilterItem { ID = 0, DisplayName = "Tất cả", IsAll = true }
            };
            foreach (var employee in employees)
            {
                employeeList.Add(new OrderFilterItem { ID = employee.ID, DisplayName = employee.FullName, IsAll = false });
            }
            cmbEmployeeFilter.ItemsSource = employeeList;
            cmbEmployeeFilter.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải dữ liệu bộ lọc: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadOrders()
    {
        try
        {
            var startDate = dpStartDate.SelectedDate;
            var endDate = dpEndDate.SelectedDate?.AddDays(1).AddTicks(-1); // End of day
            var status = (cmbStatus.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            // Get selected IDs from ComboBox (null if "Tất cả" is selected)
            int? tableId = null;
            if (cmbTableFilter.SelectedItem is OrderFilterItem tableFilter && !tableFilter.IsAll)
            {
                tableId = tableFilter.ID;
            }

            int? customerId = null;
            if (cmbCustomerFilter.SelectedItem is OrderFilterItem customerFilter && !customerFilter.IsAll)
            {
                customerId = customerFilter.ID;
            }

            int? employeeId = null;
            if (cmbEmployeeFilter.SelectedItem is OrderFilterItem employeeFilter && !employeeFilter.IsAll)
            {
                employeeId = employeeFilter.ID;
            }

            var orders = _orderManagementService.GetAllInvoices(
                startDate: startDate,
                endDate: endDate,
                status: string.IsNullOrWhiteSpace(status) ? null : status,
                tableId: tableId,
                employeeId: employeeId,
                customerId: customerId
            );

            dgOrders.ItemsSource = orders;

            // Update summary
            UpdateSummary(orders);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateSummary(System.Collections.Generic.List<Invoice> orders)
    {
        var count = orders?.Count ?? 0;
        var totalRevenue = orders?
            .Where(o => o.Status == "Paid")
            .Sum(o => o.TotalAmount) ?? 0;

        txtTotalCount.Text = count.ToString("N0");
        txtTotalRevenue.Text = $"{totalRevenue:N0} VNĐ";
    }

    private void btnSearch_Click(object sender, RoutedEventArgs e)
    {
        LoadOrders();
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        // Reload orders with current filters
        LoadOrders();
    }

    private void btnResetFilters_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reset date filters to last 7 days
            dpEndDate.SelectedDate = DateTime.Now;
            dpStartDate.SelectedDate = DateTime.Now.AddDays(-7);

            // Reset Status filter to "Tất cả"
            cmbStatus.SelectedIndex = 0;

            // Reset ComboBox filters to "Tất cả"
            cmbTableFilter.SelectedIndex = 0;
            cmbCustomerFilter.SelectedIndex = 0;
            cmbEmployeeFilter.SelectedIndex = 0;

            // Automatically run search with default filters to refresh the display
            btnSearch_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xóa bộ lọc: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var invoice = dgOrders.SelectedItem as Invoice;
        bool isEnabled = invoice != null;

        btnViewDetails.IsEnabled = isEnabled;
        btnCancel.IsEnabled = isEnabled && invoice != null && invoice.Status == "Active";
    }

    private void btnViewDetails_Click(object sender, RoutedEventArgs e)
    {
        var invoice = dgOrders.SelectedItem as Invoice;
        if (invoice == null)
        {
            MessageBox.Show("Vui lòng chọn một đơn hàng để xem chi tiết.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            // Load full details
            var fullInvoice = _orderManagementService.GetInvoiceWithDetails(invoice.ID);
            if (fullInvoice == null)
            {
                MessageBox.Show("Không tìm thấy chi tiết đơn hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show invoice details window
            var detailsWindow = new InvoiceDetailsWindow(fullInvoice);
            detailsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xem chi tiết đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        var invoice = dgOrders.SelectedItem as Invoice;
        if (invoice == null)
        {
            MessageBox.Show("Vui lòng chọn một đơn hàng để hủy.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (invoice.Status != "Active")
        {
            MessageBox.Show("Chỉ có thể hủy đơn hàng đang Active.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn hủy đơn hàng #{invoice.ID}?\n\nLưu ý: Sản phẩm sẽ được hoàn trả lại kho và bàn sẽ được giải phóng.",
            "Xác nhận hủy đơn hàng",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = _orderManagementService.CancelInvoice(invoice.ID);
                if (success)
                {
                    MessageBox.Show("Đã hủy đơn hàng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();
                }
                else
                {
                    MessageBox.Show("Không thể hủy đơn hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hủy đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}

