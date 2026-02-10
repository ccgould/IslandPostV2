namespace IslandPostPOS.Shared.DTOs
{
    public class SaleReportDTO
    {
        public string? RegistrationDate { get; set; }
        public string? SaleNumber { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentClient { get; set; }
        public string? ClientName { get; set; }
        public decimal? SubTotalSale { get; set; }
        public decimal? TaxTotalSale { get; set; }
        public decimal? TotalSale { get; set; }
        public string? Product { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Total { get; set; }
        public string? PaymentMethod { get; set; }
        public string? RegisterUser { get; set; }
    }
}
