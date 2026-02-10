using IslandPostPOS.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IslandPostPOS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SalesHistoryPage : Page
    {
        public SalesHistoryViewModel ViewModel => DataContext as SalesHistoryViewModel;
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public SalesHistoryPage(SalesHistoryViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}

