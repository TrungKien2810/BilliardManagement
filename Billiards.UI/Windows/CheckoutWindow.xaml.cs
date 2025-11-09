using System;
using System.Linq;
using System.Windows;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Windows;

public partial class CheckoutWindow : Window
{
    private Invoice _currentInvoice;
    private BillingService _billingService;

    private int _tableId;

    public CheckoutWindow(int tableId)
    {
        InitializeComponent();
        _billingService = new BillingService();
        _tableId = tableId;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Load dữ liệu trong Window_Loaded để không block UI khi khởi tạo
            _currentInvoice = _billingService.GetInvoiceForCheckout(_tableId);
            
            if (_currentInvoice == null)
            {
                MessageBox.Show("Không tìm thấy hóa đơn cho bàn này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            LoadInvoiceData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lấy thông tin hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
        }
    }

    private void LoadInvoiceData()
    {
        // Thông tin thời gian
        txtStartTime.Text = _currentInvoice.StartTime.ToString("dd/MM/yyyy HH:mm:ss");
        txtEndTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        
        var totalTime = DateTime.Now - _currentInvoice.StartTime;
        var hours = (int)totalTime.TotalHours;
        var minutes = totalTime.Minutes;
        txtTotalTime.Text = $"{hours} giờ {minutes} phút";

        // Tiền giờ
        txtTableFee.Text = $"{_currentInvoice.TableFee:N0} VNĐ";

        // Danh sách sản phẩm
        var invoiceDetails = _billingService.GetInvoiceDetails(_currentInvoice.ID);
        dgProducts.ItemsSource = invoiceDetails.Select(id => new InvoiceDetailViewModel
        {
            Product = id.Product,
            Quantity = id.Quantity,
            UnitPrice = id.UnitPrice,
            Total = id.Quantity * id.UnitPrice
        }).ToList();

        // Tổng tiền dịch vụ
        txtProductFee.Text = $"{_currentInvoice.ProductFee:N0} VNĐ";

        // Tổng cộng
        var subTotal = _currentInvoice.TableFee + _currentInvoice.ProductFee;
        txtSubTotal.Text = $"{subTotal:N0} VNĐ";

        // Giảm giá
        txtDiscount.Text = _currentInvoice.Discount.ToString("N0");

        // Tổng thanh toán
        UpdateTotalAmount();
    }

    private void UpdateTotalAmount()
    {
        if (_currentInvoice == null) return;

        var tableFee = _currentInvoice.TableFee;
        var productFee = _currentInvoice.ProductFee;
        decimal discount = 0;

        if (decimal.TryParse(txtDiscount.Text?.Replace(",", "").Replace(".", ""), out decimal discountValue))
        {
            discount = discountValue;
        }

        var totalAmount = tableFee + productFee - discount;
        if (totalAmount < 0) totalAmount = 0;

        txtTotalAmount.Text = $"{totalAmount:N0} VNĐ";
    }

    private void txtDiscount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateTotalAmount();
    }

    private void txtDiscount_LostFocus(object sender, RoutedEventArgs e)
    {
        UpdateTotalAmount();
    }

    private void btnCheckout_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Lấy giảm giá
            decimal discount = 0;
            if (decimal.TryParse(txtDiscount.Text?.Replace(",", "").Replace(".", ""), out decimal discountValue))
            {
                discount = discountValue;
            }

            // Lấy customer ID (tạm thời để null, có thể thêm tìm kiếm sau)
            int? customerId = null;

            // Finalize checkout
            var success = _billingService.FinalizeCheckout(_currentInvoice.ID, discount, customerId);
            
            if (success)
            {
                MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Thanh toán thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi thanh toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }
}

// Helper class for invoice detail display
public class InvoiceDetailViewModel
{
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

