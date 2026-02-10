using System;
using System.Collections.Generic;
using System.Text;

namespace IslandPostPOS.Shared.DTOs
{
    public class UserLoginDTO
    {
        public string? Email { get; set; }
        public string? PassWord { get; set; }
        public bool KeepLoggedIn { get; set; }
    }
}
