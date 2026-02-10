using System;
using System.Collections.Generic;
using System.Text;

namespace IslandPostPOS.Shared.DTOs
{
    public class ProductFilterDTO
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? Status { get; set; } // "Active" / "Inactive"
    }
}
