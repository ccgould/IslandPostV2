using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IslandPostPOS.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Initials { get; set;  }
        public int Role { get; set; }
        public int State { get; set; }
        public BitmapSource ProfileImage { get; set; }

    }
}
