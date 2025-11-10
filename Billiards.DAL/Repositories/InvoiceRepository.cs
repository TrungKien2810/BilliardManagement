using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class InvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository()
    {
        _context = new AppDbContext();
    }

    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public Invoice CreateNewInvoice(int tableId, int employeeId)
    {
        var newInvoice = new Invoice
        {
            TableID = tableId,
            CreatedByEmployeeID = employeeId,
            StartTime = DateTime.Now,
            Status = "Active",
            TableFee = 0,
            ProductFee = 0,
            Discount = 0,
            TotalAmount = 0
        };

        _context.Invoices.Add(newInvoice);
        _context.SaveChanges();

        // ID is automatically set by EF after SaveChanges
        return newInvoice;
    }

    public Invoice? GetActiveInvoiceByTable(int tableId)
    {
        // Clear tracked entities to avoid stale cache after updates from other contexts
        _context.ChangeTracker.Clear();

        return _context.Invoices
            .AsNoTracking()
            .FirstOrDefault(i => i.TableID == tableId && i.Status == "Active");
    }

    public Invoice? GetById(int invoiceId)
    {
        return _context.Invoices
            .Include(i => i.InvoiceDetails)
            .ThenInclude(id => id.Product)
            .FirstOrDefault(i => i.ID == invoiceId);
    }

    public void Update(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        _context.SaveChanges();
    }

    public bool CloseActiveInvoiceForTable(int tableId)
    {
        var active = _context.Invoices.FirstOrDefault(i => i.TableID == tableId && i.Status == "Active");
        if (active == null)
        {
            return false;
        }

        active.Status = "Paid";
        if (!active.EndTime.HasValue)
        {
            active.EndTime = DateTime.Now;
        }
        // đảm bảo TotalAmount nhất quán nếu chưa được tính (giữ nguyên nếu đã có)
        if (active.TotalAmount <= 0)
        {
            active.TotalAmount = (active.TableFee + active.ProductFee) - active.Discount;
            if (active.TotalAmount < 0) active.TotalAmount = 0;
        }

        _context.SaveChanges();
        return true;
    }

    public List<InvoiceDetail> GetInvoiceDetails(int invoiceId)
    {
        return _context.InvoiceDetails
            .Include(id => id.Product)
            .Where(id => id.InvoiceID == invoiceId)
            .ToList();
    }

    /// <summary>
    /// Lấy tất cả invoices với filter
    /// </summary>
    public List<Invoice> GetAllInvoices(DateTime? startDate = null, DateTime? endDate = null, string? status = null, int? tableId = null, int? employeeId = null)
    {
        _context.ChangeTracker.Clear();

        var query = _context.Invoices
            .AsNoTracking()
            .Include(i => i.Table)
            .Include(i => i.CreatedByEmployee)
            .Include(i => i.Customer)
            .Include(i => i.InvoiceDetails)
                .ThenInclude(id => id.Product)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(i => i.StartTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(i => i.StartTime <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (tableId.HasValue)
        {
            query = query.Where(i => i.TableID == tableId.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(i => i.CreatedByEmployeeID == employeeId.Value);
        }

        return query
            .OrderByDescending(i => i.StartTime)
            .ToList();
    }

    /// <summary>
    /// Hủy invoice (chuyển status thành Cancelled)
    /// </summary>
    public bool CancelInvoice(int invoiceId)
    {
        var invoice = _context.Invoices
            .Include(i => i.InvoiceDetails)
                .ThenInclude(id => id.Product)
            .FirstOrDefault(i => i.ID == invoiceId);

        if (invoice == null)
        {
            return false;
        }

        if (invoice.Status == "Paid")
        {
            throw new InvalidOperationException("Không thể hủy hóa đơn đã thanh toán.");
        }

        // Hoàn trả lại số lượng sản phẩm vào kho
        foreach (var detail in invoice.InvoiceDetails)
        {
            if (detail.Product != null)
            {
                detail.Product.StockQuantity += detail.Quantity;
            }
        }

        // Cập nhật status
        invoice.Status = "Cancelled";
        invoice.EndTime = DateTime.Now;

        // Nếu có TableID, đặt bàn về Free
        if (invoice.TableID.HasValue)
        {
            var table = _context.Tables.Find(invoice.TableID.Value);
            if (table != null && table.Status == "InUse")
            {
                table.Status = "Free";
            }
        }

        _context.SaveChanges();
        return true;
    }

    /// <summary>
    /// Lấy invoice by ID với đầy đủ thông tin
    /// </summary>
    public Invoice? GetInvoiceWithDetails(int invoiceId)
    {
        _context.ChangeTracker.Clear();

        return _context.Invoices
            .AsNoTracking()
            .Include(i => i.Table)
                .ThenInclude(t => t!.Area)
            .Include(i => i.Table)
                .ThenInclude(t => t!.TableType)
            .Include(i => i.CreatedByEmployee)
            .Include(i => i.Customer)
            .Include(i => i.InvoiceDetails)
                .ThenInclude(id => id.Product)
            .FirstOrDefault(i => i.ID == invoiceId);
    }
}

