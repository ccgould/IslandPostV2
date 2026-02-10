namespace IslandPostPOS.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using IslandPostPOS.Shared.DTOs;

public partial class PurchaseItem : ObservableObject
{
    [ObservableProperty] private int idProduct;
    [ObservableProperty] private string? barCode;
    [ObservableProperty] private string? brand;
    [ObservableProperty] private string? description;
    [ObservableProperty] private int? idCategory;
    [ObservableProperty] private int quantity = 1;
    [ObservableProperty] private decimal? price;
    [ObservableProperty] private decimal? total;
    [ObservableProperty] private double discountPercent; // 👈 percentage (0–100)
    [ObservableProperty] private string? note;
    [ObservableProperty] private bool? isDiscount;

    public PurchaseItem(ProductDTO product, int quantity = 1)
    {
        IdProduct = product.IdProduct;
        BarCode = product.BarCode;
        Brand = product.Brand;
        Description = product.Description;
        IdCategory = product.IdCategory;
        Quantity = quantity;
        Price = product.Price;
        IsDiscount = product.IsDiscount == 1;
        DiscountPercent = 0; // default no discount
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        var baseTotal = (Price ?? 0) * Quantity;
        var discountAmount = baseTotal * ((decimal)DiscountPercent / 100m); // 👈 apply percentage
        Total = baseTotal - discountAmount;
    }

    partial void OnQuantityChanged(int value) => RecalculateTotal();
    partial void OnPriceChanged(decimal? value) => RecalculateTotal();
    partial void OnDiscountPercentChanged(double value)
    {
        if(value is double.NaN)
        {
            DiscountPercent = 0;
        }
        RecalculateTotal();
    }

    public string FormatAmount(decimal? amount) => amount?.ToString("C") ?? "NA";
}