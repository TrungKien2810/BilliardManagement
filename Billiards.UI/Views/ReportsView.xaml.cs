using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Views;

// Helper class for "All" option in ComboBox
public class FilterItem
{
    public int ID { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsAll { get; set; }
}

public partial class ReportsView : UserControl
{
    private readonly ReportService _reportService;
    private readonly DashboardService _dashboardService;
    private readonly CustomerService _customerService;
    private readonly EmployeeService _employeeService;
    private readonly TableManagementService _tableService;
    private DispatcherTimer? _refreshTimer;

    public ReportsView()
    {
        InitializeComponent();
        _reportService = new ReportService();
        _dashboardService = new DashboardService();
        _customerService = new CustomerService();
        _employeeService = new EmployeeService();
        _tableService = new TableManagementService();
        Loaded += ReportsView_Loaded;

        // Defaults: this month to now
        var now = DateTime.Now;
        var firstDay = new DateTime(now.Year, now.Month, 1);
        dpStartDate.SelectedDate = firstDay;
        dpEndDate.SelectedDate = now;
    }

    private void ReportsView_Loaded(object sender, RoutedEventArgs e)
    {
        // Load dashboard data when view loads
        LoadDashboardData();

        // Load filter data
        LoadFilterData();

        // Auto refresh dashboard every 30 seconds
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _refreshTimer.Tick += (s, args) => LoadDashboardData();
        _refreshTimer.Start();
    }

    private void LoadFilterData()
    {
        try
        {
            // Load Tables
            var tables = _tableService.GetAllTables();
            var tableList = new List<FilterItem>
            {
                new FilterItem { ID = 0, DisplayName = "Tất cả", IsAll = true }
            };
            foreach (var table in tables)
            {
                tableList.Add(new FilterItem { ID = table.ID, DisplayName = table.TableName, IsAll = false });
            }
            cmbTableFilter.ItemsSource = tableList;
            cmbTableFilter.SelectedIndex = 0;

            // Load Customers
            var customers = _customerService.GetAllCustomers();
            var customerList = new List<FilterItem>
            {
                new FilterItem { ID = 0, DisplayName = "Tất cả", IsAll = true }
            };
            foreach (var customer in customers)
            {
                var displayName = string.IsNullOrWhiteSpace(customer.FullName) 
                    ? $"Khách hàng #{customer.ID}" 
                    : customer.FullName;
                customerList.Add(new FilterItem { ID = customer.ID, DisplayName = displayName, IsAll = false });
            }
            cmbCustomerFilter.ItemsSource = customerList;
            cmbCustomerFilter.SelectedIndex = 0;

            // Load Employees
            var employees = _employeeService.GetAllEmployees();
            var employeeList = new List<FilterItem>
            {
                new FilterItem { ID = 0, DisplayName = "Tất cả", IsAll = true }
            };
            foreach (var employee in employees)
            {
                employeeList.Add(new FilterItem { ID = employee.ID, DisplayName = employee.FullName, IsAll = false });
            }
            cmbEmployeeFilter.ItemsSource = employeeList;
            cmbEmployeeFilter.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải dữ liệu bộ lọc: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadDashboardData()
    {
        try
        {
            // Load statistics
            var todayRevenue = _dashboardService.GetTodayRevenue();
            var monthRevenue = _dashboardService.GetMonthRevenue();
            var inUseTables = _dashboardService.GetInUseTablesCount();
            var totalTables = _dashboardService.GetTotalTablesCount();
            var todayInvoices = _dashboardService.GetTodayInvoicesCount();
            var activeInvoices = _dashboardService.GetActiveInvoicesCount();

            // Update UI
            txtTodayRevenue.Text = $"{todayRevenue:N0} VNĐ";
            txtMonthRevenue.Text = $"{monthRevenue:N0} VNĐ";
            txtInUseTables.Text = inUseTables.ToString();
            txtTotalTables.Text = $"/ {totalTables}";
            txtTodayInvoices.Text = todayInvoices.ToString();
            txtActiveInvoices.Text = $"{activeInvoices} đang active";

            // Load daily revenue
            var dailyRevenue = _dashboardService.GetDailyRevenueThisMonth();
            dgDailyRevenue.ItemsSource = dailyRevenue;

            // Load top products
            var topProducts = _dashboardService.GetTopProductsToday(5);
            dgTopProducts.ItemsSource = topProducts;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải dữ liệu dashboard: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnRunReport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ khoảng thời gian.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = dpStartDate.SelectedDate.Value.Date;
            var endDate = dpEndDate.SelectedDate.Value.Date.AddDays(1).AddTicks(-1); // end of day

            // Get selected IDs from ComboBox (null if "Tất cả" is selected)
            int? tableId = null;
            if (cmbTableFilter.SelectedItem is FilterItem tableFilter && !tableFilter.IsAll)
            {
                tableId = tableFilter.ID;
            }

            int? customerId = null;
            if (cmbCustomerFilter.SelectedItem is FilterItem customerFilter && !customerFilter.IsAll)
            {
                customerId = customerFilter.ID;
            }

            int? employeeId = null;
            if (cmbEmployeeFilter.SelectedItem is FilterItem employeeFilter && !employeeFilter.IsAll)
            {
                employeeId = employeeFilter.ID;
            }

            // Call service with IDs (performance: uses index)
            var data = _reportService.GetRevenueReport(startDate, endDate, customerId, employeeId, tableId);
            dgReports.ItemsSource = data;

            UpdateSummary(data);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi chạy báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnResetFilters_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reset date filters to this month
            var now = DateTime.Now;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            dpStartDate.SelectedDate = firstDay;
            dpEndDate.SelectedDate = now;

            // Reset ComboBox filters to "Tất cả"
            cmbTableFilter.SelectedIndex = 0;
            cmbCustomerFilter.SelectedIndex = 0;
            cmbEmployeeFilter.SelectedIndex = 0;

            // Automatically run report with default filters to refresh the display
            btnRunReport_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xóa bộ lọc: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateSummary(List<Invoice> invoices)
    {
        var count = invoices?.Count ?? 0;
        var sumTable = invoices?.Sum(i => i.TableFee) ?? 0;
        var sumProduct = invoices?.Sum(i => i.ProductFee) ?? 0;
        var sumDiscount = invoices?.Sum(i => i.Discount) ?? 0;
        var sumTotal = invoices?.Sum(i => i.TotalAmount) ?? 0;

        txtCount.Text = count.ToString("N0");
        txtSumTableFee.Text = $"{sumTable:N0}";
        txtSumProductFee.Text = $"{sumProduct:N0}";
        txtSumDiscount.Text = $"{sumDiscount:N0}";
        txtSumTotal.Text = $"{sumTotal:N0}";
    }
}


