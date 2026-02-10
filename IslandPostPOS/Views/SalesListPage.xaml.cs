using IslandPostPOS.Models;
using IslandPostPOS.Services;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.Shared.Helpers;
using IslandPostPOS.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IslandPostPOS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SalesListPage : Page
    {
        private DebounceDispatcher _debouncer = new DebounceDispatcher();
        public StorePageViewModel ViewModel => DataContext as StorePageViewModel;
        public SalesListPage()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            // Cast the parameter back to your ViewModel type
            var viewModel = e.Parameter as StorePageViewModel;

            // Assign it to the DataContext or store it
            this.DataContext = viewModel;
        }

        private void PayButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PaymentPage),ViewModel);
        }

        public string FormatAmount(decimal? amount)
        {
            return amount?.ToString("C") ?? "NA";
        }

        private void ClearButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ProductSearchBox.Text = string.Empty;
        }

        private async void ProductSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = ((TextBox)sender).Text;

            await _debouncer.DebounceAsync(
                () =>
                {
                    if(string.IsNullOrWhiteSpace(query))
                    {
                        ViewModel.Service.Products.Clear();
                        return Task.CompletedTask;
                    }
                    else
                    {
                        return ViewModel.Service.SearchAndUpdateProductsAsync(query);
                    }
                },
                delayMs: 400 // adjust delay to taste
            );
        }
    }
}
