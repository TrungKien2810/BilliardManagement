using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Billiards.BLL.Services;
using Billiards.DAL.Models;
using Billiards.UI.Windows;

namespace Billiards.UI.Views;

public partial class OrderManagementView : UserControl
{
    private readonly OrderManagementService _orderManagementService;

    public OrderManagementView()
    {
        InitializeComponent();
        _orderManagementService = new OrderManagementService();
        Loaded += OrderManagementView_Loaded;
    }

    private void OrderManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        // Set default date range: last 7 days
        dpEndDate.SelectedDate = DateTime.Now;
        dpStartDate.SelectedDate = DateTime.Now.AddDays(-7);

        LoadOrders();
    }

    private void LoadOrders()
    {
        try
        {
            var startDate = dpStartDate.SelectedDate;
            var endDate = dpEndDate.SelectedDate?.AddDays(1).AddTicks(-1); // End of day
            var status = (cmbStatus.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            var orders = _orderManagementService.GetAllInvoices(
                startDate: startDate,
                endDate: endDate,
                status: string.IsNullOrWhiteSpace(status) ? null : status
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
        // Reset filters
        dpEndDate.SelectedDate = DateTime.Now;
        dpStartDate.SelectedDate = DateTime.Now.AddDays(-7);
        cmbStatus.SelectedIndex = 0;
        txtSearchId.Text = string.Empty;

        LoadOrders();
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

    private void txtSearchId_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (int.TryParse(txtSearchId.Text, out int invoiceId))
            {
                try
                {
                    var invoice = _orderManagementService.GetInvoiceWithDetails(invoiceId);
                    if (invoice != null)
                    {
                        // Load all invoices and select the found one
                        LoadOrders();
                        
                        // Find and select the invoice
                        var orders = dgOrders.ItemsSource as System.Collections.Generic.List<Invoice>;
                        if (orders != null)
                        {
                            var found = orders.FirstOrDefault(o => o.ID == invoiceId);
                            if (found != null)
                            {
                                dgOrders.SelectedItem = found;
                                dgOrders.ScrollIntoView(found);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Không tìm thấy đơn hàng với ID {invoiceId}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tìm kiếm đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (!string.IsNullOrWhiteSpace(txtSearchId.Text))
            {
                MessageBox.Show("Vui lòng nhập ID hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

