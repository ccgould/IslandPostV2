using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IslandPostPOS.Views.Controls
{
    public sealed partial class NotificationBanner : UserControl
    {
        public NotificationBanner()
        {
            InitializeComponent();
        }

        public async void ShowMessage(string title,string message, NotificationSeverity severity, int durationMs = 3000)
        {
            try
            {
                TitleText.Text = title;
                MessageText.Text = message;
                Root.Background = GetBackgroundBrush(severity);
                Root.Visibility = Visibility.Visible;

                // Auto-dismiss after duration
                await Task.Delay(durationMs);
                Hide();
                Services.NotificationService.Instance.ShowNext(); // show next queued message
            }
            catch (Exception ex)
            {
            }

        }

        public void Hide()
        {
            Root.Visibility = Visibility.Collapsed;
        }

        private void Dismiss_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            Services.NotificationService.Instance.ShowNext();
        }

        private Brush GetBackgroundBrush(NotificationSeverity severity)
        {
            return severity switch
            {
                NotificationSeverity.Info => new SolidColorBrush(Colors.LightBlue),
                NotificationSeverity.Warning => new SolidColorBrush(Colors.Gold),
                NotificationSeverity.Error => new SolidColorBrush(Colors.IndianRed),
                NotificationSeverity.Success => new SolidColorBrush(Colors.LightGreen),
                _ => new SolidColorBrush(Colors.LightGray)
            };
        }


        public enum NotificationSeverity
        {
            Info,
            Warning,
            Error,
            Success
        }



    }
}
