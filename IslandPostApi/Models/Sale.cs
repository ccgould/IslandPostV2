using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IslandPostApi.Models;

[Table(nameof(Sale))]
public partial class Sale
{
    public Sale()
    {
        DetailSales = new HashSet<DetailSale>();
    }
    [Key]
    public int IdSale { get; set; }
    public string? SaleNumber { get; set; }
    public int? IdTypeDocumentSale { get; set; }
    public int? IdUsers { get; set; }
    public string? CustomerDocument { get; set; }
    public string? ClientName { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? TotalTaxes { get; set; }
    public decimal? Total { get; set; }
    public DateTime? RegistrationDate { get; set; }

    public virtual TypeDocumentSale? IdTypeDocumentSaleNavigation { get; set; }

    public virtual User? IdUsersNavigation { get; set; }
    
    public virtual ICollection<DetailSale> DetailSales { get; set; }

    public string PaymentMethod { get; set; }
}
