using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    [ObservableProperty] private ObservableCollection<ProductDTO> pagedProducts;
    [ObservableProperty] private ProductService productService;
    [ObservableProperty] private ProductDTO product;
    [ObservableProperty] private BitmapImage? imageSource;

    [ObservableProperty] private string? imageFileName;
    [ObservableProperty] private CategoryDTO? category;
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string? searchText;
    [ObservableProperty] private string? categoryText;
    [ObservableProperty] private string? brandText;
    [ObservableProperty] private string? statusText;
    private ProductFilterDTO _filter = new ProductFilterDTO();


    [ObservableProperty] private IncrementalList<ProductDTO> incrementalItemsSource;

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


    public InventoryViewModel(ProductService productService)
    {
        this.productService = productService;
        // Initialize with first page
        ResetProducts();
        product = new();
    }

    public async void ResetProducts(ProductFilterDTO? filter = null)
    {
        _filter = filter ?? new ProductFilterDTO();

        // Fetch first page to know total count
        var firstPage = await ProductService.GetProductsPagedAsync(1, 20, _filter, CancellationToken.None);

        IncrementalItemsSource = new IncrementalList<ProductDTO>(LoadMoreItems)
        {
            MaxItemCount = firstPage?.TotalCount ?? 0
        };

        // Optionally preload the first page so the grid shows something immediately
        if (firstPage?.Items != null)
        {
            foreach (var item in firstPage.Items)
                IncrementalItemsSource.Add(item);
        }
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

    public void Dispose()
    {
        if (IncrementalItemsSource != null)
            IncrementalItemsSource.Clear();
    }
}
public class OptionItem
{
    public string Display { get; set; } = string.Empty;
    public int? Value { get; set; }
}
