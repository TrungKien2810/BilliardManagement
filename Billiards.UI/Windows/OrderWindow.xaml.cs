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
                    try
                    {
                        // Check if product already in cart
                        var existingItem = _cartItems.FirstOrDefault(item => item.ProductID == product.ID);
                        int quantityInCart = existingItem?.Quantity ?? 0;
                        int totalQuantity = quantityInCart + quantity;

                        // Get available stock (accounting for items already in invoice)
                        int availableStock = _orderService.GetAvailableStock(_currentInvoice.ID, product.ID);
                        
                        // Check if we have enough stock for the total quantity (existing in cart + new quantity)
                        if (availableStock < totalQuantity)
                        {
                            // Calculate how many can still be added
                            int canAdd = Math.Max(0, availableStock - quantityInCart);
                            string message = canAdd > 0
                                ? $"Không đủ hàng trong kho! Hiện tại chỉ còn {availableStock} sản phẩm. " +
                                  (quantityInCart > 0 
                                      ? $"Bạn đã có {quantityInCart} sản phẩm trong giỏ, chỉ có thể thêm tối đa {canAdd} sản phẩm nữa."
                                      : $"Bạn có thể thêm tối đa {canAdd} sản phẩm.")
                                : $"Không đủ hàng trong kho! Hiện tại chỉ còn {availableStock} sản phẩm.";
                            
                            MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Add or update cart item
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
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
            // Validate all items first and remove invalid ones
            var itemsToRemove = new List<CartItem>();
            var errorMessages = new List<string>();

            foreach (var item in _cartItems.ToList())
            {
                try
                {
                    // Check available stock
                    int availableStock = _orderService.GetAvailableStock(_currentInvoice.ID, item.ProductID);
                    if (availableStock < item.Quantity)
                    {
                        itemsToRemove.Add(item);
                        errorMessages.Add($"{item.ProductName}: Chỉ còn {availableStock} sản phẩm (yêu cầu {item.Quantity})");
                    }
                }
                catch (Exception ex)
                {
                    itemsToRemove.Add(item);
                    errorMessages.Add($"{item.ProductName}: {ex.Message}");
                }
            }

            // Remove invalid items from cart
            if (itemsToRemove.Count > 0)
            {
                foreach (var item in itemsToRemove)
                {
                    _cartItems.Remove(item);
                }
                dgCart.Items.Refresh();

                // If all items were removed, show error and return
                if (_cartItems.Count == 0)
                {
                    string errorMessage = "Tất cả sản phẩm đã bị xóa khỏi giỏ hàng do không đủ tồn kho:\n" +
                                        string.Join("\n", errorMessages);
                    MessageBox.Show(errorMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Process valid items
            var processedItems = new List<CartItem>();
            var processingErrors = new List<string>();

            foreach (var item in _cartItems.ToList())
            {
                try
                {
                    _orderService.AddProductToInvoice(_currentInvoice.ID, item.ProductID, item.Quantity);
                    processedItems.Add(item);
                }
                catch (Exception ex)
                {
                    // If processing fails (e.g., race condition), remove from cart
                    _cartItems.Remove(item);
                    processingErrors.Add($"{item.ProductName}: {ex.Message}");
                }
            }

            // Refresh cart to reflect any removed items
            if (processingErrors.Count > 0)
            {
                dgCart.Items.Refresh();
            }

            // Show combined result message
            if (processedItems.Count > 0)
            {
                string message = "Đặt hàng thành công!";
                if (itemsToRemove.Count > 0 || processingErrors.Count > 0)
                {
                    int totalRemoved = itemsToRemove.Count + processingErrors.Count;
                    message = $"Đã thêm {processedItems.Count} sản phẩm vào hóa đơn.";
                    if (totalRemoved > 0)
                    {
                        message += $"\n{totalRemoved} sản phẩm đã bị xóa do không đủ tồn kho.";
                    }
                }
                MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                // All items were invalid or failed to process
                string errorMsg = "Không có sản phẩm nào được thêm vào hóa đơn.\n\n";
                if (itemsToRemove.Count > 0)
                {
                    errorMsg += "Các sản phẩm đã bị xóa:\n" + string.Join("\n", errorMessages);
                }
                if (processingErrors.Count > 0)
                {
                    if (itemsToRemove.Count > 0) errorMsg += "\n\n";
                    errorMsg += "Lỗi khi xử lý:\n" + string.Join("\n", processingErrors);
                }
                MessageBox.Show(errorMsg, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

