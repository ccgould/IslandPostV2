using IslandPostPOS.Shared.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IslandPostApi.Models;

[Table(nameof(Product))]
public partial class Product
{
    [Key]
    public int IdProduct { get; set; }
    public string? BarCode { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public int? IdCategory { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public byte[]? Photo { get; set; }           // full-size image
    public byte[]? PhotoThumbnail { get; set; }  // smaller version

    public bool? IsActive { get; set; }
    public bool? IsDiscount { get; set; }
    public DateTime? RegistrationDate { get; set; }

    public virtual Category? IdCategoryNavigation { get; set; }

    public static ProductDTO ToDTO(Product product)
    {
        return new ProductDTO
        {
            IdProduct = product.IdProduct,
            BarCode = product.BarCode,
            Brand = product.Brand,
            Description = product.Description,
            IdCategory = product.IdCategory,
            NameCategory = product.IdCategoryNavigation?.Description,
            Quantity = product.Quantity,
            Price = product.Price,
            Photo = product.Photo,
            IsActive = product.IsActive == true ? 1 : 0,
            IsDiscount = product.IsDiscount == true ? 1 : 0,
            LastUpdated = product.RegistrationDate,
            Currency = "USD" // or pull from config
        };
    }

    public static List<ProductDTO> ToDTOList(IEnumerable<Product> products)
        => products.Select(ToDTO).ToList();
}