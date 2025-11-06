using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Windows;

public partial class OrderWindow : Window
{
    private Invoice _currentInvoice;
    private OrderService _orderService;
    private List<CartItem> _cartItems;

    public OrderWindow(Invoice invoice)
    {
        InitializeComponent();
        _currentInvoice = invoice;
        _orderService = new OrderService();
        _cartItems = new List<CartItem>();
        dgCart.ItemsSource = _cartItems;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LoadCategories();
    }

    private void LoadCategories()
    {
        try
        {
            var categories = _orderService.GetMenuCategories();
            lbCategories.ItemsSource = categories;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void lbCategories_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (lbCategories.SelectedValue != null && lbCategories.SelectedValue is int categoryId)
        {
            try
            {
                var products = _orderService.GetMenuProducts(categoryId);
                icProducts.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Product_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is Product product)
        {
            // Ask for quantity using a simple input dialog
            var inputDialog = new InputDialog($"Nhập số lượng cho {product.ProductName}:", "Nhập số lượng", "1");
            if (inputDialog.ShowDialog() == true)
            {
                if (int.TryParse(inputDialog.Answer, out int quantity) && quantity > 0)
                {
                    // Check if product already in cart
                    var existingItem = _cartItems.FirstOrDefault(item => item.ProductID == product.ID);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += quantity;
                    }
                    else
                    {
                        _cartItems.Add(new CartItem
                        {
                            ProductID = product.ID,
                            ProductName = product.ProductName,
                            Quantity = quantity,
                            UnitPrice = product.SalePrice
                        });
                    }
                    dgCart.Items.Refresh();
                }
                else
                {
                    MessageBox.Show("Số lượng không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    private void btnConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (_cartItems.Count == 0)
        {
            MessageBox.Show("Giỏ hàng trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            foreach (var item in _cartItems)
            {
                _orderService.AddProductToInvoice(_currentInvoice.ID, item.ProductID, item.Quantity);
            }
            MessageBox.Show("Đặt hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }
}

// Helper class for cart items
public class CartItem
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}

