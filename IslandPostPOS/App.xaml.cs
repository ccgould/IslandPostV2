using IslandPostPOS.Services;
using IslandPostPOS.Services.Contracts;
using IslandPostPOS.ViewModels;
using IslandPostPOS.Views;
using IslandPostPOS.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IslandPostPOS;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static Window MainWindow { get; private set; }
    public static IServiceProvider Services { get; private set; }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Configure DI
        var services = new ServiceCollection();

        // Register ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<SalesListPage>();
        services.AddTransient<InventoryPage>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<StorePage>();
        services.AddTransient<StorePageViewModel>();
        services.AddTransient<LoginPage>();
        services.AddTransient<LoginViewModel>();
        services.AddSingleton<ProductService>();
        services.AddTransient<SalesHistoryPage>();
        services.AddTransient<SalesHistoryViewModel>();
        services.AddTransient<ManagerCategoriesPage>();
        services.AddTransient<MangerCategoriesViewModel>();
        services.AddTransient<IDialogService>(sp =>
    new DialogService(App.MainWindow.Content.XamlRoot));

        services.AddTransient<LoadingViewModel>();
        // Register HttpClient (singleton, reusable)
        services.AddHttpClient("ApiClient", client =>
        {
        
            client.BaseAddress = new Uri("http://localhost:5146/");
            //client.BaseAddress = new Uri("https://localhost:7098/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        Services = services.BuildServiceProvider();
        
        MainWindow = new MainWindow();
        Frame rootFrame = new Frame();
        MainWindow.Content = rootFrame;
        MainWindow.Activate();


        // Navigate to LoadingPage
        var vm = Services.GetRequiredService<LoadingViewModel>();
        rootFrame.Navigate(typeof(LoadingPage), vm);

        // Run initialization
        var productService = Services.GetRequiredService<ProductService>();
        await productService.InitializeAsync((status, progress) =>
        {
            vm.StatusMessage = status;
            vm.ProgressValue = progress;
        });

        // Stop spinner
        vm.IsLoading = false;

        // Navigate to login page
        var loginViewModel = Services.GetRequiredService<LoginViewModel>();
        // Navigate and pass vm as parameter
        rootFrame.Navigate(typeof(LoginPage), loginViewModel);

    }
}
