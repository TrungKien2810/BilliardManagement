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

    public List<Invoice> GetPaidInvoices(DateTime startDate, DateTime endDate, 
        int? customerId = null, int? employeeId = null, int? tableId = null)
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

        // Filter by CustomerID (performance: uses index)
        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerID == customerId.Value);
        }

        // Filter by EmployeeID (performance: uses index)
        if (employeeId.HasValue)
        {
            query = query.Where(i => i.CreatedByEmployeeID == employeeId.Value);
        }

        // Filter by TableID (performance: uses index)
        if (tableId.HasValue)
        {
            query = query.Where(i => i.TableID == tableId.Value);
        }

        return query
            .OrderBy(i => i.EndTime)
            .ToList();
    }
}


