using CommunityToolkit.Mvvm.ComponentModel;
using IslandPostPOS.Helpers;
using IslandPostPOS.Models;
using IslandPostPOS.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;


namespace IslandPostPOS.Services;

public partial class ProductService : DataLoaderService
{
    [ObservableProperty] private ObservableCollection<ProductDTO> products;
    [ObservableProperty] private ObservableCollection<CategoryDTO> categories;
    [ObservableProperty] private ObservableCollection<SaleDTO> salesHistory = new();
    [ObservableProperty] private CurrentUserInfo? currentUser;

    public CustomFiltering SqlFilterBehavior { get; }

    public ProductService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
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

    public async Task SearchAndUpdateProductsAsync(string search, CancellationToken cancellationToken = default)
    {
        var results = await SearchForProductsAsync(search, cancellationToken);
        Products = new ObservableCollection<ProductDTO>(results);
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
        var id = jwt.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
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
}