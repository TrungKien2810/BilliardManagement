using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class DashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository()
    {
        _context = new AppDbContext();
    }

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy doanh thu hôm nay
    /// </summary>
    public decimal GetTodayRevenue()
    {
        _context.ChangeTracker.Clear();
        var today = DateTime.Now.Date;
        var tomorrow = today.AddDays(1);

        return _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == "Paid"
                       && i.EndTime.HasValue
                       && i.EndTime.Value >= today
                       && i.EndTime.Value < tomorrow)
            .Sum(i => i.TotalAmount);
    }

    /// <summary>
    /// Lấy doanh thu tháng này
    /// </summary>
    public decimal GetMonthRevenue()
    {
        _context.ChangeTracker.Clear();
        var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

        return _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == "Paid"
                       && i.EndTime.HasValue
                       && i.EndTime.Value >= firstDayOfMonth
                       && i.EndTime.Value < firstDayOfNextMonth)
            .Sum(i => i.TotalAmount);
    }

    /// <summary>
    /// Lấy số bàn đang sử dụng
    /// </summary>
    public int GetInUseTablesCount()
    {
        _context.ChangeTracker.Clear();
        return _context.Tables
            .AsNoTracking()
            .Count(t => t.Status == "InUse");
    }

    /// <summary>
    /// Lấy tổng số bàn
    /// </summary>
    public int GetTotalTablesCount()
    {
        _context.ChangeTracker.Clear();
        return _context.Tables
            .AsNoTracking()
            .Count();
    }

    /// <summary>
    /// Lấy số đơn hàng hôm nay
    /// </summary>
    public int GetTodayInvoicesCount()
    {
        _context.ChangeTracker.Clear();
        var today = DateTime.Now.Date;
        var tomorrow = today.AddDays(1);

        return _context.Invoices
            .AsNoTracking()
            .Count(i => i.Status == "Paid"
                       && i.EndTime.HasValue
                       && i.EndTime.Value >= today
                       && i.EndTime.Value < tomorrow);
    }

    /// <summary>
    /// Lấy số đơn hàng đang active
    /// </summary>
    public int GetActiveInvoicesCount()
    {
        _context.ChangeTracker.Clear();
        return _context.Invoices
            .AsNoTracking()
            .Count(i => i.Status == "Active");
    }

    /// <summary>
    /// Lấy top 5 sản phẩm bán chạy hôm nay
    /// </summary>
    public List<TopProduct> GetTopProductsToday(int topCount = 5)
    {
        _context.ChangeTracker.Clear();
        var today = DateTime.Now.Date;
        var tomorrow = today.AddDays(1);

        return _context.InvoiceDetails
            .AsNoTracking()
            .Include(id => id.Product)
            .Include(id => id.Invoice)
            .Where(id => id.Invoice.Status == "Paid"
                        && id.Invoice.EndTime.HasValue
                        && id.Invoice.EndTime.Value >= today
                        && id.Invoice.EndTime.Value < tomorrow)
            .GroupBy(id => new { id.ProductID, id.Product.ProductName })
            .Select(g => new TopProduct
            {
                ProductId = g.Key.ProductID,
                ProductName = g.Key.ProductName,
                TotalQuantity = g.Sum(id => id.Quantity),
                TotalRevenue = g.Sum(id => id.Quantity * id.UnitPrice)
            })
            .OrderByDescending(p => p.TotalQuantity)
            .Take(topCount)
            .ToList();
    }

    /// <summary>
    /// Lấy doanh thu theo ngày trong tháng (cho biểu đồ)
    /// </summary>
    public List<DailyRevenue> GetDailyRevenueThisMonth()
    {
        _context.ChangeTracker.Clear();
        var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

        return _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == "Paid"
                       && i.EndTime.HasValue
                       && i.EndTime.Value >= firstDayOfMonth
                       && i.EndTime.Value < firstDayOfNextMonth)
            .GroupBy(i => i.EndTime.Value.Date)
            .Select(g => new DailyRevenue
            {
                Date = g.Key,
                Revenue = g.Sum(i => i.TotalAmount),
                InvoiceCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();
    }
}

/// <summary>
/// Model cho top sản phẩm
/// </summary>
public class TopProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// Model cho doanh thu theo ngày
/// </summary>
public class DailyRevenue
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int InvoiceCount { get; set; }
}

