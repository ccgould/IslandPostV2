using IslandPostPOS.Services;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; set; }
        public LoginPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is LoginViewModel vm)
            {
                ViewModel = vm;
                this.DataContext = vm; // if you want {Binding}
                vm.LoggedIn += OnLoggedIn;
                vm.ClearPasswordRequested += () => PasswordBox.Password = string.Empty;
            }
        }

        private void OnLoggedIn(LoginResponseDTO dTO)
        {
            var service = App.Services.GetRequiredService<ProductService>();
            Frame.Navigate(typeof(MainPage),service);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            //string username = UsernameBox.Text;
            //string password = PasswordBox.Password;

            //if (username == "admin" && password == "1234")
            //{
            //    // Navigate using the Frame inside the current Page
            //    Frame.Navigate(typeof(MainPage));
            //}
            //else
            //{
            //    ContentDialog dialog = new ContentDialog()
            //    {
            //        Title = "Login Failed",
            //        Content = "Invalid username or password.",
            //        CloseButtonText = "OK",
            //        XamlRoot = this.XamlRoot
            //    };
            //    _ = dialog.ShowAsync();
            //}
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}
