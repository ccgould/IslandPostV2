using System;

namespace IslandPostPOS.Models;

internal class Email
{
    public int IdEmail { get; set; }
    public int IdSale { get; set; }
    public DateTime Date { get; set; }
    public string EmailAddress { get; set; }
    public string Url { get; set; }
    public bool IsSent { get; set; }

}
