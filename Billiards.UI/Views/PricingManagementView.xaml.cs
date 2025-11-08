using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Views;

public partial class PricingManagementView : UserControl
{
    private readonly PricingService _pricingService;
    private HourlyPricingRule? _selectedRule;

    public PricingManagementView()
    {
        InitializeComponent();
        _pricingService = new PricingService();
        Loaded += PricingManagementView_Loaded;
    }

    private void PricingManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        LoadTableTypes();
        LoadDataGrid();
    }

    private void LoadTableTypes()
    {
        try
        {
            var tableTypes = _pricingService.GetAllTableTypes();
            cmbTableType.ItemsSource = tableTypes;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải loại bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadDataGrid()
    {
        try
        {
            var rules = _pricingService.GetAllPricingRules();
            dgPricingRules.ItemsSource = rules;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải danh sách quy tắc giá: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgPricingRules_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedRule = dgPricingRules.SelectedItem as HourlyPricingRule;
        if (_selectedRule != null)
        {
            cmbTableType.SelectedValue = _selectedRule.TableTypeID;
            txtStartTime.Text = _selectedRule.StartTimeSlot.ToString(@"hh\:mm");
            txtEndTime.Text = _selectedRule.EndTimeSlot.ToString(@"hh\:mm");
            txtPricePerHour.Text = _selectedRule.PricePerHour.ToString();
        }
    }

    private void btnAddNew_Click(object sender, RoutedEventArgs e)
    {
        _selectedRule = null;
        cmbTableType.SelectedIndex = -1;
        txtStartTime.Text = "08:00";
        txtEndTime.Text = "22:00";
        txtPricePerHour.Text = string.Empty;
        dgPricingRules.SelectedItem = null;
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cmbTableType.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn loại bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParseExact(txtStartTime.Text, "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan startTime))
            {
                MessageBox.Show("Giờ bắt đầu không hợp lệ! Vui lòng nhập theo định dạng HH:mm (ví dụ: 08:00, 22:00)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParseExact(txtEndTime.Text, "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan endTime))
            {
                MessageBox.Show("Giờ kết thúc không hợp lệ! Vui lòng nhập theo định dạng HH:mm (ví dụ: 22:00, 24:00)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (startTime >= endTime)
            {
                MessageBox.Show("Giờ bắt đầu phải nhỏ hơn giờ kết thúc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPricePerHour.Text, out decimal pricePerHour) || pricePerHour < 0)
            {
                MessageBox.Show("Giá mỗi giờ không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedRule != null)
            {
                // Update existing rule
                _selectedRule.TableTypeID = (int)cmbTableType.SelectedValue;
                _selectedRule.StartTimeSlot = startTime;
                _selectedRule.EndTimeSlot = endTime;
                _selectedRule.PricePerHour = pricePerHour;
                _pricingService.UpdatePricingRule(_selectedRule);
                MessageBox.Show("Cập nhật quy tắc giá thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new rule
                var newRule = new HourlyPricingRule
                {
                    TableTypeID = (int)cmbTableType.SelectedValue,
                    StartTimeSlot = startTime,
                    EndTimeSlot = endTime,
                    PricePerHour = pricePerHour
                };
                _pricingService.AddPricingRule(newRule);
                MessageBox.Show("Thêm quy tắc giá thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
        if (_selectedRule == null)
        {
            MessageBox.Show("Vui lòng chọn quy tắc giá cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa quy tắc giá này?\n\nLoại bàn: {_selectedRule.TableType?.TypeName}\nThời gian: {_selectedRule.StartTimeSlot:hh\\:mm} - {_selectedRule.EndTimeSlot:hh\\:mm}\nGiá: {_selectedRule.PricePerHour:N0} VNĐ/giờ",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _pricingService.DeletePricingRule(_selectedRule.ID);
                MessageBox.Show("Xóa quy tắc giá thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDataGrid();
                btnAddNew_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa quy tắc giá: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

