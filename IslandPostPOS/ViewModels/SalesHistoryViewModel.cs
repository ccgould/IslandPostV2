using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IslandPostPOS.Services;
using System;
using System.Threading.Tasks;

namespace IslandPostPOS.ViewModels
{
    public partial class SalesHistoryViewModel : ObservableObject
    {
        public APIService Service { get; set; }
        [ObservableProperty] private DateTimeOffset? startDate;
        [ObservableProperty] private DateTimeOffset? endDate;
        [ObservableProperty] private string? saleNumber;

        public SalesHistoryViewModel(APIService service)
        {
            Service = service;
        }

        [RelayCommand]
        private async Task Search()
        {
            string? start = StartDate?.Date.ToString("yyyy-MM-dd");
            string? end = EndDate?.Date.ToString("yyyy-MM-dd");

            await Service.SearchAndUpdateSalesHistoryAsync(
                SaleNumber,
                start,
                end
            );
        }
    }
}
