using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IslandPostApi.Models;

[Table(nameof(Rol))]
public partial class Rol
{
    public Rol()
    {
        RolMenus = new HashSet<RolMenu>();
        Users = new HashSet<User>();
    }

    [Key]
    public int IdRol { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? RegistrationDate { get; set; }

    public virtual ICollection<RolMenu> RolMenus { get; set; }
    public virtual ICollection<User> Users { get; set; }
}
