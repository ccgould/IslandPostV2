using IslandPostPOS.Models;
using IslandPostPOS.Services;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.Shared.Helpers;
using IslandPostPOS.ViewModels;
using IslandPostPOS.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

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
            this.Unloaded += SalesListPage_Unloaded;
            this.Loaded += SalesListPage_Loaded;

        }

        private void SalesListPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.SubscribeScannerEvents();
        }

        private void SalesListPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.UnsubscribeScannerEvents();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            // Cast the parameter back to your ViewModel type
            var viewModel = e.Parameter as StorePageViewModel;

            // Assign it to the DataContext or store it
            this.DataContext = viewModel;

            await viewModel.LoadParkedSalesAsync();
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

        private async void parkedSaleBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var inputPage = new ParkSaleDialog();

            var dialog = new ContentDialog
            {
                Title = "Park this sale with a note.",
                Content = inputPage,
                PrimaryButtonText = "Park Sale",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot // important in WinUI 3
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.ParkSaleAsync(inputPage.Value);
            }

        }

        private void RetrieveSale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SaleDTO sale)
            {
                ViewModel.RetrieveCommand.Execute(sale);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProductSearchBox.Focus(FocusState.Programmatic);
        }
    }
}
