using IslandPostApi.Data;
using IslandPostApi.Models;
using Microsoft.Extensions.Caching.Memory;

namespace IslandPostApi.Services
{
    public class ProductSearchService
    {
        private readonly IslandPostDbContext _context;
        private readonly IMemoryCache _cache;

        public ProductSearchService(IslandPostDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        //public async Task<List<Product>> SearchProductsAsync(string term)
        //{
        //    var key = $"search_{term.ToLower()}";

        //    return await _cache.GetOrCreateAsync<List<Product>>(key, async entry =>
        //    {
        //        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);

        //        return await _context.Products
        //            .Where(p => p.Description.Contains(term) || p.BarCode.Contains(term))
        //            .Take(50)
        //            .ToListAsync();
        //    });
        //}  
    }
}
