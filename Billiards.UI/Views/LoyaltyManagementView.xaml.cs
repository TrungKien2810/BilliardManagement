using System;
using System.Windows;
using System.Windows.Controls;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Views;

public partial class LoyaltyManagementView : UserControl
{
    private readonly LoyaltyService _loyaltyService;
    private LoyaltyRule? _currentRule;

    public LoyaltyManagementView()
    {
        InitializeComponent();
        _loyaltyService = new LoyaltyService();
        Loaded += LoyaltyManagementView_Loaded;
    }

    private void LoyaltyManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        LoadCurrentRule();
        UpdateExample();
    }

    private void LoadCurrentRule()
    {
        try
        {
            _currentRule = _loyaltyService.GetActiveRule();
            
            if (_currentRule == null)
            {
                // Nếu chưa có quy tắc, tạo mới với giá trị mặc định
                _currentRule = new LoyaltyRule
                {
                    PointsPerAmount = 10000,
                    AmountPerPoint = 100,
                    MinimumPointsToRedeem = 1000,
                    IsActive = true
                };
            }

            // Đổ dữ liệu vào form
            txtPointsPerAmount.Text = _currentRule.PointsPerAmount.ToString("N0");
            txtAmountPerPoint.Text = _currentRule.AmountPerPoint.ToString("N0");
            txtMinimumPointsToRedeem.Text = _currentRule.MinimumPointsToRedeem.ToString("N0");
            chkIsActive.IsChecked = _currentRule.IsActive;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải quy tắc tích điểm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateExample()
    {
        try
        {
            var pointsPerAmount = 10000m;
            var amountPerPoint = 100m;
            var minPoints = 1000;

            if (decimal.TryParse(txtPointsPerAmount.Text?.Replace(",", "").Replace(".", ""), out decimal ppa))
            {
                pointsPerAmount = ppa;
            }

            if (decimal.TryParse(txtAmountPerPoint.Text?.Replace(",", "").Replace(".", ""), out decimal app))
            {
                amountPerPoint = app;
            }

            if (int.TryParse(txtMinimumPointsToRedeem.Text?.Replace(",", "").Replace(".", ""), out int mpt))
            {
                minPoints = mpt;
            }

            // Tính ví dụ
            var exampleAmount = 500000m; // 500.000đ
            var pointsEarned = (int)Math.Floor(exampleAmount / pointsPerAmount);
            var discountFromPoints = minPoints * amountPerPoint;

            txtExample.Text = $"Ví dụ: Khách hàng thanh toán {exampleAmount:N0} VNĐ\n" +
                             $"→ Tích được: {pointsEarned} điểm ({exampleAmount:N0} / {pointsPerAmount:N0})\n" +
                             $"→ Với {minPoints:N0} điểm, khách hàng có thể đổi: {discountFromPoints:N0} VNĐ giảm giá\n" +
                             $"→ ({minPoints:N0} điểm × {amountPerPoint:N0} VNĐ/điểm)";
        }
        catch
        {
            txtExample.Text = "Vui lòng nhập đúng định dạng số";
        }
    }

    private void txtPointsPerAmount_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateExample();
    }

    private void txtAmountPerPoint_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateExample();
    }

    private void txtMinimumPointsToRedeem_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateExample();
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate
            if (!decimal.TryParse(txtPointsPerAmount.Text?.Replace(",", "").Replace(".", ""), out decimal pointsPerAmount) || pointsPerAmount <= 0)
            {
                MessageBox.Show("Quy tắc tích điểm không hợp lệ! Vui lòng nhập số dương.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPointsPerAmount.Focus();
                return;
            }

            if (!decimal.TryParse(txtAmountPerPoint.Text?.Replace(",", "").Replace(".", ""), out decimal amountPerPoint) || amountPerPoint <= 0)
            {
                MessageBox.Show("Quy tắc đổi điểm không hợp lệ! Vui lòng nhập số dương.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtAmountPerPoint.Focus();
                return;
            }

            if (!int.TryParse(txtMinimumPointsToRedeem.Text?.Replace(",", "").Replace(".", ""), out int minimumPoints) || minimumPoints < 0)
            {
                MessageBox.Show("Số điểm tối thiểu không hợp lệ! Vui lòng nhập số nguyên không âm.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtMinimumPointsToRedeem.Focus();
                return;
            }

            // Cập nhật hoặc tạo mới quy tắc
            if (_currentRule == null || _currentRule.ID == 0)
            {
                // Tạo mới
                _currentRule = new LoyaltyRule
                {
                    PointsPerAmount = pointsPerAmount,
                    AmountPerPoint = amountPerPoint,
                    MinimumPointsToRedeem = minimumPoints,
                    IsActive = chkIsActive.IsChecked ?? true
                };
                _loyaltyService.AddRule(_currentRule);
                _loyaltyService.SetActiveRule(_currentRule.ID);
                MessageBox.Show("✅ Đã tạo quy tắc tích điểm mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Cập nhật
                _currentRule.PointsPerAmount = pointsPerAmount;
                _currentRule.AmountPerPoint = amountPerPoint;
                _currentRule.MinimumPointsToRedeem = minimumPoints;
                _currentRule.IsActive = chkIsActive.IsChecked ?? true;
                _loyaltyService.UpdateRule(_currentRule);
                _loyaltyService.SetActiveRule(_currentRule.ID);
                MessageBox.Show("✅ Đã cập nhật quy tắc tích điểm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Reload để đảm bảo dữ liệu nhất quán
            LoadCurrentRule();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu quy tắc tích điểm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

