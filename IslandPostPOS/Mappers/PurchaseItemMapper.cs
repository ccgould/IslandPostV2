using IslandPostPOS.Models;
using IslandPostPOS.Shared.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace IslandPostPOS.Mappers
{
    public static class PurchaseItemMapper
    {
        public static PurchaseItem FromProductDto(ProductDTO product, int quantity = 1)
        {
            return new PurchaseItem(product, quantity);
        }

        public static PurchaseItem FromDetailSaleDto(DetailSaleDTO detail)
        {
            return new PurchaseItem(detail);
        }

        public static List<PurchaseItem> FromDetailSaleDtos(IEnumerable<DetailSaleDTO> details)
        {
            return details.Select(FromDetailSaleDto).ToList();
        }

        public static List<PurchaseItem> FromProductDtos(IEnumerable<ProductDTO> products)
        {
            return products.Select(p => FromProductDto(p)).ToList();
        }
    }
}