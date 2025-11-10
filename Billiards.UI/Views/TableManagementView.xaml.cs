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
            txtTableName.Text = _selectedTable.TableName;
            cmbArea.SelectedValue = _selectedTable.AreaID;
            cmbTableType.SelectedValue = _selectedTable.TypeID;
            
            // Set status
            var statusItem = cmbStatus.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == _selectedTable.Status);
            if (statusItem != null)
                cmbStatus.SelectedItem = statusItem;
        }
    }

    private void btnAddNewTable_Click(object sender, RoutedEventArgs e)
    {
        _selectedTable = null;
        txtTableName.Text = string.Empty;
        cmbArea.SelectedIndex = -1;
        cmbTableType.SelectedIndex = -1;
        cmbStatus.SelectedIndex = 0; // Default to Free
        dgTables.SelectedItem = null;
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

            if (_selectedTable != null)
            {
                // Update existing table
                _selectedTable.TableName = txtTableName.Text;
                _selectedTable.AreaID = (int)cmbArea.SelectedValue;
                _selectedTable.TypeID = (int)cmbTableType.SelectedValue;
                _selectedTable.Status = status;
                _tableService.UpdateTable(_selectedTable);
                MessageBox.Show("Cập nhật bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new table
                var newTable = new Table
                {
                    TableName = txtTableName.Text,
                    AreaID = (int)cmbArea.SelectedValue,
                    TypeID = (int)cmbTableType.SelectedValue,
                    Status = status
                };
                _tableService.AddTable(newTable);
                MessageBox.Show("Thêm bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadTables();
            btnAddNewTable_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Xóa bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTables();
                btnAddNewTable_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
            txtAreaName.Text = _selectedArea.AreaName;
        }
    }

    private void btnAddNewArea_Click(object sender, RoutedEventArgs e)
    {
        _selectedArea = null;
        txtAreaName.Text = string.Empty;
        dgAreas.SelectedItem = null;
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

            if (_selectedArea != null)
            {
                // Update existing area
                _selectedArea.AreaName = txtAreaName.Text;
                _tableService.UpdateArea(_selectedArea);
                MessageBox.Show("Cập nhật khu vực thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new area
                var newArea = new Area
                {
                    AreaName = txtAreaName.Text
                };
                _tableService.AddArea(newArea);
                MessageBox.Show("Thêm khu vực thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadAreas();
            LoadAreasForCombo(); // Reload combo box
            btnAddNewArea_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"Đã xóa khu vực \"{_selectedArea.AreaName}\" và {allTables.Count} bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show($"Đã xóa khu vực \"{_selectedArea.AreaName}\" và {allTables.Count} bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Xóa khu vực thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAreas();
                    LoadAreasForCombo();
                    btnAddNewArea_Click(sender, e);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xóa khu vực: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
            txtTableTypeName.Text = _selectedTableType.TypeName;
        }
    }

    private void btnAddNewTableType_Click(object sender, RoutedEventArgs e)
    {
        _selectedTableType = null;
        txtTableTypeName.Text = string.Empty;
        dgTableTypes.SelectedItem = null;
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

            if (_selectedTableType != null)
            {
                // Update existing table type
                _selectedTableType.TypeName = txtTableTypeName.Text;
                _tableService.UpdateTableType(_selectedTableType);
                MessageBox.Show("Cập nhật loại bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new table type
                var newTableType = new TableType
                {
                    TypeName = txtTableTypeName.Text
                };
                _tableService.AddTableType(newTableType);
                MessageBox.Show("Thêm loại bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadTableTypes();
            LoadTableTypesForCombo(); // Reload combo box
            btnAddNewTableType_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Xóa loại bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTableTypes();
                LoadTableTypesForCombo(); // Reload combo box
                btnAddNewTableType_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa loại bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion
}



