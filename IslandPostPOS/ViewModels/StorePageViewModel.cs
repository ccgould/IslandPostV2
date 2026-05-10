using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IslandPostPOS.Models;
using IslandPostPOS.Services;
using IslandPostPOS.Services.Contracts;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.Shared.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static IslandPostPOS.Views.Controls.NotificationBanner;

namespace IslandPostPOS.ViewModels;

public partial class StorePageViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ProductDTO> filteredProducts = new();
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
    private string _portName = "COM9";

    private SaleDTO? _registeredSale;
    private readonly IDialogService dialogService;

    public APIService Service { get; private set; }

    public StorePageViewModel(APIService service, IDialogService dialogService,UsbComScannerService scannerService, ReceiptService receiptService)
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

        _scanner = scannerService;
        this.receiptService = receiptService;
    }

    private async void OnScanReceived(object? sender, string data)
    {
        try
        {
            var product = await SearchProductAsync(data);
            if (product != null)
            {
                AddPurchaseItem(product);
            }
            else
            {
                NotificationService.Instance.Show("Not found", $"Product with barcode {data} not found in database", NotificationSeverity.Warning);

            }
        }
        catch (Exception ex)
        {
            NotificationService.Instance.Show("Error", ex.Message, NotificationSeverity.Error);
        }
    }

    private void OnScannerDisconnected(object? sender, EventArgs e)
    {
       // NotificationService.Instance.Show("Scanner Disconnected", "Please reconnect your barcode scanner.");
    }

    private void OnScannerReconnected(object? sender, EventArgs e)
    {
       NotificationService.Instance.Show("Scanner Connected", "Barcode scanner connected.");
    }

    [RelayCommand]
    private void AddPurchaseItem(ProductDTO product)
    {
        if (product is null) return;

        if (SaleItems.Any(x => x.IdProduct == product.IdProduct))
        {
            var saleItem = SaleItems.FirstOrDefault(x => x.IdProduct == product.IdProduct);
            if (saleItem != null) 
            {
                saleItem.Quantity += 1;
                AmountToPay += saleItem.Price ?? 0;
                NotificationService.Instance.Show("Product", "Product already in list adding 1 to count");
            }
        }
        else
        {
            var item = new PurchaseItem(product);
            item.PropertyChanged += PurchaseItem_PropertyChanged; // 👈 listen for changes
            SaleItems.Add(item);

            AmountToPay += item.Total ?? 0;
            ShowTotals();
        }

        SearchText = string.Empty;
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

        if(e.PropertyName == nameof(PurchaseItem.Quantity))
        {
            var purchaseItem = sender as PurchaseItem;
           AmountToPay += purchaseItem?.Price ?? 0;
        }
    }

    [RelayCommand]
    private void DeleteProduct(PurchaseItem p)
    {
        try
        {
            AmountToPay -= p.Total ?? 0;
            p.PropertyChanged -= PurchaseItem_PropertyChanged; // 👈 avoid memory leaks
            SaleItems.Remove(p);
            ShowTotals();
        }
        catch (Exception ex)
        {
            NotificationService.Instance.Show("Error", ex.Message, NotificationSeverity.Error);
        }
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
            IdUsers = Service.CurrentUser?.IdUsers,
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

        _registeredSale = await Service.CheckoutUnifiedAsync(saleDto);

        if (_registeredSale != null)
        {
            // Optionally show confirmation dialog or update UI
            FinalizeMessage = $"Sale #{_registeredSale.SaleNumber} registered successfully!";
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


    private CancellationTokenSource _cts;
    private UsbComScannerService _scanner;
    private readonly ReceiptService receiptService;

    [RelayCommand]
    private async Task SearchProductsAsync(string query)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            await Task.Delay(250, _cts.Token); // debounce

            IsSearching = true;

            if (string.IsNullOrWhiteSpace(query))
            {
                FilteredProducts.Clear();
                return;
            }

            // Step 1: Show cached results instantly
            var cachedResults = Service.GetCachedResults(query).ToList();
            if (cachedResults.Any())
            {
                FilteredProducts = new ObservableCollection<ProductDTO>(cachedResults);
            }

            // Step 2: Refresh from API
            var freshResults = await Service.SearchAndUpdateProductsAsync(query, _cts.Token);
            FilteredProducts = new ObservableCollection<ProductDTO>(freshResults);
        }
        catch (TaskCanceledException)
        {
            NotificationService.Instance.Show("Cancelled", "Search was cancelled", NotificationSeverity.Warning);
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task<ProductDTO> SearchProductAsync(string query)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            await Task.Delay(250, _cts.Token); // debounce

            IsSearching = true;

            if (string.IsNullOrWhiteSpace(query))
            {
                FilteredProducts.Clear();
                return null;
            }

            // Step 1: Show cached results instantly
            var cachedResults = Service.GetCachedResults(query).ToList();
            if (cachedResults.Any())
            {
                return cachedResults.FirstOrDefault();
            }
            else
            {
                var results = await Service.SearchAndUpdateProductsAsync(query, _cts.Token);
                return results.FirstOrDefault();
            }
        }
        catch (TaskCanceledException)
        {
            NotificationService.Instance.Show("Cancelled", "Search was cancelled", NotificationSeverity.Warning);
        }
        finally
        {
            IsSearching = false;
        }

        return null;
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

        // Step 2: Register the sale
        var registeredSale = await Service.CheckoutUnifiedAsync(saleDto);

        // Step 3: Park the sale (if not already parked)
        var parkedSale = await Service.ParkSaleAsync(registeredSale.IdSale);

        if (parkedSale is not null)
        {
            // Show notification
            Service.ParkedSales.Add(parkedSale);
        }
    }

    
    [RelayCommand]
    private async Task Retrieve(SaleDTO sale)
    {
        if (sale is null) return;

        // 👇 Call API to mark sale as Retrieved
        var updatedSale = await Service.RetrieveSaleAsync(sale.IdSale);
        if (updatedSale == null) return;

        // Clear current sale
        Clear();

        // Rehydrate SaleItems from the retrieved sale
        foreach (var detail in updatedSale.DetailSales)
        {
            var item = new PurchaseItem(detail);
            item.PropertyChanged += PurchaseItem_PropertyChanged;
            SaleItems.Add(item);
        }

        // Restore totals
        SubTotal = updatedSale.Subtotal ?? 0;
        TotalTax = updatedSale.TotalTaxes ?? 0;
        SaleTotal = updatedSale.Total ?? 0;
        AmountToPay = updatedSale.Total ?? 0;

        // Restore other metadata
        //PaymentMethod = updatedSale.PaymentMethod;
        //ClientName = updatedSale.ClientName;
        //Note = updatedSale.Note;
        //Status = updatedSale.Status; // 👈 keep track of lifecycle state
    }
    
    public async Task LoadParkedSalesAsync()
    {
        Service.ParkedSales.Clear();
        var parked = await Service.GetParkedSalesAsync();
        foreach (var sale in parked)
        {
            Service.ParkedSales.Add(sale);
        }
    }

    internal async Task CompleteSale()
    {
        await Service.FinalizeSaleAsync(_registeredSale.IdSale);
        Clear();
    }

    public void SubscribeScannerEvents()
    {
        _scanner.ScanReceived += OnScanReceived;
        _scanner.ScannerDisconnected += OnScannerDisconnected;
        _scanner.ScannerReconnected += OnScannerReconnected;
    }

    public void UnsubscribeScannerEvents()
    {
        _scanner.ScanReceived -= OnScanReceived;
        _scanner.ScannerDisconnected -= OnScannerDisconnected;
        _scanner.ScannerReconnected -= OnScannerReconnected;
    }

    private async Task EmailReciept(string email)
    {
        var receipt = new ReceiptData
        {
            company_name = "Island Post Paradise Island",
            company_address = "Royal Beach Club Paradise Island",
            company_email = "islandpostphotospi@gmail.com",
            receipt_no = _registeredSale.SaleNumber,
            receipt_date = _registeredSale.RegistrationDate.Value.ToString("yyyy-MM-dd hh:mm:ss tt"),
            footer = "Thank you for shopping with us!",
            tax = "10",
            logo = "https://craftmypdf-upload.s3-ap-southeast-1.amazonaws.com/3e9/b0d7c504-4ee7-4173-aa2d-12b16ec10814.png",
            currency = "$"
        };

        foreach (var item in SaleItems)
        {
            receipt.items.Add(new ReceiptItem
            {
                description = item.Description,
                qty = item.Quantity,
                unitprice = item.Price ?? 0
            });
        }

        var result = await receiptService.GenerateReceiptAsync(receipt);
    }

    [RelayCommand]
    private async Task PrintReceipt()
    {

    }
}
