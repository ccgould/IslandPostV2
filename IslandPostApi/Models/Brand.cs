using System.ComponentModel.DataAnnotations;

namespace IslandPostApi.Models
{
    public class Brand
    {
        [Key]
        public int idBrand { get; set; }
        public string Name { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
