using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class OrderManagementService
{
    private readonly InvoiceRepository _invoiceRepository;

    public OrderManagementService()
    {
        _invoiceRepository = new InvoiceRepository();
    }

    public OrderManagementService(InvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    /// <summary>
    /// Lấy tất cả invoices với filter
    /// </summary>
    public List<Invoice> GetAllInvoices(DateTime? startDate = null, DateTime? endDate = null, string? status = null, 
        int? tableId = null, int? employeeId = null, int? customerId = null)
    {
        return _invoiceRepository.GetAllInvoices(startDate, endDate, status, tableId, employeeId, customerId);
    }

    /// <summary>
    /// Lấy invoice by ID với đầy đủ thông tin
    /// </summary>
    public Invoice? GetInvoiceWithDetails(int invoiceId)
    {
        return _invoiceRepository.GetInvoiceWithDetails(invoiceId);
    }

    /// <summary>
    /// Hủy invoice
    /// </summary>
    public bool CancelInvoice(int invoiceId)
    {
        return _invoiceRepository.CancelInvoice(invoiceId);
    }
}

