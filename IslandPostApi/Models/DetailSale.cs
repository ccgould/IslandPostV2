using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IslandPostApi.Models;

[Table(nameof(DetailSale))]
public partial class DetailSale
{
    [Key]
    public int IdDetailSale { get; set; }
    public int? IdSale { get; set; }
    public int? IdProduct { get; set; }
    public string? BrandProduct { get; set; }
    public string? DescriptionProduct { get; set; }
    public string? CategoryProducty { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? Total { get; set; }
    public virtual Sale? IdSaleNavigation { get; set; }
}