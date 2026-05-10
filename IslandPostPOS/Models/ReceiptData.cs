using System.Collections.Generic;

namespace IslandPostPOS.Models;

public class ReceiptItem
{
    public string description { get; set; }
    public int qty { get; set; }
    public decimal unitprice { get; set; }
}

public class ReceiptData
{
    public List<ReceiptItem> items { get; set; } = new();
    public string company_name { get; set; }
    public string company_address { get; set; }
    public string company_email { get; set; }
    public string receipt_no { get; set; }
    public string receipt_date { get; set; }
    public string footer { get; set; }
    public string tax { get; set; }
    public string logo { get; set; }
    public string currency { get; set; }
}