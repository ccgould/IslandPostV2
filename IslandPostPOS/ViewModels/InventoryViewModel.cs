using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IslandPostPOS.Enumerators;
using IslandPostPOS.Models;
using IslandPostPOS.Services;
using IslandPostPOS.Shared.DTOs;
using Microsoft.UI.Xaml.Media.Imaging;
using Syncfusion.UI.Xaml.DataGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace IslandPostPOS.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    [ObservableProperty] private APIService productService;
    [ObservableProperty] private ProductDTO product;
    [ObservableProperty] private BitmapImage? imageSource;

    [ObservableProperty] private string? imageFileName;
    [ObservableProperty] private CategoryDTO? category;
    [ObservableProperty] private bool isEditing;
    
    private ProductFilterDTO _filter = new ProductFilterDTO();
    [ObservableProperty] private bool isLoading;

    public List<OptionItem> ActiveOptions { get; } = new()
    {
        new OptionItem { Display = "Active", Value = 1 },
        new OptionItem { Display = "Not Active", Value = 0 },
        new OptionItem { Display = "Unknown", Value = null }
    };

    public List<OptionItem> DiscountOptions { get; } = new()
    {
        new OptionItem { Display = "Yes", Value = 1 },
        new OptionItem { Display = "No", Value = 0 },
        new OptionItem { Display = "Unknown", Value = null }
    };




    // Full product list

    // Bound filter properties
    [ObservableProperty]
    private string searchText;

    [ObservableProperty]
    private string categoryText;

    [ObservableProperty]
    private string brandText;

    // Expose all enum values for binding
    public Array StatusOptions => Enum.GetValues(typeof(ProductStatus));


    [ObservableProperty] private ProductStatus selectedStatus = ProductStatus.All;


    // Command to refresh filter (optional if you want a Search button)
    [RelayCommand]
    private void ApplyFilters()
    {
        // Notify the view to refresh the DataGrid filter
        FilterChanged?.Invoke();
    }

    // Predicate used by SfDataGrid.View.Filter
    public bool FilterPredicate(object record)
    {
        if (record is not ProductDTO p) return false;

        // Search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            if (!(p.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true
                  || p.BarCode?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true))
                return false;
        }

        // Category
        if (!string.IsNullOrWhiteSpace(CategoryText) &&
            !string.Equals(p.NameCategory, CategoryText, StringComparison.OrdinalIgnoreCase))
            return false;

        // Brand
        if (!string.IsNullOrWhiteSpace(BrandText) &&
            !string.Equals(p.Brand, BrandText, StringComparison.OrdinalIgnoreCase))
            return false;

        // Status filter
        if (SelectedStatus == ProductStatus.Active && p.IsActive != 1)
            return false;

        if (SelectedStatus == ProductStatus.Inactive && p.IsActive != 0)
            return false;

        // If All, no filter applied


        return true;
    }

    // Reset method
    [RelayCommand]
    public void ResetFilters()
    {
        SearchText = string.Empty;
        CategoryText = string.Empty;
        BrandText = string.Empty;
        SelectedStatus = ProductStatus.All; // reset to default
        
        // Notify the view to refresh the filter
        FilterChanged?.Invoke();
    }


    // Event to notify the view when filters change
    public event Action FilterChanged;

    // Auto-trigger filter refresh when properties change
    partial void OnSearchTextChanged(string value) => FilterChanged?.Invoke();
    partial void OnCategoryTextChanged(string value) => FilterChanged?.Invoke();
    partial void OnBrandTextChanged(string value) => FilterChanged?.Invoke();
    partial void OnCategoryChanged(CategoryDTO? value) => FilterChanged?.Invoke();

    partial void OnSelectedStatusChanged(ProductStatus value) => FilterChanged?.Invoke();


    public InventoryViewModel(APIService productService)
    {
        this.productService = productService;
        // Initialize with first page
        ResetProducts();
        product = new();
    }

    public async void ResetProducts(ProductFilterDTO? filter = null)
    {
        
    }


    private async Task<IList<ProductDTO>> LoadMoreItems(CancellationToken token, uint count, int baseIndex)
    {
        int pageNumber = (baseIndex / (int)count) + 1;

        var result = await ProductService.GetProductsPagedAsync(pageNumber, (int)count, _filter, token);

        return result?.Items ?? new List<ProductDTO>();
    }


    internal async Task ChangeDataGridPage(int pageNumber = 1, int pageSize = 10)
    {
       var paged = await ProductService.GetProductsPagedAsync(pageNumber, pageSize);
        
        if (paged != null)
        {
            // UI now has pagedProducts bound
            Debug.WriteLine($"Got {paged.Items.Count} products out of {paged.TotalCount}");
        }
    }

    [RelayCommand]
    private async Task SelectThumbnailAsync()
    {
        var picker = new FileOpenPicker();
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        StorageFile file = await picker.PickSingleFileAsync();
        if (file == null) return;

        ImageFileName = file.Name;

        using var stream = await file.OpenAsync(FileAccessMode.Read);
        var decoder = await BitmapDecoder.CreateAsync(stream);

        // Target thumbnail size
        uint thumbWidth = 150;
        uint thumbHeight = 150;

        // Create encoder for JPEG thumbnail
        using var thumbStream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateForTranscodingAsync(thumbStream, decoder);

        encoder.BitmapTransform.ScaledWidth = thumbWidth;
        encoder.BitmapTransform.ScaledHeight = thumbHeight;
        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;

        await encoder.FlushAsync();

        // Convert to byte[] and Base64
        using var ms = new MemoryStream();
        await thumbStream.AsStream().CopyToAsync(ms);
        byte[] thumbBytes = ms.ToArray();

        Product.Photo = thumbBytes;
        //Product.Base64Photo = Convert.ToBase64String(thumbBytes);

        // Display thumbnail
        var bitmap = new BitmapImage();
        thumbStream.Seek(0);
        await bitmap.SetSourceAsync(thumbStream);
        ImageSource = bitmap;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (Product is not null)
        {
            if (string.IsNullOrWhiteSpace(Product.Description))
            {
                //Send error message
                return;
            }
            if (string.IsNullOrWhiteSpace(Product.BarCode))
            {
                //Send error message
                return;
            }

            if (Product.IdProduct > 0)
            {
                await ProductService.UpdateProductAsync(Product);
            }
            else
            {
                   await ProductService.AddProductAsync(Product);
            }
            IsEditing = false;
        }
    }

    [RelayCommand]
    private async Task Delete(ProductDTO product)
    {
        if(product is not null)
        {
            await ProductService.DeleteProductAsync(product.IdProduct);
        }
    }

    [RelayCommand]
    private async Task Edit(ProductDTO product)
    {
        if(product is not null)
        {
            Product = product;
        }
        IsEditing = true;
    }

    [RelayCommand]
    private async Task NewPeoduct()
    {
        Product = new();
        IsEditing = true;
    }

    [RelayCommand]
    private async Task Back()
    {
        IsEditing = false;
    }
     
    internal async Task LoadProducts()
    {
        IsLoading = true;
        await ProductService.LoadAllProductsAsync();
        IsLoading = false;
    }
}
public class OptionItem
{
    public string Display { get; set; } = string.Empty;
    public int? Value { get; set; }
}
