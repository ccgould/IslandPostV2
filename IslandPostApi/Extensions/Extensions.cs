using IslandPostApi.Models;
using IslandPostPOS.Shared.DTOs;

namespace IslandPostApi.Extensions
{
    public static class ProductExtensions
    {
        public static ProductDTO ToDTO(this Product product)
        {
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
                IsDiscount = product.IsDiscount.HasValue ? (product.IsDiscount.Value ? 1 : 0) : null,

                // ⚠️ No Photo or Base64 here — handled via endpoints
                PhotoUrl = null,
                ThumbnailUrl = null
            };
        }

        public static List<ProductDTO> ToDTOList(this IEnumerable<Product> products)
        {
            if (products == null) return new List<ProductDTO>();
            return products.Select(p => p.ToDTO()).ToList();
        }

        public static Product ToEntity(this ProductDTO dto)
        {
            if (dto == null) return null;

            return new Product
            {
                IdProduct = dto.IdProduct,
                BarCode = dto.BarCode,
                Brand = dto.Brand,
                Description = dto.Description,
                IdCategory = dto.IdCategory,
                Quantity = dto.Quantity,
                Price = dto.Price,
                Photo = dto.Photo, // only accept raw byte[] if provided
                IsActive = dto.IsActive.HasValue ? dto.IsActive.Value == 1 : null,
                IsDiscount = dto.IsDiscount.HasValue ? dto.IsDiscount.Value == 1 : null,
                RegistrationDate = DateTime.Now
            };
        }


        public static List<Product> ToEntityList(this IEnumerable<ProductDTO> dtos)
        {
            if (dtos == null) return new List<Product>();
            return dtos.Select(d => d.ToEntity()).ToList();
        }
    }
}