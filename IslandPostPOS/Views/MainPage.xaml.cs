using IslandPostPOS.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IslandPostPOS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ProductService Service { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            ContentFrame.Navigate(typeof(HomePage)); // Default page
                                                     // Get the current window

            AddMenuItems();

            var window = App.MainWindow; // assuming you stored it in App.xaml.cs
            window.ExtendsContentIntoTitleBar = true; // Extend the content into the title bar and hide the default title bar
            window.AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
            window.SetTitleBar(titleBar); // Set the custom title bar
            window.AppWindow.SetIcon("Assets/Tiles/logo.ico");
            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.Parameter is ProductService service)
            {
                Service = service;
            }
        }

        private void ManageAccount_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to account management page
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Open settings page
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            // Trigger your logout logic
        }

        private void AddMenuItems()
        {
            var productsPage = new NavigationViewItem
            {
                Content = "Products",
                Icon = new SymbolIcon(Symbol.XboxOneConsole),
                Tag = "products"
            };
            var inventoryPage = new NavigationViewItem
            {
                Content = "Inventory",
                Icon = new SymbolIcon(Symbol.XboxOneConsole),
                Tag = "inventory"
            };
            var catergoryPage = new NavigationViewItem
            {
                Content = "Category",
                Icon = new SymbolIcon(Symbol.XboxOneConsole),
                Tag = "category"
            };

            var salesHistoryPage = new NavigationViewItem
            {
                Content = "Sales History",
                Icon = new SymbolIcon(Symbol.Back),
                Tag = "salesHistory"
            };
            var salesReportPage = new NavigationViewItem
            {
                Content = "Sales Reports",
                Icon = new SymbolIcon(Symbol.ReportHacked),
                Tag = "salesReports"
            };
            storeItem.MenuItems.Add(salesHistoryPage);
            storeItem.MenuItems.Add(salesReportPage);

            inventoryItem.MenuItems.Add(inventoryPage);
            inventoryItem.MenuItems.Add(catergoryPage);

        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                if(selectedItem.Tag is not null)
                {
                    switch (((string)selectedItem.Tag).ToLower())
                    {
                        case "home":
                            ContentFrame.Navigate(typeof(HomePage));
                            break;
                        case "profile":
                            ContentFrame.Navigate(typeof(ProfilePage));
                            break;
                        case "settings":
                            ContentFrame.Navigate(typeof(SettingsPage));
                            break;
                        case "store":
                            var storePage = App.Services.GetService(typeof(StorePage));
                            ContentFrame.Content = storePage;
                            break;
                        case "inventory":
                            var inventoryPage = App.Services.GetService(typeof(InventoryPage));
                            ContentFrame.Content = inventoryPage;
                            //var navService = new NavigationService(App.Services, ContentFrame);
                            //navService.Navigate<InventoryPage>();

                            break;
                        case "saleshistory":
                            var salesHistory = App.Services.GetService(typeof(SalesHistoryPage));
                            ContentFrame.Content = salesHistory;
                            break;
                        case "category":
                            var category = App.Services.GetService(typeof(ManagerCategoriesPage));
                            ContentFrame.Content = category;
                            break;
                    }
                }
            }
        }

    }
}
