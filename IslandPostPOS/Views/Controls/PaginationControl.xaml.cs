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

namespace IslandPostPOS.Views.Controls
{
    public sealed class PaginationControlValueChangedEventArgs
    {
        public PaginationControlValueChangedEventArgs(int oldValue, int newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public int OldValue { get; }

        public int NewValue { get; }
    }

    public sealed partial class PaginationControl : UserControl
    {
        public static readonly DependencyProperty PageCountProperty = DependencyProperty.Register(
            nameof(MinPage),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(1));

        public static readonly DependencyProperty MaxPageProperty = DependencyProperty.Register(
            nameof(MaxPage),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(1));

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(
            nameof(CurrentPage),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(1));

        public static readonly DependencyProperty IsPreviousPageButtonEnabledProperty = DependencyProperty.Register(
            nameof(IsPreviousPageButtonEnabled),
            typeof(bool),
            typeof(PaginationControl),
            new PropertyMetadata(true, (d, e) => (d as PaginationControl)?.UpdateButtons()));

        public static readonly DependencyProperty IsNextPageButtonEnabledProperty = DependencyProperty.Register(
            nameof(IsNextPageButtonEnabled),
            typeof(bool),
            typeof(PaginationControl),
            new PropertyMetadata(true, (d, e) => (d as PaginationControl)?.UpdateButtons()));

        public PaginationControl()
        {
            this.InitializeComponent();
        }

        public event TypedEventHandler<PaginationControl, PaginationControlValueChangedEventArgs>? PageChanged;

        public bool IsPreviousPageButtonEnabled
        {
            get => (bool)GetValue(IsPreviousPageButtonEnabledProperty);
            set => SetValue(IsPreviousPageButtonEnabledProperty, value);
        }

        public bool IsNextPageButtonEnabled
        {
            get => (bool)GetValue(IsNextPageButtonEnabledProperty);
            set => SetValue(IsNextPageButtonEnabledProperty, value);
        }

        public int CurrentPage
        {
            get => (int)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public int MinPage
        {
            get => (int)GetValue(PageCountProperty);
            set => SetValue(PageCountProperty, value);
        }

        public int MaxPage
        {
            get => (int)GetValue(MaxPageProperty);
            set => SetValue(MaxPageProperty, value);
        }

        private void UpdateButtons()
        {
            PreviousPageButton.IsEnabled = (CurrentPage > MinPage) && IsPreviousPageButtonEnabled;
            NextPageButton.IsEnabled = (CurrentPage < MaxPage) && IsNextPageButtonEnabled;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = Math.Max(CurrentPage - 1, MinPage);
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = Math.Min(CurrentPage + 1, MaxPage);
        }

        private void CurrentPageNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            UpdateButtons();
            PageChanged?.Invoke(
                this,
                new PaginationControlValueChangedEventArgs(
                    (int)args.OldValue,
                    (int)args.NewValue));
        }
    }
}