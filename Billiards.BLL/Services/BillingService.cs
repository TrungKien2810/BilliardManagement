using Billiards.DAL;
using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class BillingService
{
    private readonly InvoiceRepository _invoiceRepository;
    private readonly PricingRepository _pricingRepository;
    private readonly TableRepository _tableRepository;
    private readonly AppDbContext _context;

    public BillingService()
    {
        _context = new AppDbContext();
        _invoiceRepository = new InvoiceRepository(_context);
        _pricingRepository = new PricingRepository(_context);
        _tableRepository = new TableRepository(_context);
    }

    public decimal CalculateTableFee(int invoiceId)
    {
        return CalculateTableFee(invoiceId, DateTime.Now);
    }

    /// <summary>
    /// Tính TableFee với EndTime cụ thể
    /// </summary>
    private decimal CalculateTableFee(int invoiceId, DateTime endTime)
    {
        var invoice = _invoiceRepository.GetById(invoiceId);
        if (invoice == null || invoice.TableID == null)
        {
            return 0;
        }

        var table = _tableRepository.GetById(invoice.TableID.Value);
        if (table == null || table.TypeID == null)
        {
            return 0;
        }

        var startTime = invoice.StartTime;
        var totalTime = endTime - startTime;

        if (totalTime.TotalMinutes <= 0)
        {
            return 0;
        }

        // Lấy pricing rules cho loại bàn này
        var rules = _pricingRepository.GetRules(table.TypeID.Value);
        if (rules.Count == 0)
        {
            return 0;
        }

        return CalculateTableFeeInternal(startTime, endTime, rules);
    }

    /// <summary>
    /// Tính TableFee với StartTime, EndTime và Rules cụ thể (internal method)
    /// </summary>
    private decimal CalculateTableFeeInternal(DateTime startTime, DateTime endTime, List<HourlyPricingRule> rules)
    {
        var totalTime = endTime - startTime;

        if (totalTime.TotalMinutes <= 0)
        {
            return 0;
        }

        // Tính tiền theo thời gian thực tế (tính theo phút, không làm tròn lên)
        decimal totalFee = 0;
        var totalMinutes = totalTime.TotalMinutes;

        if (totalMinutes <= 0)
        {
            return 0;
        }

        // Tính tiền: lặp qua từng phút và tính theo giá tương ứng
        // Cách này đảm bảo tính chính xác theo từng khung giờ
        var totalMinutesInt = (int)Math.Floor(totalMinutes);
        var remainingSeconds = (totalMinutes - totalMinutesInt) * 60; // Phần giây còn lại
        
        for (int minute = 0; minute < totalMinutesInt; minute++)
        {
            var checkTime = startTime.AddMinutes(minute);
            var timeOfDay = checkTime.TimeOfDay;
            
            // Tìm rule phù hợp với thời điểm này
            HourlyPricingRule? applicableRule = GetApplicableRule(rules, timeOfDay);
            
            decimal pricePerHour = applicableRule?.PricePerHour ?? rules.First().PricePerHour;
            // Tính tiền cho 1 phút = giá/giờ / 60
            totalFee += pricePerHour / 60m;
        }
        
        // Tính phần thời gian còn lại (giây) nếu có
        if (remainingSeconds > 0)
        {
            var lastMinuteTime = startTime.AddMinutes(totalMinutesInt);
            var timeOfDay = lastMinuteTime.TimeOfDay;
            HourlyPricingRule? applicableRule = GetApplicableRule(rules, timeOfDay);
            
            decimal pricePerHour = applicableRule?.PricePerHour ?? rules.First().PricePerHour;
            // Tính tiền cho phần giây còn lại: (số giây / 3600) * giá/giờ
            totalFee += (pricePerHour / 3600m) * (decimal)remainingSeconds;
        }

        // Làm tròn đến số nguyên (đồng)
        return Math.Round(totalFee, 0);
    }

    private HourlyPricingRule? GetApplicableRule(List<HourlyPricingRule> rules, TimeSpan timeOfDay)
    {
        foreach (var rule in rules)
        {
            // Xử lý rule trong cùng một ngày (ví dụ: 08:00 - 22:00)
            if (rule.StartTimeSlot <= rule.EndTimeSlot)
            {
                // Rule bình thường: StartTimeSlot < EndTimeSlot
                if (timeOfDay >= rule.StartTimeSlot && timeOfDay < rule.EndTimeSlot)
                {
                    return rule;
                }
            }
            else
            {
                // Xử lý rule vượt qua nửa đêm (ví dụ: 22:00 - 08:00 ngày hôm sau)
                // Trong SQL Server, TIME type không thể lưu 24:00:00, nên nếu StartTimeSlot > EndTimeSlot
                // thì có nghĩa là rule vượt qua nửa đêm
                // Ví dụ: StartTimeSlot = 22:00:00, EndTimeSlot = 00:00:00 (thực tế là 24:00:00)
                if (timeOfDay >= rule.StartTimeSlot || timeOfDay < rule.EndTimeSlot)
                {
                    return rule;
                }
            }
        }
        
        return null;
    }

    public Invoice GetInvoiceForCheckout(int tableId)
    {
        var activeInvoice = _invoiceRepository.GetActiveInvoiceByTable(tableId);
        if (activeInvoice == null)
        {
            throw new Exception($"Không tìm thấy hóa đơn đang active cho bàn {tableId}");
        }

        // Tính lại TableFee
        activeInvoice.TableFee = CalculateTableFee(activeInvoice.ID);

        // Lấy ProductFee (đã có từ Task 03)
        // ProductFee đã được cập nhật trong OrderService

        // Tính TotalAmount
        activeInvoice.TotalAmount = activeInvoice.TableFee + activeInvoice.ProductFee - activeInvoice.Discount;

        return activeInvoice;
    }

    public bool FinalizeCheckout(int invoiceId, decimal discount, int? customerId, int? pointsToRedeem = null)
    {
        try
        {
            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                return false;
            }

            // Kiểm tra invoice phải đang Active
            if (invoice.Status != "Active")
            {
                throw new Exception("Hóa đơn không ở trạng thái Active. Không thể thanh toán.");
            }

            // Lưu TableID trước khi cập nhật
            var tableId = invoice.TableID;

            // Set EndTime trước khi tính TableFee
            invoice.EndTime = DateTime.Now;

            // Tính lại TableFee với EndTime mới (quan trọng: phải tính lại trước khi lưu vào DB)
            if (invoice.TableID.HasValue)
            {
                // Tính TableFee dựa trên StartTime và EndTime
                invoice.TableFee = CalculateTableFee(invoice.ID, invoice.EndTime.Value);
            }

            // Cập nhật thông tin thanh toán
            invoice.Discount = discount;
            invoice.CustomerID = customerId;
            invoice.Status = "Paid";
            
            // Tính lại TotalAmount (sau khi đã cập nhật TableFee)
            invoice.TotalAmount = invoice.TableFee + invoice.ProductFee - invoice.Discount;
            if (invoice.TotalAmount < 0) invoice.TotalAmount = 0;

            // Trừ điểm nếu có (trong transaction)
            if (customerId.HasValue && pointsToRedeem.HasValue && pointsToRedeem.Value > 0)
            {
                var loyaltyService = new LoyaltyService();
                loyaltyService.RedeemPoints(customerId.Value, pointsToRedeem.Value);
            }

            // Cập nhật invoice vào database
            _invoiceRepository.Update(invoice);

            // Cập nhật trạng thái bàn về Free (sau khi đã cập nhật invoice)
            if (tableId.HasValue)
            {
                _tableRepository.UpdateTableStatus(tableId.Value, "Free");
            }

            return true;
        }
        catch (Exception ex)
        {
            // Log lỗi nếu cần
            throw new Exception($"Lỗi khi thanh toán: {ex.Message}", ex);
        }
    }


    public List<InvoiceDetail> GetInvoiceDetails(int invoiceId)
    {
        return _invoiceRepository.GetInvoiceDetails(invoiceId);
    }
}

