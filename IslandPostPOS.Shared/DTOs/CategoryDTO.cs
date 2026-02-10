using System;
using System.Collections.Generic;
using System.Text;

namespace IslandPostPOS.Shared.DTOs
{
    public class CategoryDTO
    {
        public int IdCategory { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
