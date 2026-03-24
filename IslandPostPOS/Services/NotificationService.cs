using IslandPostPOS.Views.Controls;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using static IslandPostPOS.Views.Controls.NotificationBanner;

namespace IslandPostPOS.Services
{
    public class NotificationService
    {
        private readonly Queue<(string title,string message, NotificationSeverity severity)> _queue = new();
        private NotificationBanner _banner;
        private bool _isShowing = false;

        public static NotificationService Instance { get; } = new NotificationService();

        private NotificationService() { }

        public void Initialize(NotificationBanner banner)
        {
            _banner = banner;
        }

        public void Show(string title,string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            _queue.Enqueue((title,message, severity));

            if (!_isShowing)
            {
                ShowNext();
            }
        }

        public void ShowNext()
        {
            if (_queue.Count > 0)
            {
                var (title,msg, sev) = _queue.Dequeue();
                _banner.ShowMessage(title,msg, sev);
                _isShowing = true;
            }
            else
            {
                _banner.Hide();
                _isShowing = false;
            }
        }
    }

}