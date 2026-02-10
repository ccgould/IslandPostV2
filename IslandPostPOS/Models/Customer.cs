using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace IslandPostPOS.Models;
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime RegistrationDateAtStore { get; set; }
    public bool IsMemeber { get; set; }
    public decimal MembershipDiscount { get; set; }
    public DateTime LastTime { get; set; }
    public BitmapImage Image { get; set; }
}
