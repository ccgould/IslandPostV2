using CommunityToolkit.Mvvm.ComponentModel;
using IslandPostPOS.Helpers;
using IslandPostPOS.Models;
using IslandPostPOS.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;


namespace IslandPostPOS.Services;

public partial class APIService : DataLoaderService
{
    private readonly Dictionary<int, ProductDTO> _productCache = new();
    [ObservableProperty] private ObservableCollection<ProductDTO> products;
    [ObservableProperty] private ObservableCollection<CategoryDTO> categories;
    [ObservableProperty] private ObservableCollection<SaleDTO> salesHistory = new();
    [ObservableProperty] private ObservableCollection<SaleDTO> parkedSales = new();
    [ObservableProperty] private CurrentUserInfo? currentUser;

    public CustomFiltering SqlFilterBehavior { get; }
    public bool IsTestMode { get; private set; } = false;

    public APIService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
        products = new();
        categories = new();
        SqlFilterBehavior = new CustomFiltering(this);

        // Auto-restore token if persisted
        var localSettings = ApplicationData.Current.LocalSettings;
        if (localSettings.Values.ContainsKey("JwtToken"))
        {
            string savedToken = localSettings.Values["JwtToken"].ToString();
            var client = GetClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", savedToken);

            DecodeAndSetCurrentUser(savedToken);
        }
    }

    public async Task InitializeAsync(Action<string, double>? reportProgress = null, CancellationToken cancellationToken = default)
    {
        var loaders = new ILoaderDescriptor[]
        {
            //new LoaderDescriptor<ProductDTO>("api/Product/GetAllProducts", "Products", c => Products = c),
            new LoaderDescriptor<CategoryDTO>("api/Category/GetAllCategories", "Categories", c => Categories = c)
        };

        await base.InitializeAsync(loaders, reportProgress, cancellationToken);
    }

    public async Task<List<ProductDTO>> SearchForProductsAsync(string search, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await GetClient().GetAsync($"api/Product/GetProducts?search={search}", cancellationToken);
            var products = await response.Content.ReadFromJsonAsync<List<ProductDTO>>(cancellationToken: cancellationToken);
            return products ?? new List<ProductDTO>();
        }
        catch (Exception ex)
        {

        }
        return new List<ProductDTO>();
    }

    public async Task<List<ProductDTO>> SearchAndUpdateProductsAsync(
            string search,
            CancellationToken cancellationToken = default)
    {
        var results = await SearchForProductsAsync(search, cancellationToken);

        // Update cache
        foreach (var product in results)
        {
            _productCache[product.IdProduct] = product;
        }

        return results;
    }

    public IEnumerable<ProductDTO> GetCachedResults(string query)
    {
        return _productCache.Values.Where(p =>
            (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(p.Brand) && p.Brand.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(p.BarCode) && p.BarCode.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(p.NameCategory) && p.NameCategory.Contains(query, StringComparison.OrdinalIgnoreCase))
        );
    }


    public HttpClient GetClient() => (HttpClient)this.GetType()
        .BaseType?
        .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.GetValue(this);

    public async Task<ProductDTO?> AddProductAsync(ProductDTO newProduct, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        // Post the new product to the API
        var response = await client.PostAsJsonAsync("api/Product/AddProduct", newProduct, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Read back the created product (with ID, etc.)
        var createdProduct = await response.Content.ReadFromJsonAsync<ProductDTO>(cancellationToken: cancellationToken);

        if (createdProduct != null)
        {
            // Update local collection so UI stays in sync
            Products.Add(createdProduct);
        }

        return createdProduct;
    }

    public async Task<ProductDTO?> UpdateProductAsync(ProductDTO updatedProduct, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        // Call the EditProduct endpoint (no {id} in the URL)
        var response = await client.PutAsJsonAsync("api/Product/EditProduct", updatedProduct, cancellationToken);
        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<ProductDTO>(cancellationToken: cancellationToken);

        if (product != null)
        {
            // Replace the product in the local collection
            var existing = Products.FirstOrDefault(p => p.IdProduct == product.IdProduct);
            if (existing != null)
            {
                var index = Products.IndexOf(existing);
                Products[index] = product;
            }
        }

        return product;
    }

    public async Task<bool> DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.DeleteAsync($"api/Product/DeleteProduct/{productId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return false;

        // Remove locally if delete succeeded
        var existing = Products.FirstOrDefault(p => p.IdProduct == productId);
        if (existing != null)
        {
            Products.Remove(existing);
        }

        return true;
    }

    public async Task<SaleDTO?> CheckOutAsync(SaleDTO sale, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.PostAsJsonAsync("api/Sales/RegisterSale", sale, cancellationToken);
        response.EnsureSuccessStatusCode();

        var registeredSale = await response.Content.ReadFromJsonAsync<SaleDTO>(cancellationToken: cancellationToken);
        return registeredSale;
    }

    public async Task<LoginResponseDTO?> LoginAsync(UserLoginDTO loginDto, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.PostAsJsonAsync("api/Access/login", loginDto, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDTO>(
            cancellationToken: cancellationToken);

        if (loginResponse != null)
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var localSettings = ApplicationData.Current.LocalSettings;
            if (loginDto.KeepLoggedIn)
                localSettings.Values["JwtToken"] = loginResponse.Token;
            else
                localSettings.Values.Remove("JwtToken");

            DecodeAndSetCurrentUser(loginResponse.Token);
        }

        return loginResponse;
    }

    public async Task<List<SaleDTO>> SearchForSalesHistoryAsync(
    string? saleNumber = null,
    string? startDate = null,
    string? endDate = null,
    CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        // Build query string
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(saleNumber))
            queryParams.Add($"saleNumber={saleNumber}");
        if (!string.IsNullOrEmpty(startDate))
            queryParams.Add($"startDate={startDate}");
        if (!string.IsNullOrEmpty(endDate))
            queryParams.Add($"endDate={endDate}");

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;

        var response = await client.GetAsync($"api/Sales/History{queryString}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var history = await response.Content.ReadFromJsonAsync<List<SaleDTO>>(cancellationToken: cancellationToken);
        return history ?? new List<SaleDTO>();
    }

    public async Task SearchAndUpdateSalesHistoryAsync(
        string? saleNumber = null,
        string? startDate = null,
        string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var results = await SearchForSalesHistoryAsync(saleNumber, startDate, endDate, cancellationToken);
        SalesHistory = new ObservableCollection<SaleDTO>(results);
    }

    public void Logout()
    {
        var client = GetClient();
        client.DefaultRequestHeaders.Authorization = null;

        var localSettings = ApplicationData.Current.LocalSettings;
        if (localSettings.Values.ContainsKey("JwtToken"))
            localSettings.Values.Remove("JwtToken");

        CurrentUser = null;
    }

    private void DecodeAndSetCurrentUser(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var id = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        CurrentUser = new CurrentUserInfo
        {
            IdUsers = int.TryParse(id, out var parsedId) ? parsedId : 0,
            Role = role ?? "",
            Email = email ?? "",
            Name = name ?? ""
        };
    }
    public async Task<PagedResult<ProductDTO>?> GetProductsPagedAsync(
    int pageNumber = 1,
    int pageSize = 10,
    ProductFilterDTO? filter = null,
    CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.PostAsJsonAsync(
            $"api/Product/GetProductsPaged?pageNumber={pageNumber}&pageSize={pageSize}",
            filter ?? new ProductFilterDTO(),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PagedResult<ProductDTO>>(cancellationToken: cancellationToken);
    }

    public async Task<CategoryDTO?> AddCategoryAsync(CategoryDTO newCategory, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.PostAsJsonAsync("api/Category/CreateCategory", newCategory, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        var createdCategory = await response.Content.ReadFromJsonAsync<CategoryDTO>(cancellationToken: cancellationToken);

        if (createdCategory != null)
        {
            Categories.Add(createdCategory);
        }

        return createdCategory;
    }

    public async Task<CategoryDTO?> UpdateCategoryAsync(CategoryDTO updatedCategory, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.PutAsJsonAsync($"api/Category/{updatedCategory.IdCategory}", updatedCategory, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        var category = await response.Content.ReadFromJsonAsync<CategoryDTO>(cancellationToken: cancellationToken);

        if (category != null)
        {
            var existing = Categories.FirstOrDefault(c => c.IdCategory == category.IdCategory);
            if (existing != null)
            {
                var index = Categories.IndexOf(existing);
                Categories[index] = category;
            }
        }

        return category;
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.DeleteAsync($"api/Category/{categoryId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return false;

        var existing = Categories.FirstOrDefault(c => c.IdCategory == categoryId);
        if (existing != null)
        {
            Categories.Remove(existing);
        }

        return true;
    }

    public async Task<CategoryDTO?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.GetAsync($"api/Category/{categoryId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<CategoryDTO>(cancellationToken: cancellationToken);
    }

    public async Task<List<CategoryDTO>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.GetAsync("api/Category/GetAllCategories", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new List<CategoryDTO>();

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDTO>>(cancellationToken: cancellationToken);
        Categories = new ObservableCollection<CategoryDTO>(categories ?? new List<CategoryDTO>());
        return categories ?? new List<CategoryDTO>();
    }

    public async Task<List<ProductDTO>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        var response = await client.GetAsync("api/Product/GetAllProducts", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new List<ProductDTO>();

        var products = await response.Content.ReadFromJsonAsync<List<ProductDTO>>(cancellationToken: cancellationToken);

        // Update local collection so UI stays in sync
        Products = new ObservableCollection<ProductDTO>(products ?? new List<ProductDTO>());

        return products ?? new List<ProductDTO>();
    }

    public async Task LoadAllProductsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Products.Clear();
            var items = await GetAllProductsAsync(cancellationToken);
            Products = new ObservableCollection<ProductDTO>(items);
        }
        catch (Exception ex)
        {
            // Log or handle error gracefully
            throw;
        }
    }

    public async Task<SaleDTO?> GetSaleAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = GetClient();
        var response = await client.GetAsync($"api/Sales/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<SaleDTO>(cancellationToken: cancellationToken);
    }

    public async Task<SaleDTO?> ParkSaleAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var client = GetClient();
        var response = await client.PostAsync($"api/Sales/{saleId}/park", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SaleDTO>(cancellationToken: cancellationToken);
    }

    public async Task<SaleDTO?> RetrieveSaleAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var client = GetClient();
        var response = await client.PostAsync($"api/Sales/{saleId}/retrieve", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SaleDTO>(cancellationToken: cancellationToken);
    }

    public async Task<SaleDTO?> FinalizeSaleAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var client = GetClient();
        var response = await client.PostAsync($"api/Sales/{saleId}/finalize", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SaleDTO>(cancellationToken: cancellationToken);
    }

    public async Task<SaleDTO?> CancelSaleAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var client = GetClient();
        var response = await client.PostAsync($"api/Sales/{saleId}/cancel", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SaleDTO>(cancellationToken: cancellationToken);
    }

    public async Task<List<SaleDTO>> GetParkedSalesAsync(CancellationToken cancellationToken = default)
    {
        var client = GetClient();
        var response = await client.GetAsync("api/Sales/parked", cancellationToken);
        response.EnsureSuccessStatusCode();

        var sales = await response.Content.ReadFromJsonAsync<List<SaleDTO>>(cancellationToken: cancellationToken);
        return sales ?? new List<SaleDTO>();
    }
    public async Task<List<ChartEarningsModel>> LoadLast7DaysTotalsAsync()
    {
        var startDate = DateTime.Today.AddDays(-6).ToString("yyyy-MM-dd");
        var endDate = DateTime.Today.ToString("yyyy-MM-dd");

        // ✅ Correct API call
        var url = $"api/sales/dailytotals?startDate={startDate}&endDate={endDate}";
        
        var response = await GetClient().GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var report = JsonSerializer.Deserialize<List<SaleReportDTO>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Map SaleReportDTO → ChartEarningsModel
        var chartData = report?.Select(r => new ChartEarningsModel
        {
            Date = DateTime.Parse(r.RegistrationDate),   // parse string → DateTime
            Amount = (double)r.Total   // cast decimal → double
        }).ToList() ?? new List<ChartEarningsModel>();

       return chartData;
    }

    public async Task<SalesSummaryDTO> LoadSalesSummaryAsync()
    {

        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;

        var url = $"api/sales/salessummary?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        var response = await GetClient().GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var report = JsonSerializer.Deserialize<SalesSummaryDTO>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return report;
    }

    public async Task<SaleDTO?> CheckoutUnifiedAsync(SaleDTO sale, CancellationToken cancellationToken = default)
    {
        if (IsTestMode)
            return await CheckOutTestAsync(sale, cancellationToken);
        else
            return await CheckOutAsync(sale, cancellationToken);
    }

    public Task<SaleDTO?> CheckOutTestAsync(SaleDTO sale, CancellationToken cancellationToken = default)
    {
        // Simulate latency
        return Task.FromResult(new SaleDTO
        {
            IdSale = -1, // marker for test
            SaleNumber = $"TEST-{Guid.NewGuid().ToString("N")[..6]}",
            IdUsers = sale.IdUsers,
            Users = sale.Users,
            ClientName = sale.ClientName ?? "Test Client",
            CustomerDocument = sale.CustomerDocument,
            Subtotal = sale.Subtotal,
            TotalTaxes = sale.TotalTaxes,
            Total = sale.Total,
            RegistrationDate = DateTime.Now,
            PaymentMethod = sale.PaymentMethod ?? "TEST",
            DetailSales = sale.DetailSales,
            Status = IslandPostPOS.Shared.Enumerators.SaleStatus.Parked, // or a custom Test status
            Note = "TEST MODE: Not submitted"
        });
    }

    public async Task<EndOfShiftReportDTO> GetReportPdfAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        // Format query string in ISO 8601
        string url = $"api/report/reportpdf?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}";

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Deserialize JSON into your DTO
        return await response.Content.ReadFromJsonAsync<EndOfShiftReportDTO>(cancellationToken: cancellationToken);
    }

    public async Task SaveReportPdfAsync(DateTime startDate, DateTime endDate, string filePath, CancellationToken cancellationToken = default)
    {

        var report = await GetReportPdfAsync(startDate, endDate, cancellationToken);
        var generator = new PdfReportGenerator();
        var pdfBytes = generator.GenerateReport(report);

        await File.WriteAllBytesAsync(filePath, pdfBytes, cancellationToken);
    }
}
