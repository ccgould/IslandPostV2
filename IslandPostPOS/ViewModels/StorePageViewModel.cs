using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IslandPostPOS.Models;
using IslandPostPOS.Services;
using IslandPostPOS.Services.Contracts;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.Shared.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserDataTasks.DataProvider;

namespace IslandPostPOS.ViewModels;

public partial class StorePageViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PurchaseItem> saleItems;
    [ObservableProperty] private ObservableCollection<Customer> customers;
    [ObservableProperty] private ObservableCollection<TransactionDTO> transactions = new();
    [ObservableProperty] private ObservableCollection<PaymentMethod> paymentMethods = new();
    [ObservableProperty] private bool isSearching;
    [ObservableProperty] private string searchText;

    private DebounceDispatcher _debouncer = new DebounceDispatcher();

    public decimal RemainingBalance => SaleTotal - Transactions.Sum(t => t.Amount);
    public bool CanCheckout => RemainingBalance == 0;


    [ObservableProperty] private decimal subTotal = 0.0m;
    [ObservableProperty] private decimal totalTax = 0.0m;
    [ObservableProperty] private decimal amountToPay = 0.0m;
    [ObservableProperty] private decimal saleTotal = 0.0m;
    [ObservableProperty] private decimal taxValue = 10m;
    [ObservableProperty] private bool isTransactionComplete = false;
    [ObservableProperty] private bool giveChange = false;
    [ObservableProperty] private string saleCountTxt = "0 items";
    [ObservableProperty] private decimal changeAmount = 0.0m;
    [ObservableProperty] private string finalizeMessage = "Payment Recieved";
    private readonly IDialogService dialogService;

    public APIService Service { get; private set; }

    public StorePageViewModel(APIService service, IDialogService dialogService)
    {
        saleItems = new();
        Service = service;
        this.dialogService = dialogService;
        var options = new[] { "CASH","VISA", "MASTER","AMEX","DISCOVER", "OTHER" };

        for (int i = 0; i < options.Length; i++)
        {
            string? paymentMethod = options[i];
            paymentMethods.Add(new PaymentMethod { IdPaymentMethod = i, Name = paymentMethod});
        }
    }

    [RelayCommand]
    private void AddPurchaseItem(ProductDTO product)
    {
        if (product is null) return;

        if (SaleItems.Any(x => x.IdProduct == product.IdProduct))
        {
            return;
        }

        var item = new PurchaseItem(product);
        item.PropertyChanged += PurchaseItem_PropertyChanged; // 👈 listen for changes
        SaleItems.Add(item);

        AmountToPay += item.Total ?? 0;
        ShowTotals();
    }

    private void SaleCount()
    {
        var totalItems = SaleItems.Sum(x => x.Quantity);
        SaleCountTxt =  $"{totalItems} items";
    }

    private void PurchaseItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PurchaseItem.Quantity) ||
            e.PropertyName == nameof(PurchaseItem.Price) ||
            e.PropertyName == nameof(PurchaseItem.DiscountPercent) ||
            e.PropertyName == nameof(PurchaseItem.Total))
        {
            ShowTotals();
        }
    }

    [RelayCommand]
    private void DeleteProduct(PurchaseItem p)
    {
        AmountToPay -= p.Total ?? 0;
        p.PropertyChanged -= PurchaseItem_PropertyChanged; // 👈 avoid memory leaks
        SaleItems.Remove(p);
        ShowTotals();
    }

    private void ShowTotals()
    {
        decimal total = SaleItems.Sum(item => item.Total ?? 0);
        decimal subtotal = total / (1 + (TaxValue / 100));
        decimal tax = total - subtotal;

        SubTotal = subtotal;
        TotalTax = tax;
        SaleTotal = total;

        SaleCount();
    }

    [RelayCommand]
    private async void PaymentOption(PaymentMethod method)
    {
        if (method is null || AmountToPay <= 0) return;

        decimal tenderedAmount = AmountToPay;

        if (method.Name.Equals("Cash", StringComparison.OrdinalIgnoreCase))
        {
            var enteredAmount = await dialogService.ShowCashDialogAsync();
            if (enteredAmount is null) return; // cancelled
            tenderedAmount = enteredAmount.Value;
        }


        var paidTotal = Transactions.Sum(t => t.Amount);
        decimal appliedAmount = tenderedAmount;
        decimal change = 0;

        if (paidTotal + tenderedAmount > SaleTotal)
        {
            if (method.Name.Equals("Cash", StringComparison.OrdinalIgnoreCase))
            {
                appliedAmount = RemainingBalance;
                change = (paidTotal + tenderedAmount) - SaleTotal;
            }
            else
            {
                appliedAmount = RemainingBalance;
            }
        }

        Transactions.Add(new TransactionDTO
        {
            IdPaymentMethod = method.IdPaymentMethod,
            Amount = appliedAmount,
            RegisteredDate = DateTime.UtcNow,
            PaymentMethodName = method.Name
        });

        UpdatePaymentState();

        if (CanCheckout)
        {
            await CheckOut();
            IsTransactionComplete = true;
        }

        // Reset AmountToPay
        AmountToPay = method.Name.Equals("Cash", StringComparison.OrdinalIgnoreCase) ? 0 : RemainingBalance;

        if(change > 0m)
        {
            ChangeAmount = change; // property to show change due
            GiveChange = true;
            FinalizeMessage = $"Give ${ChangeAmount} Change";
        }
        else
        {
            FinalizeMessage = "Payment Recieved";
        }
    }

    private async Task CheckOut()
    {
        var saleDto = new SaleDTO
        {
            Subtotal = SubTotal,
            TotalTaxes = TotalTax,
            Total = SaleTotal,
            RegistrationDate = DateTime.UtcNow,
            PaymentMethod = string.Join(", ", Transactions.Select(t => t.PaymentMethodName)),
            DetailSales = SaleItems.Select(item => new DetailSaleDTO
            {
                IdProduct = item.IdProduct,
                Quantity = item.Quantity,
                Price = item.Price,
                Total = item.Total
            }).ToList()
        };

        var registeredSale = await Service.CheckOutAsync(saleDto);

        if (registeredSale != null)
        {
            // Optionally show confirmation dialog or update UI
            FinalizeMessage = $"Sale #{registeredSale.SaleNumber} registered successfully!";
        }
    }

    private void UpdatePaymentState()
    {
        OnPropertyChanged(nameof(RemainingBalance));
        OnPropertyChanged(nameof(CanCheckout));
    }

    public void Clear()
    {
        SubTotal = decimal.Zero;
        TotalTax = decimal.Zero;
        SaleTotal = decimal.Zero;
        IsTransactionComplete = false;
        Transactions.Clear();
        giveChange = false;
        for (int i = SaleItems.Count - 1; i >= 0; i--)
        {
            DeleteProduct(SaleItems[i]);
        }
        AmountToPay = 0.0m;

        UpdatePaymentState();
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = SearchProductsAsync(value);
    }


    [RelayCommand]
    private async Task SearchProductsAsync(string query)
    {
        await _debouncer.DebounceAsync(
            async () =>
            {
                IsSearching = true;

                if (string.IsNullOrWhiteSpace(query))
                {
                    Service.FilteredProducts.Clear();
                }
                else
                {
                    await Service.SearchAndUpdateProductsAsync(query);
                }

                IsSearching = false;
            },
            delayMs: 400
        );
    }

    internal async Task ParkSaleAsync(string value)
    {
        var saleDto = new SaleDTO
        {
            Subtotal = SubTotal,
            TotalTaxes = TotalTax,
            Total = SaleTotal,
            IdUsers = Service.CurrentUser?.IdUsers,
            Note = value,
            RegistrationDate = DateTime.UtcNow,
            PaymentMethod = string.Join(", ", Transactions.Select(t => t.PaymentMethodName)),
            DetailSales = SaleItems.Select(item => new DetailSaleDTO
            {
                IdProduct = item.IdProduct,
                Quantity = item.Quantity,
                Price = item.Price,
                Total = item.Total
            }).ToList()
        };

        Clear();

        var parkedSale = await Service.ParkSaleAsync(saleDto);

        if (parkedSale is not null)
        {
            // Show notification
            Service.ParkedSales.Add(parkedSale);
        }
    }
}
