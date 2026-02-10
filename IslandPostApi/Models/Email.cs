using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IslandPostApi.Models;

[Table(nameof(Email))]
public class Email
{
    [Key]
    public int IdEmail { get; set; }
    public int IdSale { get; set; }
    public DateTime? Date { get; set; }
    public string? EmailAddress { get; set; }
    public string? Url { get; set; }
    public bool? IsSent { get; set; }
}

