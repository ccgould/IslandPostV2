using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IslandPostApi.Models;

[Table("CorrelativeNumber")]
public partial class CorrelativeNumber
{
    [Key]
    public int IdCorrelativeNumber { get; set; }
    public int? LastNumber { get; set; }
    public int? QuantityDigits { get; set; }
    public string? Management { get; set; }
    public DateTime? DateUpdate { get; set; }
}
