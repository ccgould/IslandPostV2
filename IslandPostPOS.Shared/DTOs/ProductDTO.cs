using System;

namespace IslandPostPOS.Shared.DTOs
{
    public class ProductDTO
    {
        public int IdProduct { get; set; }
        public string? BarCode { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public int? IdCategory { get; set; }
        public string? NameCategory { get; set; }

        public int? Quantity { get; set; }
        public decimal? Price { get; set; }

        public byte[]? Photo { get; set; }

        // Image endpoints (served by controller)
        public string? PhotoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Database contract: int? values
        public int? IsActive { get; set; }
        public int? IsDiscount { get; set; }

        // UI-facing properties (safe defaults)
        public double QuantityUI
        {
            get => (double)(Quantity ?? 0);
            set => Quantity = (int)value;
        }

        // Optional: metadata for client-side caching or display
        public DateTime? LastUpdated { get; set; }   // when product was last modified
        public string? Currency { get; set; } = "USD"; // or configurable per store
    }
}