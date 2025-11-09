using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Views;

public partial class ReportsView : UserControl
{
    private readonly ReportService _reportService;

    public ReportsView()
    {
        InitializeComponent();
        _reportService = new ReportService();

        // Defaults: this month to now
        var now = DateTime.Now;
        var firstDay = new DateTime(now.Year, now.Month, 1);
        dpStartDate.SelectedDate = firstDay;
        dpEndDate.SelectedDate = now;
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

            var customerName = txtCustomerNameFilter.Text?.Trim();
            var employeeName = txtEmployeeNameFilter.Text?.Trim();
            var data = _reportService.GetRevenueReport(
                startDate,
                endDate,
                string.IsNullOrWhiteSpace(customerName) ? null : customerName,
                string.IsNullOrWhiteSpace(employeeName) ? null : employeeName);
            dgReports.ItemsSource = data;

            UpdateSummary(data);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi chạy báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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


