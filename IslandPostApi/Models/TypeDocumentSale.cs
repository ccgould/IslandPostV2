using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IslandPostApi.Models;

[Table(nameof(TypeDocumentSale))]
public partial class TypeDocumentSale
{
    public TypeDocumentSale()
    {
        Sales = new HashSet<Sale>();
    }
    [Key]
    public int IdTypeDocumentSale { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? RegistrationDate { get; set; }

    [JsonIgnore]
    public virtual ICollection<Sale> Sales { get; set; }
}
