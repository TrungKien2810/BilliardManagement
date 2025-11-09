using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class ReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository()
    {
        _context = new AppDbContext();
    }

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Invoice> GetPaidInvoices(DateTime startDate, DateTime endDate, string? customerName = null, string? employeeName = null)
    {
        // Ensure we don't return stale entities
        _context.ChangeTracker.Clear();

        var query = _context.Invoices
            .AsNoTracking()
            .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Product)
            .Include(i => i.Table)
            .Include(i => i.CreatedByEmployee)
            .Include(i => i.Customer)
            .Where(i => i.Status == "Paid"
                        && i.EndTime.HasValue
                        && i.EndTime.Value >= startDate
                        && i.EndTime.Value <= endDate);

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            query = query.Where(i => i.Customer != null
                                     && i.Customer.FullName != null
                                     && EF.Functions.Like(i.Customer.FullName, $"%{customerName}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeName))
        {
            query = query.Where(i => i.CreatedByEmployee != null
                                     && i.CreatedByEmployee.FullName != null
                                     && EF.Functions.Like(i.CreatedByEmployee.FullName, $"%{employeeName}%"));
        }

        return query
            .OrderBy(i => i.EndTime)
            .ToList();
    }
}


