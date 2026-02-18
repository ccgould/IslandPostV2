using IslandPostPOS.Models;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Syncfusion.UI.Xaml.DataGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IslandPostPOS.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class InventoryPage : Page
{
    public InventoryViewModel ViewModel => DataContext as InventoryViewModel;
    public InventoryPage(InventoryViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        // Populate status combo
        statusCmb.ItemsSource = new List<string> { "Active", "Inactive" };

        // Populate category combo
        categoryCmb.ItemsSource = new List<string> { "Postcards", "Stickers", "Magnets" };

        // Populate brand combo
        brandCmb.ItemsSource = new List<string> { "IslandPost", "Gadget" };

        //_= ViewModel.ChangeDataGridPage();
        _ = ViewModel.LoadProducts();

        this.Loaded += ProductsPage_Loaded;
    }

    private void ProductsPage_Loaded(object sender, RoutedEventArgs e)
    {     

        // When VM filter inputs change, refresh the grid filter
        ViewModel.FilterChanged += () =>
        {
            // Wire the grid's filter to the VM predicate
            dataGrid.View.Filter = ViewModel.FilterPredicate;
            dataGrid.View?.RefreshFilter();
        };

        // If you prefer PropertyChanged:
        // ViewModel.PropertyChanged += (s, ev) => { if (ev.PropertyName is "SearchText" or "CategoryText" ...) SfDataGrid.View?.RefreshFilter(); };
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var filter = new ProductFilterDTO
        {
            Search = searchBar.Text.Trim(),
            Category = categoryCmb.SelectedItem as string,
            Brand = brandCmb.SelectedItem as string,
            Status = statusCmb.SelectedItem as string
        };

        ViewModel.ResetProducts(filter);
    }

    private void backBTN_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsEditing = false;
    }

    private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
    {
        searchBar.Text = string.Empty;
        categoryCmb.SelectedItem = null;
        brandCmb.SelectedItem = null;
        statusCmb.SelectedItem = null;

        ViewModel.ResetProducts();
    }
}