using CommunityToolkit.Mvvm.ComponentModel;
using IslandPostPOS.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IslandPostPOS.Models
{
    public partial class Product : ObservableObject
    {
        public int IdProduct { get; set; }
        [ObservableProperty] private string? barCode;
        [ObservableProperty] private string? brand;
        [ObservableProperty] private string? description;
        public int? IdCategory { get; set; }
        [ObservableProperty] private int quantity;
        [ObservableProperty] private decimal? price;
        [ObservableProperty] private byte[]? photo;
        [ObservableProperty] private bool? isActive = false;
        [ObservableProperty] private bool? isDiscount = false;
        public DateTime? RegistrationDate { get; set; }


        public virtual Category? IdCategoryNavigation { get; set; }
    }
}