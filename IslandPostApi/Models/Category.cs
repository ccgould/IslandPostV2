using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IslandPostApi.Models;

[Table("Category")]
public partial class Category
{
    public Category()
    {
        Products = new HashSet<Product>();
    }

    [Key]
    public int IdCategory { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? RegistrationDate { get; set; }

    public virtual ICollection<Product> Products { get; set; }
}
