using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IslandPostApi.Models;

[Table(nameof(RolMenu))]
public partial class RolMenu
{
    [Key]
    public int IdRolMenu { get; set; }
    public int? IdRol { get; set; }
    public int? IdMenu { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? RegistrationDate { get; set; }

    public virtual Menu? IdMenuNavigation { get; set; }
    public virtual Rol? IdRolNavigation { get; set; }
}