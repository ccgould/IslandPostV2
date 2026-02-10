using IslandPostApi.Contracts;
using IslandPostApi.Data;
using IslandPostApi.Models;
using IslandPostApi.Utilities;
using IslandPostPOS.Shared.DTOs;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IslandPostApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IslandPostDbContext context;

        public ProductService(IslandPostDbContext context)
        {
            this.context = context;
        }

        public async Task<List<ProductDTO>> SearchForProducts(string search)
        {
            var products = await context.Products
                .Where(p => p.IsActive == true
                            && p.Quantity > 0
                            && (
                                p.BarCode.Contains(search) ||
                                p.Brand.Contains(search) ||
                                p.Description.Contains(search)
                               )
                      )
                .Include(p => p.IdCategoryNavigation)
                .ToListAsync();

            return products.Select(p => new ProductDTO
            {
                IdProduct = p.IdProduct,
                BarCode = p.BarCode,
                Brand = p.Brand,
                Description = p.Description,
                IdCategory = p.IdCategory,
                NameCategory = p.IdCategoryNavigation?.Description,
                Quantity = p.Quantity,
                Price = p.Price ?? 0m,
                IsActive = p.IsActive.HasValue ? (p.IsActive.Value ? 1 : 0) : null,
                IsDiscount = p.IsDiscount.HasValue ? (p.IsDiscount.Value ? 1 : 0) : null
                // ⚠️ PhotoUrl/ThumbnailUrl handled in controller
            }).ToList();
        }

        public async Task<ProductDTO> AddAsync(ProductDTO dto)
        {
            var productExists = await context.Products
                .FirstOrDefaultAsync(p => p.BarCode == dto.BarCode);

            if (productExists != null)
                throw new TaskCanceledException("The barcode already exists");

            var entity = new Product
            {
                BarCode = dto.BarCode,
                Brand = dto.Brand,
                Description = dto.Description,
                IdCategory = dto.IdCategory,
                Quantity = dto.Quantity ?? 0,
                Price = dto.Price,
                IsActive = dto.IsActive == 1,
                IsDiscount = dto.IsDiscount == 1,
                RegistrationDate = DateTime.UtcNow
            };

            if (dto.Photo != null)
                entity.Photo = dto.Photo;

            await context.Products.AddAsync(entity);
            await context.SaveChangesAsync();

            if (entity.IdProduct == 0)
                throw new TaskCanceledException("Failed to create product");

            if (entity.IdCategory != null)
            {
                await context.Entry(entity)
                    .Reference(p => p.IdCategoryNavigation)
                    .LoadAsync();
            }

            return new ProductDTO
            {
                IdProduct = entity.IdProduct,
                BarCode = entity.BarCode,
                Brand = entity.Brand,
                Description = entity.Description,
                IdCategory = entity.IdCategory,
                NameCategory = entity.IdCategoryNavigation?.Description,
                Quantity = entity.Quantity,
                Price = entity.Price ?? 0m,
                IsActive = entity.IsActive.HasValue ? (entity.IsActive.Value ? 1 : 0) : null,
                IsDiscount = entity.IsDiscount.HasValue ? (entity.IsDiscount.Value ? 1 : 0) : null
            };
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            var product = await context.Products
                .AsNoTracking()
                .Include(p => p.IdCategoryNavigation)
                .FirstOrDefaultAsync(p => p.IdProduct == id);

            if (product == null) return null;

            return new ProductDTO
            {
                IdProduct = product.IdProduct,
                BarCode = product.BarCode,
                Brand = product.Brand,
                Description = product.Description,
                IdCategory = product.IdCategory,
                NameCategory = product.IdCategoryNavigation?.Description,
                Quantity = product.Quantity,
                Price = product.Price ?? 0m,
                IsActive = product.IsActive.HasValue ? (product.IsActive.Value ? 1 : 0) : null,
                IsDiscount = product.IsDiscount.HasValue ? (product.IsDiscount.Value ? 1 : 0) : null
            };
        }

        public async Task<ProductDTO> EditAsync(ProductDTO dto)
        {
            var entity = await context.Products
                .FirstOrDefaultAsync(p => p.IdProduct == dto.IdProduct);

            if (entity == null)
                throw new KeyNotFoundException("Product not found");

            if (!string.IsNullOrEmpty(dto.BarCode))
                entity.BarCode = dto.BarCode;
            if (!string.IsNullOrEmpty(dto.Brand))
                entity.Brand = dto.Brand;
            if (!string.IsNullOrEmpty(dto.Description))
                entity.Description = dto.Description;
            if (dto.IdCategory.HasValue)
                entity.IdCategory = dto.IdCategory;
            if (dto.Quantity.HasValue)
                entity.Quantity = dto.Quantity.Value;
            if (dto.Price.HasValue)
                entity.Price = dto.Price.Value;
            if (dto.IsActive.HasValue)
                entity.IsActive = dto.IsActive == 1;
            if (dto.IsDiscount.HasValue)
                entity.IsDiscount = dto.IsDiscount == 1;
            if (dto.Photo != null)
                entity.Photo = dto.Photo;

            await context.SaveChangesAsync();

            if (entity.IdCategory != null)
            {
                await context.Entry(entity)
                    .Reference(p => p.IdCategoryNavigation)
                    .LoadAsync();
            }

            return new ProductDTO
            {
                IdProduct = entity.IdProduct,
                BarCode = entity.BarCode,
                Brand = entity.Brand,
                Description = entity.Description,
                IdCategory = entity.IdCategory,
                NameCategory = entity.IdCategoryNavigation?.Description,
                Quantity = entity.Quantity,
                Price = entity.Price ?? 0m,
                IsActive = entity.IsActive.HasValue ? (entity.IsActive.Value ? 1 : 0) : null,
                IsDiscount = entity.IsDiscount.HasValue ? (entity.IsDiscount.Value ? 1 : 0) : null
            };
        }

        public async Task<bool> DeleteAsync(int idProduct)
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.IdProduct == idProduct);

            if (product == null)
                throw new TaskCanceledException("The product does not exist");

            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProductDTO>> GetAllAsync()
        {
            var products = await context.Products
                .AsNoTracking()
                .Include(p => p.IdCategoryNavigation)
                .ToListAsync();

            return products.Select(p => new ProductDTO
            {
                IdProduct = p.IdProduct,
                BarCode = p.BarCode,
                Brand = p.Brand,
                Description = p.Description,
                IdCategory = p.IdCategory,
                NameCategory = p.IdCategoryNavigation?.Description,
                Quantity = p.Quantity,
                Price = p.Price ?? 0m,
                IsActive = p.IsActive.HasValue ? (p.IsActive.Value ? 1 : 0) : null,
                IsDiscount = p.IsDiscount.HasValue ? (p.IsDiscount.Value ? 1 : 0) : null
            }).ToList();
        }

        public async Task UpdateProductPhotoAsync(int id, byte[] photoBytes)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) throw new Exception("Product not found");

            product.Photo = photoBytes;

            // ✅ Keep PhotoThumbnail only in entity
            product.PhotoThumbnail = ImageHelper.GenerateThumbnail(photoBytes);

            await context.SaveChangesAsync();
        }

        public async Task<(List<ProductDTO> Items, int TotalCount)> GetProductsPagedAsync(
            int pageNumber, int pageSize, ProductFilterDTO filter)
        {
            var query = context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(p =>
                    (p.Description != null && p.Description.Contains(filter.Search)) ||
                    (p.Brand != null && p.Brand.Contains(filter.Search)) ||
                    (p.BarCode != null && p.BarCode.Contains(filter.Search)));
            }

            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(p => p.IdCategoryNavigation != null &&
                                         p.IdCategoryNavigation.Description == filter.Category);
            }

            if (!string.IsNullOrEmpty(filter.Brand))
            {
                query = query.Where(p => p.Brand != null && p.Brand == filter.Brand);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                if (filter.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.IsActive == true);
                else if (filter.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.IsActive == false);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.IdProduct)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (Product.ToDTOList(items), totalCount);
        }
    }
}