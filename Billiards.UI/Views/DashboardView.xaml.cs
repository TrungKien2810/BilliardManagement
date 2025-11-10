using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Billiards.BLL.Services;

namespace Billiards.UI.Views;

public partial class DashboardView : UserControl
{
    private readonly DashboardService _dashboardService;
    private DispatcherTimer? _refreshTimer;

    public DashboardView()
    {
        InitializeComponent();
        _dashboardService = new DashboardService();
        Loaded += DashboardView_Loaded;
    }

    private void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        LoadDashboardData();

        // Auto refresh every 30 seconds
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _refreshTimer.Tick += (s, args) => LoadDashboardData();
        _refreshTimer.Start();
    }

    private void LoadDashboardData()
    {
        try
        {
            // Load statistics
            var todayRevenue = _dashboardService.GetTodayRevenue();
            var monthRevenue = _dashboardService.GetMonthRevenue();
            var inUseTables = _dashboardService.GetInUseTablesCount();
            var totalTables = _dashboardService.GetTotalTablesCount();
            var todayInvoices = _dashboardService.GetTodayInvoicesCount();
            var activeInvoices = _dashboardService.GetActiveInvoicesCount();

            // Update UI
            txtTodayRevenue.Text = $"{todayRevenue:N0} VNĐ";
            txtMonthRevenue.Text = $"{monthRevenue:N0} VNĐ";
            txtInUseTables.Text = inUseTables.ToString();
            txtTotalTables.Text = $"/ {totalTables}";
            txtTodayInvoices.Text = todayInvoices.ToString();
            txtActiveInvoices.Text = $"{activeInvoices} đang active";

            // Load daily revenue
            var dailyRevenue = _dashboardService.GetDailyRevenueThisMonth();
            dgDailyRevenue.ItemsSource = dailyRevenue;

            // Load top products
            var topProducts = _dashboardService.GetTopProductsToday(5);
            dgTopProducts.ItemsSource = topProducts;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải dữ liệu dashboard: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

