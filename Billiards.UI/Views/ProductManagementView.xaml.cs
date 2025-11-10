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
        // Khởi tạo UI về Create Mode khi load lần đầu
        UpdateUIForCreateMode();
        UpdateUIForCategoryCreateMode();
    }

    private void LoadDataGrid()
    {
        try
        {
            var products = _productService.GetAllProducts();
            dgProducts.ItemsSource = products;
            dgProducts.SelectedItem = null;
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
            // 1. Đổ dữ liệu từ sản phẩm được chọn vào form
            txtProductName.Text = _selectedProduct.ProductName;
            cmbCategory.SelectedValue = _selectedProduct.CategoryID;
            txtSalePrice.Text = _selectedProduct.SalePrice.ToString("N0"); // Format số với dấu phẩy
            txtStockQuantity.Text = _selectedProduct.StockQuantity.ToString();

            // 2. Thay đổi trạng thái nút (Chuyển sang Edit Mode)
            btnSave.Content = "Cập nhật"; // Đổi chữ nút "Lưu" thành "Cập nhật"
            btnDelete.Visibility = Visibility.Visible; // Hiển thị nút Xóa khi đang sửa
        }
        else
        {
            // Không có sản phẩm nào được chọn, chuyển về Create Mode
            UpdateUIForCreateMode();
        }
    }

    private void btnAddNew_Click(object sender, RoutedEventArgs e)
    {
        // Chuyển sang Create Mode
        _selectedProduct = null; // Đặt về null
        dgProducts.SelectedItem = null; // Bỏ chọn trên DataGrid

        // 1. Xóa trắng Form
        ClearForm();

        // 2. Cập nhật UI cho Create Mode
        UpdateUIForCreateMode();

        // 3. Tự động focus vào ô đầu tiên
        txtProductName.Focus();
    }

    private void ClearForm()
    {
        txtProductName.Text = string.Empty;
        cmbCategory.SelectedIndex = -1;
        txtSalePrice.Text = string.Empty;
        txtStockQuantity.Text = string.Empty;
    }

    private void UpdateUIForCreateMode()
    {
        // Thay đổi trạng thái nút (Chuyển sang Create Mode)
        btnSave.Content = "Lưu mới"; // Đổi chữ nút "Lưu" thành "Lưu mới"
        btnDelete.Visibility = Visibility.Collapsed; // Ẩn nút Xóa khi đang thêm mới
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtProductName.Focus();
                return;
            }

            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbCategory.Focus();
                return;
            }

            // Parse giá bán (loại bỏ dấu phẩy nếu có)
            string salePriceText = txtSalePrice.Text.Replace(",", "").Replace(".", "");
            if (!decimal.TryParse(salePriceText, out decimal salePrice) || salePrice < 0)
            {
                MessageBox.Show("Giá bán không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSalePrice.Focus();
                return;
            }

            if (!int.TryParse(txtStockQuantity.Text, out int stockQuantity) || stockQuantity < 0)
            {
                MessageBox.Show("Tồn kho không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStockQuantity.Focus();
                return;
            }

            // Logic chính: Kiểm tra _selectedProduct để xác định Create hay Update
            if (_selectedProduct == null)
            {
                // CREATE MODE: Tạo sản phẩm mới
                var newProduct = new Product
                {
                    ProductName = txtProductName.Text.Trim(),
                    CategoryID = (int)cmbCategory.SelectedValue,
                    SalePrice = salePrice,
                    StockQuantity = stockQuantity
                };
                _productService.AddProduct(newProduct);
                MessageBox.Show("✅ Thêm sản phẩm mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // UPDATE MODE: Cập nhật sản phẩm hiện tại
                _selectedProduct.ProductName = txtProductName.Text.Trim();
                _selectedProduct.CategoryID = (int)cmbCategory.SelectedValue;
                _selectedProduct.SalePrice = salePrice;
                _selectedProduct.StockQuantity = stockQuantity;
                _productService.UpdateProduct(_selectedProduct);
                MessageBox.Show("✅ Cập nhật sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Tải lại DataGrid và reset form về Create Mode
            LoadDataGrid();
            btnAddNew_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("✅ Xóa sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDataGrid();
                btnAddNew_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi xóa sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void lbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedCategory = lbCategories.SelectedItem as ProductCategory;
        if (_selectedCategory != null)
        {
            // 1. Đổ dữ liệu từ danh mục được chọn vào form
            txtCategoryName.Text = _selectedCategory.CategoryName;

            // 2. Thay đổi trạng thái nút (Chuyển sang Edit Mode)
            btnSaveCategory.Content = "Cập nhật"; // Đổi chữ nút "Lưu" thành "Cập nhật"
            btnDeleteCategory.Visibility = Visibility.Visible; // Hiển thị nút Xóa khi đang sửa
        }
        else
        {
            // Không có danh mục nào được chọn, chuyển về Create Mode
            UpdateUIForCategoryCreateMode();
        }
    }

    private void btnAddNewCategory_Click(object sender, RoutedEventArgs e)
    {
        // Chuyển sang Create Mode
        _selectedCategory = null; // Đặt về null
        lbCategories.SelectedItem = null; // Bỏ chọn trên ListBox

        // 1. Xóa trắng Form
        ClearCategoryForm();

        // 2. Cập nhật UI cho Create Mode
        UpdateUIForCategoryCreateMode();

        // 3. Tự động focus vào ô nhập
        txtCategoryName.Focus();
    }

    private void ClearCategoryForm()
    {
        txtCategoryName.Text = string.Empty;
    }

    private void UpdateUIForCategoryCreateMode()
    {
        // Thay đổi trạng thái nút (Chuyển sang Create Mode)
        btnSaveCategory.Content = "Lưu mới"; // Đổi chữ nút "Lưu" thành "Lưu mới"
        btnDeleteCategory.Visibility = Visibility.Collapsed; // Ẩn nút Xóa khi đang thêm mới
    }

    private void btnSaveCategory_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên danh mục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCategoryName.Focus();
                return;
            }

            // Logic chính: Kiểm tra _selectedCategory để xác định Create hay Update
            if (_selectedCategory == null)
            {
                // CREATE MODE: Tạo danh mục mới
                var newCategory = new ProductCategory
                {
                    CategoryName = txtCategoryName.Text.Trim()
                };
                _productService.AddCategory(newCategory);
                MessageBox.Show("✅ Thêm danh mục mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // UPDATE MODE: Cập nhật danh mục hiện tại
                _selectedCategory.CategoryName = txtCategoryName.Text.Trim();
                _productService.UpdateCategory(_selectedCategory);
                MessageBox.Show("✅ Cập nhật danh mục thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Tải lại danh sách danh mục và reset form về Create Mode
            LoadCategories();
            btnAddNewCategory_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("✅ Xóa danh mục thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCategories();
                btnAddNewCategory_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi xóa danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}



