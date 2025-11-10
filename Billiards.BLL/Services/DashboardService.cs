using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class DashboardService
{
    private readonly DashboardRepository _dashboardRepository;

    public DashboardService()
    {
        _dashboardRepository = new DashboardRepository();
    }

    public DashboardService(DashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public decimal GetTodayRevenue()
    {
        return _dashboardRepository.GetTodayRevenue();
    }

    public decimal GetMonthRevenue()
    {
        return _dashboardRepository.GetMonthRevenue();
    }

    public int GetInUseTablesCount()
    {
        return _dashboardRepository.GetInUseTablesCount();
    }

    public int GetTotalTablesCount()
    {
        return _dashboardRepository.GetTotalTablesCount();
    }

    public int GetTodayInvoicesCount()
    {
        return _dashboardRepository.GetTodayInvoicesCount();
    }

    public int GetActiveInvoicesCount()
    {
        return _dashboardRepository.GetActiveInvoicesCount();
    }

    public List<TopProduct> GetTopProductsToday(int topCount = 5)
    {
        return _dashboardRepository.GetTopProductsToday(topCount);
    }

    public List<DailyRevenue> GetDailyRevenueThisMonth()
    {
        return _dashboardRepository.GetDailyRevenueThisMonth();
    }
}

