using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IslandPostApi.Models;

[Table("Users")]
public partial class User
{
    public User()
    {
        Sales = new HashSet<Sale>();
    }

    [Key]
    public int IdUsers { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? IdRol { get; set; }
    public string? Password { get; set; }
    public byte[]? Photo { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? RegistrationDate { get; set; }

    public virtual Rol? IdRolNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<Sale> Sales { get; set; }
}
