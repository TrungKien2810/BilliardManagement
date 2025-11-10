using System;
using System.Collections.Generic;
using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class ReportService
{
    private readonly ReportRepository _reportRepository;

    public ReportService()
    {
        _reportRepository = new ReportRepository();
    }

    public ReportService(ReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public List<Invoice> GetRevenueReport(DateTime startDate, DateTime endDate, 
        int? customerId = null, int? employeeId = null, int? tableId = null)
    {
        return _reportRepository.GetPaidInvoices(startDate, endDate, customerId, employeeId, tableId);
    }
}


