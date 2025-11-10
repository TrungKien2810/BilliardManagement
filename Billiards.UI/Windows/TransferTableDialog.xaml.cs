using System;
using System.Linq;
using System.Windows;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Windows;

public partial class TransferTableDialog : Window
{
    private readonly TableService _tableService;
    private readonly int _fromTableId;
    private readonly string _fromTableName;

    public int? SelectedTableId { get; private set; }

    public TransferTableDialog(int fromTableId, string fromTableName)
    {
        InitializeComponent();
        _tableService = new TableService();
        _fromTableId = fromTableId;
        _fromTableName = fromTableName;

        Loaded += TransferTableDialog_Loaded;
    }

    private void TransferTableDialog_Loaded(object sender, RoutedEventArgs e)
    {
        // Set info text
        txtInfo.Text = $"Chọn bàn đích để chuyển từ {_fromTableName}:";

        // Load available tables (only Free tables, excluding current table)
        try
        {
            var allTables = _tableService.GetTableMap();
            var availableTables = allTables
                .Where(t => t.Status == "Free" && t.ID != _fromTableId)
                .OrderBy(t => t.TableName)
                .ToList();

            if (availableTables.Count == 0)
            {
                txtInfo.Text = $"Không có bàn trống nào để chuyển từ {_fromTableName}.";
                lbTables.IsEnabled = false;
            }
            else
            {
                lbTables.ItemsSource = availableTables;
                lbTables.SelectionChanged += LbTables_SelectionChanged;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            DialogResult = false;
            Close();
        }
    }

    private void LbTables_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        btnConfirm.IsEnabled = lbTables.SelectedItem != null;
    }

    private void btnConfirm_Click(object sender, RoutedEventArgs e)
    {
        var selectedTable = lbTables.SelectedItem as Table;
        if (selectedTable == null)
        {
            MessageBox.Show("Vui lòng chọn một bàn để chuyển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedTableId = selectedTable.ID;
        DialogResult = true;
        Close();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

