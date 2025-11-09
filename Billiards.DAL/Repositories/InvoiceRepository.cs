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
}

