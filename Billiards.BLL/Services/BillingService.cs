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

        var endTime = DateTime.Now;
        var startTime = invoice.StartTime;
        var totalTime = endTime - startTime;

        if (totalTime.TotalHours <= 0)
        {
            return 0;
        }

        // Lấy pricing rules cho loại bàn này
        var rules = _pricingRepository.GetRules(table.TypeID.Value);
        if (rules.Count == 0)
        {
            return 0;
        }

        // Tính tiền đơn giản: tính từng giờ và áp dụng giá theo khung giờ
        decimal totalFee = 0;
        var totalHours = (int)Math.Ceiling(totalTime.TotalHours); // Làm tròn lên
        
        // Lấy giá trung bình hoặc giá đầu tiên nếu không có rule phù hợp
        var defaultPrice = rules.First().PricePerHour;
        
        // Tính tiền theo từng giờ
        for (int hour = 0; hour < totalHours; hour++)
        {
            var hourTime = startTime.AddHours(hour);
            var hourTimeOfDay = hourTime.TimeOfDay;
            
            // Tìm rule phù hợp với giờ này
            var applicableRule = rules.FirstOrDefault(r => 
                hourTimeOfDay >= r.StartTimeSlot && hourTimeOfDay < r.EndTimeSlot);
            
            if (applicableRule == null)
            {
                // Nếu không có rule phù hợp, dùng giá mặc định
                totalFee += defaultPrice;
            }
            else
            {
                totalFee += applicableRule.PricePerHour;
            }
        }

        return totalFee;
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

    public bool FinalizeCheckout(int invoiceId, decimal discount, int? customerId)
    {
        try
        {
            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                return false;
            }

            // Lưu TableID trước khi cập nhật
            var tableId = invoice.TableID;

            // Cập nhật thông tin thanh toán
            invoice.Discount = discount;
            invoice.CustomerID = customerId;
            invoice.Status = "Paid";
            invoice.EndTime = DateTime.Now;
            
            // Tính lại TotalAmount
            invoice.TotalAmount = invoice.TableFee + invoice.ProductFee - invoice.Discount;

            // Cập nhật invoice
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

