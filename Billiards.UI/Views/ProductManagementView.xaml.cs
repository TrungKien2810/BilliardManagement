using System;
using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Views;

public partial class ProductManagementView : UserControl
{
    private readonly ProductService _productService;
    private Product? _selectedProduct;
    private ProductCategory? _selectedCategory;

    public ProductManagementView()
    {
        InitializeComponent();
        _productService = new ProductService();
        Loaded += ProductManagementView_Loaded;
    }

    private void ProductManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        LoadDataGrid();
        LoadCategories();
    }

    private void LoadDataGrid()
    {
        try
        {
            var products = _productService.GetAllProducts();
            dgProducts.ItemsSource = products;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadCategories()
    {
        try
        {
            var categories = _productService.GetAllCategories();
            cmbCategory.ItemsSource = categories;
            lbCategories.ItemsSource = categories;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedProduct = dgProducts.SelectedItem as Product;
        if (_selectedProduct != null)
        {
            txtProductName.Text = _selectedProduct.ProductName;
            cmbCategory.SelectedValue = _selectedProduct.CategoryID;
            txtSalePrice.Text = _selectedProduct.SalePrice.ToString();
            txtStockQuantity.Text = _selectedProduct.StockQuantity.ToString();
        }
    }

    private void btnAddNew_Click(object sender, RoutedEventArgs e)
    {
        _selectedProduct = null;
        txtProductName.Text = string.Empty;
        cmbCategory.SelectedIndex = -1;
        txtSalePrice.Text = string.Empty;
        txtStockQuantity.Text = string.Empty;
        dgProducts.SelectedItem = null;
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtSalePrice.Text, out decimal salePrice) || salePrice < 0)
            {
                MessageBox.Show("Giá bán không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStockQuantity.Text, out int stockQuantity) || stockQuantity < 0)
            {
                MessageBox.Show("Tồn kho không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedProduct != null)
            {
                // Update existing product
                _selectedProduct.ProductName = txtProductName.Text;
                _selectedProduct.CategoryID = (int)cmbCategory.SelectedValue;
                _selectedProduct.SalePrice = salePrice;
                _selectedProduct.StockQuantity = stockQuantity;
                _productService.UpdateProduct(_selectedProduct);
                MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new product
                var newProduct = new Product
                {
                    ProductName = txtProductName.Text,
                    CategoryID = (int)cmbCategory.SelectedValue,
                    SalePrice = salePrice,
                    StockQuantity = stockQuantity
                };
                _productService.AddProduct(newProduct);
                MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadDataGrid();
            btnAddNew_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedProduct == null)
        {
            MessageBox.Show("Vui lòng chọn sản phẩm cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa sản phẩm \"{_selectedProduct.ProductName}\"?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _productService.DeleteProduct(_selectedProduct.ID);
                MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDataGrid();
                btnAddNew_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void lbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedCategory = lbCategories.SelectedItem as ProductCategory;
        if (_selectedCategory != null)
        {
            txtCategoryName.Text = _selectedCategory.CategoryName;
        }
    }

    private void btnAddCategory_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên danh mục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newCategory = new ProductCategory
            {
                CategoryName = txtCategoryName.Text
            };
            _productService.AddCategory(newCategory);
            MessageBox.Show("Thêm danh mục thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadCategories();
            txtCategoryName.Text = string.Empty;
            lbCategories.SelectedItem = null;
            _selectedCategory = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnDeleteCategory_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCategory == null)
        {
            MessageBox.Show("Vui lòng chọn danh mục cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa danh mục \"{_selectedCategory.CategoryName}\"?\n\nLưu ý: Nếu danh mục này đang được sử dụng bởi các sản phẩm, việc xóa có thể gây lỗi.",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _productService.DeleteCategory(_selectedCategory.ID);
                MessageBox.Show("Xóa danh mục thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCategories();
                txtCategoryName.Text = string.Empty;
                lbCategories.SelectedItem = null;
                _selectedCategory = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}



