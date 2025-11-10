using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Billiards.BLL.Services;
using Billiards.DAL.Models;

namespace Billiards.UI.Windows
{
    public partial class CheckoutWindow : Window
    {
        private Invoice _currentInvoice;
        private BillingService _billingService;
        private LoyaltyService _loyaltyService;
        private CustomerService _customerService;

        private int _tableId;
        private Customer? _currentCustomer;
        private int _redeemedPoints = 0; // Số điểm đã đổi
        private decimal _pointsDiscount = 0; // Số tiền giảm giá từ đổi điểm

        public CheckoutWindow(int tableId)
        {
            InitializeComponent();
            _billingService = new BillingService();
            _loyaltyService = new LoyaltyService();
            _customerService = new CustomerService();
            _tableId = tableId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load dữ liệu trong Window_Loaded để không block UI khi khởi tạo
                _currentInvoice = _billingService.GetInvoiceForCheckout(_tableId);
                
                if (_currentInvoice == null)
                {
                    MessageBox.Show("Không tìm thấy hóa đơn cho bàn này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                LoadInvoiceData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy thông tin hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void LoadInvoiceData()
        {
            // Thông tin thời gian
            txtStartTime.Text = _currentInvoice.StartTime.ToString("dd/MM/yyyy HH:mm:ss");
            txtEndTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            
            var totalTime = DateTime.Now - _currentInvoice.StartTime;
            var hours = (int)totalTime.TotalHours;
            var minutes = totalTime.Minutes;
            txtTotalTime.Text = $"{hours} giờ {minutes} phút";

            // Tiền giờ
            txtTableFee.Text = $"{_currentInvoice.TableFee:N0} VNĐ";

            // Danh sách sản phẩm
            var invoiceDetails = _billingService.GetInvoiceDetails(_currentInvoice.ID);
            dgProducts.ItemsSource = invoiceDetails.Select(id => new InvoiceDetailViewModel
            {
                Product = id.Product,
                Quantity = id.Quantity,
                UnitPrice = id.UnitPrice,
                Total = id.Quantity * id.UnitPrice
            }).ToList();

            // Tổng tiền dịch vụ
            txtProductFee.Text = $"{_currentInvoice.ProductFee:N0} VNĐ";

            // Tổng cộng
            var subTotal = _currentInvoice.TableFee + _currentInvoice.ProductFee;
            txtSubTotal.Text = $"{subTotal:N0} VNĐ";

            // Giảm giá
            txtDiscount.Text = _currentInvoice.Discount.ToString("N0");

            // Tổng thanh toán
            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            if (_currentInvoice == null) return;

            var tableFee = _currentInvoice.TableFee;
            var productFee = _currentInvoice.ProductFee;
            decimal discount = 0;

            if (decimal.TryParse(txtDiscount.Text?.Replace(",", "").Replace(".", ""), out decimal discountValue))
            {
                // Đảm bảo discount không âm
                discount = Math.Max(0, discountValue);
            }

            // Cộng thêm giảm giá từ đổi điểm
            discount += _pointsDiscount;

            var totalAmount = tableFee + productFee - discount;
            if (totalAmount < 0) totalAmount = 0;

            txtTotalAmount.Text = $"{totalAmount:N0} VNĐ";
        }

        private void txtDiscount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateTotalAmount();
        }

        private void txtDiscount_LostFocus(object sender, RoutedEventArgs e)
        {
            // Validate và đảm bảo giá trị không âm
            if (decimal.TryParse(txtDiscount.Text?.Replace(",", "").Replace(".", ""), out decimal discountValue))
            {
                if (discountValue < 0)
                {
                    MessageBox.Show("Giảm giá không được nhỏ hơn 0!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtDiscount.Text = "0";
                }
                else
                {
                    // Format lại số
                    txtDiscount.Text = discountValue.ToString("N0");
                }
            }
            else
            {
                // Nếu không parse được, set về 0
                txtDiscount.Text = "0";
            }
            
            UpdateTotalAmount();
        }
        
        private void txtDiscount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập số
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        private void txtDiscount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Ngăn nhập dấu trừ (-)
            if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                e.Handled = true;
            }
        }

        private void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy giảm giá và validate
                decimal discount = 0;
                if (decimal.TryParse(txtDiscount.Text?.Replace(",", "").Replace(".", ""), out decimal discountValue))
                {
                    // Đảm bảo discount không âm
                    if (discountValue < 0)
                    {
                        MessageBox.Show("Giảm giá không được nhỏ hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    discount = discountValue;
                }

                // Cộng thêm giảm giá từ đổi điểm
                discount += _pointsDiscount;

                // Liên kết khách hàng
                int? customerId = null;
                int? pointsToRedeem = null;
                if (_currentCustomer != null)
                {
                    customerId = _currentCustomer.ID;
                    if (_redeemedPoints > 0)
                    {
                        pointsToRedeem = _redeemedPoints;
                    }
                }

                // Finalize checkout (trừ điểm sẽ được xử lý trong BillingService)
                var success = _billingService.FinalizeCheckout(_currentInvoice.ID, discount, customerId, pointsToRedeem);
                
                if (success)
                {
                    // Tích điểm sau thanh toán (nếu có khách hàng)
                    if (customerId.HasValue)
                    {
                        try
                        {
                            var totalAmount = _currentInvoice.TableFee + _currentInvoice.ProductFee - discount;
                            var pointsEarned = _loyaltyService.CalculatePointsEarned(totalAmount);
                            if (pointsEarned > 0)
                            {
                                _loyaltyService.EarnPoints(customerId.Value, pointsEarned);
                                MessageBox.Show($"Thanh toán thành công!\n\nĐã tích {pointsEarned:N0} điểm cho khách hàng.", 
                                              "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Không chặn thanh toán nếu tích điểm thất bại
                            MessageBox.Show($"Thanh toán thành công!\n\nLưu ý: Không thể tích điểm: {ex.Message}", 
                                          "Thành công", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Thanh toán thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thanh toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void txtCustomerPhone_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CheckCustomerPhone();
        }

        private void txtCustomerPhone_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckCustomerPhone();
            }
        }

        private void CheckCustomerPhone()
        {
            try
            {
                var phone = txtCustomerPhone.Text?.Trim();
                _currentCustomer = null;
                _redeemedPoints = 0;
                _pointsDiscount = 0;
                txtPointsDiscountInfo.Visibility = Visibility.Collapsed;
                btnRedeemPoints.Visibility = Visibility.Collapsed;
                btnAddCustomer.Visibility = Visibility.Collapsed;
                txtCustomerInfo.Text = string.Empty;
                txtCustomerPoints.Text = string.Empty;

                if (string.IsNullOrWhiteSpace(phone))
                {
                    return;
                }

                // Tìm khách hàng
                var existing = _customerService.GetCustomerByPhoneNumber(phone);
                if (existing != null)
                {
                    _currentCustomer = existing;
                    txtCustomerInfo.Text = $"✓ Khách hàng: {existing.FullName}";
                    txtCustomerInfo.Foreground = System.Windows.Media.Brushes.Green;
                    
                    // Hiển thị điểm hiện tại
                    txtCustomerPoints.Text = $"⭐ Điểm hiện tại: {existing.LoyaltyPoints:N0} điểm";
                    
                    // Kiểm tra xem có đủ điểm để đổi không
                    if (_loyaltyService.CanRedeemPoints(existing.LoyaltyPoints))
                    {
                        btnRedeemPoints.Visibility = Visibility.Visible;
                        btnRedeemPoints.IsEnabled = true;
                    }
                    else
                    {
                        var minPoints = _loyaltyService.GetMinimumPointsToRedeem();
                        txtCustomerPoints.Text += $"\n⚠️ Cần ít nhất {minPoints:N0} điểm để đổi";
                        txtCustomerPoints.Foreground = System.Windows.Media.Brushes.Orange;
                    }
                }
                else
                {
                    txtCustomerInfo.Text = "⚠️ Không tìm thấy khách hàng";
                    txtCustomerInfo.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    btnAddCustomer.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                txtCustomerInfo.Text = $"❌ Lỗi: {ex.Message}";
                txtCustomerInfo.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void btnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var phone = txtCustomerPhone.Text?.Trim();
                if (string.IsNullOrWhiteSpace(phone))
                {
                    MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Tạo dialog nhập tên khách hàng
                var inputDialog = new InputDialog(
                    "Nhập tên khách hàng:",
                    "Thêm khách hàng mới",
                    "");
                
                if (inputDialog.ShowDialog() == true)
                {
                    var customerName = inputDialog.Answer?.Trim();
                    if (string.IsNullOrWhiteSpace(customerName))
                    {
                        MessageBox.Show("Tên khách hàng không được để trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Tạo khách hàng mới
                    var newCustomer = new Customer
                    {
                        FullName = customerName,
                        PhoneNumber = phone,
                        LoyaltyPoints = 0
                    };

                    _customerService.AddCustomer(newCustomer);
                    _currentCustomer = newCustomer;
                    
                    MessageBox.Show("Đã thêm khách hàng mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Cập nhật lại UI
                    CheckCustomerPhone();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm khách hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRedeemPoints_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentCustomer == null)
                {
                    MessageBox.Show("Không tìm thấy khách hàng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var rule = _loyaltyService.GetActiveRule();
                if (rule == null || !rule.IsActive)
                {
                    MessageBox.Show("Hệ thống tích điểm chưa được kích hoạt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Tính số tiền giảm giá tối đa có thể đổi
                var maxDiscount = _loyaltyService.CalculateDiscountFromPoints(_currentCustomer.LoyaltyPoints);
                
                // Tính tổng tiền hiện tại (chưa giảm giá từ điểm)
                var currentDiscount = 0m;
                if (decimal.TryParse(txtDiscount.Text?.Replace(",", "").Replace(".", ""), out decimal discountValue))
                {
                    currentDiscount = Math.Max(0, discountValue);
                }
                var subTotal = _currentInvoice.TableFee + _currentInvoice.ProductFee - currentDiscount;
                
                // Không thể giảm nhiều hơn tổng tiền
                var actualDiscount = Math.Min(maxDiscount, subTotal);
                
                // Tính số điểm cần đổi
                var pointsToRedeem = (int)Math.Ceiling(actualDiscount / rule.AmountPerPoint);
                var actualDiscountFromPoints = pointsToRedeem * rule.AmountPerPoint;
                
                // Xác nhận với người dùng
                var confirmMessage = $"Bạn có {_currentCustomer.LoyaltyPoints:N0} điểm.\n\n" +
                                    $"Bạn muốn đổi {pointsToRedeem:N0} điểm để giảm {actualDiscountFromPoints:N0} VNĐ?\n\n" +
                                    $"Điểm còn lại sau khi đổi: {_currentCustomer.LoyaltyPoints - pointsToRedeem:N0} điểm";
                
                var result = MessageBox.Show(confirmMessage, "Xác nhận đổi điểm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Cập nhật điểm và giảm giá
                    _redeemedPoints = pointsToRedeem;
                    _pointsDiscount = actualDiscountFromPoints;
                    
                    // Hiển thị thông tin
                    txtPointsDiscountInfo.Text = $"💰 Đã đổi {pointsToRedeem:N0} điểm để giảm {actualDiscountFromPoints:N0} VNĐ";
                    txtPointsDiscountInfo.Visibility = Visibility.Visible;
                    
                    // Cập nhật điểm hiển thị
                    txtCustomerPoints.Text = $"⭐ Điểm hiện tại: {_currentCustomer.LoyaltyPoints:N0} điểm\n" +
                                            $"➖ Sẽ trừ: {pointsToRedeem:N0} điểm\n" +
                                            $"✅ Điểm còn lại: {_currentCustomer.LoyaltyPoints - pointsToRedeem:N0} điểm";
                    txtCustomerPoints.Foreground = System.Windows.Media.Brushes.Green;
                    
                    // Vô hiệu hóa nút đổi điểm (đã đổi rồi)
                    btnRedeemPoints.IsEnabled = false;
                    
                    // Cập nhật tổng thanh toán
                    UpdateTotalAmount();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đổi điểm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Helper class for invoice detail display
    public class InvoiceDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
