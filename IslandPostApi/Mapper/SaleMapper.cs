using IslandPostApi.Models;
using IslandPostPOS.Shared.DTOs;

namespace IslandPostApi.Mapper
{
    public static class SaleMapper
    {
        public static SaleDTO ToDto(Sale sale)
        {
            return new SaleDTO
            {
                IdSale = sale.IdSale,
                Subtotal = sale.Subtotal,
                TotalTaxes = sale.TotalTaxes,
                Total = sale.Total,
                RegistrationDate = sale.RegistrationDate,
                PaymentMethod = sale.PaymentMethod,
                Note = sale.Note,
                DetailSales = sale.DetailSales.Select(d => new DetailSaleDTO
                {
                    IdDetailSale = d.IdDetailSale,
                    IdSale = d.IdSale,
                    IdProduct = d.IdProduct,
                    Quantity = d.Quantity,
                    Price = d.Price,
                    Total = d.Total,

                    // Hydrate from Product navigation if available
                    DescriptionProduct = d.Product?.Description ?? d.DescriptionProduct,
                    BrandProduct = d.Product?.Brand ?? d.BrandProduct,
                    CategoryProducty = d.Product?.IdCategoryNavigation?.Description ?? d.CategoryProducty
                }).ToList()
            };
        }

        public static Sale ToEntity(SaleDTO dto)
        {
            return new Sale
            {
                IdSale = dto.IdSale,
                Subtotal = dto.Subtotal,
                TotalTaxes = dto.TotalTaxes,
                Total = dto.Total,
                RegistrationDate = dto.RegistrationDate,
                PaymentMethod = dto.PaymentMethod,
                Note = dto.Note,
                DetailSales = dto.DetailSales.Select(d => new DetailSale
                {
                    IdDetailSale = d.IdDetailSale,
                    IdSale = d.IdSale,
                    IdProduct = d.IdProduct,
                    Quantity = d.Quantity,
                    Price = d.Price,
                    Total = d.Total,

                    // Snapshot fields (denormalized copy)
                    DescriptionProduct = d.DescriptionProduct,
                    BrandProduct = d.BrandProduct,
                    CategoryProducty = d.CategoryProducty
                }).ToList()
            };
        }
    }
}
