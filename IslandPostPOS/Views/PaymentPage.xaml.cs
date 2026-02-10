using IslandPostPOS.Models;
using IslandPostPOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IslandPostPOS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PaymentPage : Page
    {
        public StorePageViewModel ViewModel => DataContext as StorePageViewModel;

        public PaymentPage()
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

        private void backBTN_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        public string FormatAmount(decimal? amount)
        {
            return amount?.ToString("C") ?? "NA";
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Clear();
            Frame.Navigate(typeof(SalesListPage), ViewModel);
        }
    }
}
