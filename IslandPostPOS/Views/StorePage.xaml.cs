using IslandPostPOS.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.Globalization.NumberFormatting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IslandPostPOS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StorePage : Page
    {
        private StorePageViewModel ViewModel => DataContext as StorePageViewModel; 
        public StorePage(StorePageViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            ContentFrame.Navigate(typeof(SalesListPage),ViewModel);

        }

        private void NumberBox_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if(sender is NumberBox nb)
            {
                IncrementNumberRounder rounder = new IncrementNumberRounder();
                rounder.Increment = 0.25;
                rounder.RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp;

                DecimalFormatter formatter = new DecimalFormatter();
                formatter.IntegerDigits = 1;
                formatter.FractionDigits = 2;
                formatter.NumberRounder = rounder;
                nb.NumberFormatter = formatter;
            }
        }
    }
}
