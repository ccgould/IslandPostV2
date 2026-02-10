using System;
using System.Collections.Generic;

namespace IslandPostPOS.Shared.DTOs
{
    public class SaleDTO
    {
        public int IdSale { get; set; }
        public string? SaleNumber { get; set; }
        public int? IdTypeDocumentSale { get; set; }
        public int? IdUsers { get; set; }
        public string? Users { get; set; }
        public string? CustomerDocument { get; set; }
        public string? ClientName { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? TotalTaxes { get; set; }
        public decimal? Total { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? PaymentMethod { get; set; }

        // If you want to expose child details in the DTO:
        public List<DetailSaleDTO>? DetailSales { get; set; }
        public string FormatAmount(decimal? amount) => amount?.ToString("C") ?? "NA";

    }
}

