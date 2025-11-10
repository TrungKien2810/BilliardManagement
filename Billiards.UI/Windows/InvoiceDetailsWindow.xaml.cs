using System;
using System.Linq;
using System.Windows;
using Billiards.DAL.Models;

namespace Billiards.UI.Windows;

public partial class InvoiceDetailsWindow : Window
{
    private readonly Invoice _invoice;

    public InvoiceDetailsWindow(Invoice invoice)
    {
        InitializeComponent();
        _invoice = invoice;
        Loaded += InvoiceDetailsWindow_Loaded;
    }

    private void InvoiceDetailsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        LoadInvoiceDetails();
    }

    private void LoadInvoiceDetails()
    {
        // Invoice Info
        txtInvoiceId.Text = _invoice.ID.ToString();
        txtTable.Text = _invoice.Table?.TableName ?? "N/A";
        txtStartTime.Text = _invoice.StartTime.ToString("dd/MM/yyyy HH:mm:ss");
        txtEndTime.Text = _invoice.EndTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
        txtEmployee.Text = _invoice.CreatedByEmployee?.FullName ?? "N/A";
        txtCustomer.Text = _invoice.Customer?.FullName ?? "N/A";
        
        // Status with color
        txtStatus.Text = _invoice.Status;
        txtStatus.Foreground = _invoice.Status switch
        {
            "Active" => System.Windows.Media.Brushes.Orange,
            "Paid" => System.Windows.Media.Brushes.Green,
            "Cancelled" => System.Windows.Media.Brushes.Red,
            _ => System.Windows.Media.Brushes.Black
        };

        // Invoice Details
        var details = _invoice.InvoiceDetails.Select(id => new InvoiceDetailViewModel
        {
            Product = id.Product,
            Quantity = id.Quantity,
            UnitPrice = id.UnitPrice,
            Total = id.Quantity * id.UnitPrice
        }).ToList();

        dgInvoiceDetails.ItemsSource = details;

        // Summary
        txtTableFee.Text = $"{_invoice.TableFee:N0} VNĐ";
        txtProductFee.Text = $"{_invoice.ProductFee:N0} VNĐ";
        txtDiscount.Text = $"{_invoice.Discount:N0} VNĐ";
        txtTotalAmount.Text = $"{_invoice.TotalAmount:N0} VNĐ";
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}

