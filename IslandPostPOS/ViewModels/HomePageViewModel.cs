using CommunityToolkit.Mvvm.ComponentModel;
using IslandPostPOS.Models;
using IslandPostPOS.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IslandPostPOS.ViewModels
{
    public partial class HomePageViewModel : ObservableObject
    {
        private readonly APIService service;
        [ObservableProperty] private ObservableCollection<ChartEarningsModel>? dailyData;
        [ObservableProperty] private ObservableCollection<ChartEarningsModel>? weeklyData;
        [ObservableProperty] private ObservableCollection<ChartEarningsModel>? monthlyData;

        [ObservableProperty] private string todayTotal;
        [ObservableProperty] private string weeklyTotal;
        [ObservableProperty] private string monthlyTotal;

        public HomePageViewModel(APIService service)
        {
            this.service = service;
            _ = Init();
        }

        private async Task Init()
        {
            try
            {
                //var data = await service.LoadLast7DaysTotalsAsync();
                // In your page or another ViewModel method
                //Data = new ObservableCollection<ChartEarningsModel>(data);


                var report = await service.LoadSalesSummaryAsync();


                DailyData = new ObservableCollection<ChartEarningsModel>(
                   report.DailyTotals.Select(r => new ChartEarningsModel { Amount = (double)r.Total })
                   );

                WeeklyData = new ObservableCollection<ChartEarningsModel>(
                            report.WeeklyDailyTotals.Select(r => new ChartEarningsModel
                            {
                                Date = r.DateValue,                       // DateTime for chart axis
                                Amount = (double)(r.Total ?? 0)
                            })
);


                MonthlyData = new ObservableCollection<ChartEarningsModel>(
                            report.MonthlyTotals.Select(r => new ChartEarningsModel
                            {
                                Date = r.DateValue,                // DateTime for axis
                                Amount = (double)(r.Total ?? 0)
                            })
);


                TodayTotal = report.TodayTotal.ToString("C", CultureInfo.GetCultureInfo("en-US"));
                WeeklyTotal = report.WeeklyTotal.ToString("C", CultureInfo.GetCultureInfo("en-US"));
                MonthlyTotal = report.MonthlyTotal.ToString("C", CultureInfo.GetCultureInfo("en-US"));

            }
            catch (Exception ex)
            {
                // Handle errors gracefully (logging, UI message, etc.)
                Console.WriteLine($"Error loading report: {ex.Message}");
            }

        }
    }
}