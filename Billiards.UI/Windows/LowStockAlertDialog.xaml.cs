using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Windows;

public partial class LowStockAlertDialog : Window
{
    private readonly List<Product> _lowStockProducts;
    private readonly bool _isAdmin;
    public bool NavigateToProducts { get; private set; }

    public LowStockAlertDialog(List<Product> lowStockProducts, bool isAdmin = false)
    {
        InitializeComponent();
        _lowStockProducts = lowStockProducts;
        _isAdmin = isAdmin;
        Loaded += LowStockAlertDialog_Loaded;
    }

    private void LowStockAlertDialog_Loaded(object sender, RoutedEventArgs e)
    {
        // Update count
        txtCount.Text = _lowStockProducts.Count.ToString();

        // Load products into DataGrid
        dgLowStockProducts.ItemsSource = _lowStockProducts;

        // Ẩn nút "Đến Quản lý Sản phẩm" nếu không phải Admin
        if (!_isAdmin)
        {
            btnGoToProducts.Visibility = Visibility.Collapsed;
            // Căn giữa nút "Đóng" khi ẩn nút "Đến Quản lý Sản phẩm"
            spButtons.HorizontalAlignment = HorizontalAlignment.Center;
        }
    }

    private void btnGoToProducts_Click(object sender, RoutedEventArgs e)
    {
        NavigateToProducts = true;
        DialogResult = true;
        Close();
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        NavigateToProducts = false;
        DialogResult = false;
        Close();
    }
}

