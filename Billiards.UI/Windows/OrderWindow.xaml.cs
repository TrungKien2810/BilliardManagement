using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Windows;

public partial class OrderWindow : Window
{
    private Invoice _currentInvoice;
    private OrderService _orderService;
    private ObservableCollection<CartItem> _cartItems;

    public OrderWindow(Invoice invoice)
    {
        InitializeComponent();
        _currentInvoice = invoice;
        _orderService = new OrderService();
        _cartItems = new ObservableCollection<CartItem>();
        dgCart.ItemsSource = _cartItems;

        // Set window title with table info
        this.Title = $"Đặt hàng - {invoice.Table?.TableName ?? "Bàn không xác định"}";
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

            // Auto-select first category if available
            if (categories.Any())
            {
                lbCategories.SelectedIndex = 0;
                // Đảm bảo load sản phẩm của category đầu tiên
                var firstCategory = categories.First();
                LoadProductsForCategory(firstCategory.ID);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LoadProductsForCategory(int categoryId)
    {
        try
        {
            var products = _orderService.GetMenuProducts(categoryId);
            icProducts.ItemsSource = products;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải sản phẩm: {ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void lbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Sửa lại: dùng SelectedItem thay vì SelectedValue
        if (lbCategories.SelectedItem != null && lbCategories.SelectedItem is ProductCategory selectedCategory)
        {
            LoadProductsForCategory(selectedCategory.ID);
        }
        else
        {
            icProducts.ItemsSource = null;
        }
    }

    private void Product_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Product product)
        {
            // Check if product has stock
            if (product.StockQuantity <= 0)
            {
                MessageBox.Show($"Sản phẩm '{product.ProductName}' đã hết hàng!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Ask for quantity using a simple input dialog
            var inputDialog = new InputDialog(
                $"Nhập số lượng cho {product.ProductName}:\n(Còn lại: {product.StockQuantity})",
                "Nhập số lượng",
                "1");

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

                        // Check if we have enough stock for the total quantity
                        if (availableStock < totalQuantity)
                        {
                            // Calculate how many can still be added
                            int canAdd = Math.Max(0, availableStock - quantityInCart);
                            string message = canAdd > 0
                                ? $"⚠️ Không đủ hàng trong kho!\n\n" +
                                  $"Hiện tại chỉ còn: {availableStock} sản phẩm\n" +
                                  (quantityInCart > 0
                                      ? $"Trong giỏ: {quantityInCart} sản phẩm\n" +
                                        $"Có thể thêm tối đa: {canAdd} sản phẩm"
                                      : $"Có thể thêm tối đa: {canAdd} sản phẩm")
                                : $"⚠️ Không đủ hàng trong kho!\n\n" +
                                  $"Hiện tại chỉ còn: {availableStock} sản phẩm\n" +
                                  $"Trong giỏ: {quantityInCart} sản phẩm";

                            MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Add or update cart item
                        if (existingItem != null)
                        {
                            existingItem.Quantity += quantity;
                            // Force update binding
                            var index = _cartItems.IndexOf(existingItem);
                            _cartItems[index] = existingItem;
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

                        // Show success notification
                        ShowTemporaryNotification($"✓ Đã thêm {quantity} x {product.ProductName} vào giỏ hàng");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("⚠️ Số lượng không hợp lệ!\n\nVui lòng nhập số nguyên dương.",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }
    }

    private void ShowTemporaryNotification(string message)
    {
        // Simple notification using MessageBox with auto-close after short time
        // You can replace this with a custom toast notification if needed
        var result = MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void btnConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (_cartItems.Count == 0)
        {
            MessageBox.Show("⚠️ Giỏ hàng trống!\n\nVui lòng chọn ít nhất một sản phẩm.",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Show confirmation dialog with cart summary
        var totalItems = _cartItems.Sum(x => x.Quantity);
        var totalAmount = _cartItems.Sum(x => x.Total);
        var confirmMessage = $"📋 XÁC NHẬN ĐẶT HÀNG\n\n" +
                           $"Tổng số sản phẩm: {totalItems}\n" +
                           $"Tổng tiền: {totalAmount:N0} VNĐ\n\n" +
                           $"Bạn có chắc chắn muốn xác nhận?";

        var confirmResult = MessageBox.Show(confirmMessage,
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmResult != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            // Disable button to prevent double-click
            btnConfirm.IsEnabled = false;

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
                        errorMessages.Add($"• {item.ProductName}: Chỉ còn {availableStock} (yêu cầu {item.Quantity})");
                    }
                }
                catch (Exception ex)
                {
                    itemsToRemove.Add(item);
                    errorMessages.Add($"• {item.ProductName}: {ex.Message}");
                }
            }

            // Remove invalid items from cart
            if (itemsToRemove.Count > 0)
            {
                foreach (var item in itemsToRemove)
                {
                    _cartItems.Remove(item);
                }

                // If all items were removed, show error and return
                if (_cartItems.Count == 0)
                {
                    string errorMessage = "❌ TẤT CẢ SẢN PHẨM BỊ XÓA\n\n" +
                                        "Không đủ tồn kho cho các sản phẩm:\n\n" +
                                        string.Join("\n", errorMessages);
                    MessageBox.Show(errorMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnConfirm.IsEnabled = true;
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
                    processingErrors.Add($"• {item.ProductName}: {ex.Message}");
                }
            }

            // Show combined result message
            if (processedItems.Count > 0)
            {
                string message = "✅ ĐẶT HÀNG THÀNH CÔNG!\n\n";
                message += $"Đã thêm {processedItems.Count} sản phẩm vào hóa đơn.";

                int totalRemoved = itemsToRemove.Count + processingErrors.Count;
                if (totalRemoved > 0)
                {
                    message += $"\n\n⚠️ {totalRemoved} sản phẩm đã bị xóa do không đủ tồn kho.";
                }

                MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                // All items were invalid or failed to process
                string errorMsg = "❌ KHÔNG CÓ SẢN PHẨM NÀO ĐƯỢC THÊM\n\n";
                if (itemsToRemove.Count > 0)
                {
                    errorMsg += "Các sản phẩm bị xóa:\n" + string.Join("\n", errorMessages);
                }
                if (processingErrors.Count > 0)
                {
                    if (itemsToRemove.Count > 0) errorMsg += "\n\n";
                    errorMsg += "Lỗi khi xử lý:\n" + string.Join("\n", processingErrors);
                }
                MessageBox.Show(errorMsg, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                btnConfirm.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ LỖI\n\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            btnConfirm.IsEnabled = true;
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        if (_cartItems.Count > 0)
        {
            var result = MessageBox.Show(
                "⚠️ Bạn có chắc chắn muốn hủy?\n\nGiỏ hàng sẽ bị xóa.",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        this.DialogResult = false;
        this.Close();
    }
}

// Helper class for cart items
public class CartItem : System.ComponentModel.INotifyPropertyChanged
{
    private int _quantity;

    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Total));
            }
        }
    }

    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}