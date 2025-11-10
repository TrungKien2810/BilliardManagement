using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Views;

public partial class TableManagementView : UserControl
{
    private readonly TableManagementService _tableService;
    private Table? _selectedTable;
    private Area? _selectedArea;
    private TableType? _selectedTableType;

    public TableManagementView()
    {
        InitializeComponent();
        _tableService = new TableManagementService();
        Loaded += TableManagementView_Loaded;
    }

    private void TableManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        LoadTables();
        LoadAreasForCombo();
        LoadTableTypesForCombo();
        LoadAreas();
        LoadTableTypes();
        // Khởi tạo UI về Create Mode khi load lần đầu
        UpdateUIForTableCreateMode();
        UpdateUIForAreaCreateMode();
        UpdateUIForTableTypeCreateMode();
    }

    #region Tables Management

    private void LoadTables()
    {
        try
        {
            var tables = _tableService.GetAllTables();
            dgTables.ItemsSource = tables;
            dgTables.SelectedItem = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadAreasForCombo()
    {
        try
        {
            var areas = _tableService.GetAllAreas();
            cmbArea.ItemsSource = areas;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải khu vực: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadTableTypesForCombo()
    {
        try
        {
            var tableTypes = _tableService.GetAllTableTypes();
            cmbTableType.ItemsSource = tableTypes;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải loại bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedTable = dgTables.SelectedItem as Table;
        if (_selectedTable != null)
        {
            // 1. Đổ dữ liệu từ bàn được chọn vào form
            txtTableName.Text = _selectedTable.TableName;
            cmbArea.SelectedValue = _selectedTable.AreaID;
            cmbTableType.SelectedValue = _selectedTable.TypeID;
            
            // Set status
            var statusItem = cmbStatus.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == _selectedTable.Status);
            if (statusItem != null)
                cmbStatus.SelectedItem = statusItem;

            // 2. Thay đổi trạng thái nút (Chuyển sang Edit Mode)
            btnSaveTable.Content = "Cập nhật";
            btnDeleteTable.Visibility = Visibility.Visible;
        }
        else
        {
            // Không có bàn nào được chọn, chuyển về Create Mode
            UpdateUIForTableCreateMode();
        }
    }

    private void btnAddNewTable_Click(object sender, RoutedEventArgs e)
    {
        // Chuyển sang Create Mode
        _selectedTable = null;
        dgTables.SelectedItem = null;

        // 1. Xóa trắng Form
        ClearTableForm();

        // 2. Cập nhật UI cho Create Mode
        UpdateUIForTableCreateMode();

        // 3. Tự động focus vào ô đầu tiên
        txtTableName.Focus();
    }

    private void ClearTableForm()
    {
        txtTableName.Text = string.Empty;
        cmbArea.SelectedIndex = -1;
        cmbTableType.SelectedIndex = -1;
        cmbStatus.SelectedIndex = 0; // Default to Free
    }

    private void UpdateUIForTableCreateMode()
    {
        btnSaveTable.Content = "Lưu mới";
        btnDeleteTable.Visibility = Visibility.Collapsed;
    }

    private void btnSaveTable_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtTableName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbArea.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn khu vực!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbTableType.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn loại bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var status = (cmbStatus.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Free";

            // Logic chính: Kiểm tra _selectedTable để xác định Create hay Update
            if (_selectedTable == null)
            {
                // CREATE MODE: Tạo bàn mới
                var newTable = new Table
                {
                    TableName = txtTableName.Text.Trim(),
                    AreaID = (int)cmbArea.SelectedValue,
                    TypeID = (int)cmbTableType.SelectedValue,
                    Status = status
                };
                _tableService.AddTable(newTable);
                MessageBox.Show("✅ Thêm bàn mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // UPDATE MODE: Cập nhật bàn hiện tại
                _selectedTable.TableName = txtTableName.Text.Trim();
                _selectedTable.AreaID = (int)cmbArea.SelectedValue;
                _selectedTable.TypeID = (int)cmbTableType.SelectedValue;
                _selectedTable.Status = status;
                _tableService.UpdateTable(_selectedTable);
                MessageBox.Show("✅ Cập nhật bàn thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadTables();
            btnAddNewTable_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnDeleteTable_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTable == null)
        {
            MessageBox.Show("Vui lòng chọn bàn cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa bàn \"{_selectedTable.TableName}\"?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _tableService.DeleteTable(_selectedTable.ID);
                MessageBox.Show("✅ Xóa bàn thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTables();
                btnAddNewTable_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi xóa bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion

    #region Areas Management

    private void LoadAreas()
    {
        try
        {
            var areas = _tableService.GetAllAreas();
            dgAreas.ItemsSource = areas;
            dgAreas.SelectedItem = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách khu vực: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgAreas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedArea = dgAreas.SelectedItem as Area;
        if (_selectedArea != null)
        {
            // 1. Đổ dữ liệu từ khu vực được chọn vào form
            txtAreaName.Text = _selectedArea.AreaName;

            // 2. Thay đổi trạng thái nút (Chuyển sang Edit Mode)
            btnSaveArea.Content = "Cập nhật";
            btnDeleteArea.Visibility = Visibility.Visible;
        }
        else
        {
            // Không có khu vực nào được chọn, chuyển về Create Mode
            UpdateUIForAreaCreateMode();
        }
    }

    private void btnAddNewArea_Click(object sender, RoutedEventArgs e)
    {
        // Chuyển sang Create Mode
        _selectedArea = null;
        dgAreas.SelectedItem = null;

        // 1. Xóa trắng Form
        ClearAreaForm();

        // 2. Cập nhật UI cho Create Mode
        UpdateUIForAreaCreateMode();

        // 3. Tự động focus vào ô nhập
        txtAreaName.Focus();
    }

    private void ClearAreaForm()
    {
        txtAreaName.Text = string.Empty;
    }

    private void UpdateUIForAreaCreateMode()
    {
        btnSaveArea.Content = "Lưu mới";
        btnDeleteArea.Visibility = Visibility.Collapsed;
    }

    private void btnSaveArea_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtAreaName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khu vực!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Logic chính: Kiểm tra _selectedArea để xác định Create hay Update
            if (_selectedArea == null)
            {
                // CREATE MODE: Tạo khu vực mới
                var newArea = new Area
                {
                    AreaName = txtAreaName.Text.Trim()
                };
                _tableService.AddArea(newArea);
                MessageBox.Show("✅ Thêm khu vực mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // UPDATE MODE: Cập nhật khu vực hiện tại
                _selectedArea.AreaName = txtAreaName.Text.Trim();
                _tableService.UpdateArea(_selectedArea);
                MessageBox.Show("✅ Cập nhật khu vực thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadAreas();
            LoadAreasForCombo(); // Reload combo box
            btnAddNewArea_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnDeleteArea_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedArea == null)
        {
            MessageBox.Show("Vui lòng chọn khu vực cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Check if area has tables in use
            var inUseTables = _tableService.GetInUseTablesByArea(_selectedArea.ID);
            var allTables = _tableService.GetTablesByArea(_selectedArea.ID);

            if (inUseTables.Any())
            {
                var result = MessageBox.Show(
                    $"Khu vực \"{_selectedArea.AreaName}\" có {inUseTables.Count} bàn đang được sử dụng.\n\n" +
                    $"Bạn muốn:\n" +
                    $"• Có - Xóa luôn tất cả {allTables.Count} bàn thuộc khu vực này và xóa khu vực\n" +
                    $"• Không - Hủy thao tác xóa\n\n" +
                    $"Lưu ý: Nếu chọn 'Có', tất cả các bàn (kể cả bàn đang sử dụng) sẽ bị xóa vĩnh viễn!",
                    "Cảnh báo",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _tableService.DeleteArea(_selectedArea.ID, deleteTables: true);
                    MessageBox.Show($"✅ Đã xóa khu vực \"{_selectedArea.AreaName}\" và {allTables.Count} bàn thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAreas();
                    LoadAreasForCombo();
                    LoadTables(); // Reload tables
                    btnAddNewArea_Click(sender, e);
                }
            }
            else if (allTables.Any())
            {
                var result = MessageBox.Show(
                    $"Khu vực \"{_selectedArea.AreaName}\" có {allTables.Count} bàn.\n\n" +
                    $"Bạn muốn:\n" +
                    $"• Có - Xóa luôn tất cả {allTables.Count} bàn thuộc khu vực này và xóa khu vực\n" +
                    $"• Không - Hủy thao tác xóa",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _tableService.DeleteArea(_selectedArea.ID, deleteTables: true);
                    MessageBox.Show($"✅ Đã xóa khu vực \"{_selectedArea.AreaName}\" và {allTables.Count} bàn thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAreas();
                    LoadAreasForCombo();
                    LoadTables(); // Reload tables
                    btnAddNewArea_Click(sender, e);
                }
            }
            else
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa khu vực \"{_selectedArea.AreaName}\"?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _tableService.DeleteArea(_selectedArea.ID);
                    MessageBox.Show("✅ Xóa khu vực thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAreas();
                    LoadAreasForCombo();
                    btnAddNewArea_Click(sender, e);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi khi xóa khu vực: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region TableTypes Management

    private void LoadTableTypes()
    {
        try
        {
            var tableTypes = _tableService.GetAllTableTypes();
            dgTableTypes.ItemsSource = tableTypes;
            dgTableTypes.SelectedItem = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách loại bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgTableTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedTableType = dgTableTypes.SelectedItem as TableType;
        if (_selectedTableType != null)
        {
            // 1. Đổ dữ liệu từ loại bàn được chọn vào form
            txtTableTypeName.Text = _selectedTableType.TypeName;

            // 2. Thay đổi trạng thái nút (Chuyển sang Edit Mode)
            btnSaveTableType.Content = "Cập nhật";
            btnDeleteTableType.Visibility = Visibility.Visible;
        }
        else
        {
            // Không có loại bàn nào được chọn, chuyển về Create Mode
            UpdateUIForTableTypeCreateMode();
        }
    }

    private void btnAddNewTableType_Click(object sender, RoutedEventArgs e)
    {
        // Chuyển sang Create Mode
        _selectedTableType = null;
        dgTableTypes.SelectedItem = null;

        // 1. Xóa trắng Form
        ClearTableTypeForm();

        // 2. Cập nhật UI cho Create Mode
        UpdateUIForTableTypeCreateMode();

        // 3. Tự động focus vào ô nhập
        txtTableTypeName.Focus();
    }

    private void ClearTableTypeForm()
    {
        txtTableTypeName.Text = string.Empty;
    }

    private void UpdateUIForTableTypeCreateMode()
    {
        btnSaveTableType.Content = "Lưu mới";
        btnDeleteTableType.Visibility = Visibility.Collapsed;
    }

    private void btnSaveTableType_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtTableTypeName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Logic chính: Kiểm tra _selectedTableType để xác định Create hay Update
            if (_selectedTableType == null)
            {
                // CREATE MODE: Tạo loại bàn mới
                var newTableType = new TableType
                {
                    TypeName = txtTableTypeName.Text.Trim()
                };
                _tableService.AddTableType(newTableType);
                MessageBox.Show("✅ Thêm loại bàn mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // UPDATE MODE: Cập nhật loại bàn hiện tại
                _selectedTableType.TypeName = txtTableTypeName.Text.Trim();
                _tableService.UpdateTableType(_selectedTableType);
                MessageBox.Show("✅ Cập nhật loại bàn thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadTableTypes();
            LoadTableTypesForCombo(); // Reload combo box
            btnAddNewTableType_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnDeleteTableType_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTableType == null)
        {
            MessageBox.Show("Vui lòng chọn loại bàn cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa loại bàn \"{_selectedTableType.TypeName}\"?\n\nLưu ý: Nếu loại bàn này đang được sử dụng bởi các bàn hoặc quy tắc giá, việc xóa có thể gây lỗi.",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _tableService.DeleteTableType(_selectedTableType.ID);
                MessageBox.Show("✅ Xóa loại bàn thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTableTypes();
                LoadTableTypesForCombo(); // Reload combo box
                btnAddNewTableType_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi xóa loại bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion
}



