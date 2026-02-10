using System;

namespace IslandPostPOS.Shared.DTOs
{
    public class UserDTO
    {
        public int IdUsers { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? IdRol { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? RegistrationDate { get; set; }

        // Optional: include role name if you want to expose it
        public string? RoleName { get; set; }
    }
}