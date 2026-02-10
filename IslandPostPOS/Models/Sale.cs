namespace IslandPostPOS.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using System;

public partial class Sale : ObservableObject
{
    [ObservableProperty]
    private int idSale;

    [ObservableProperty]
    private string saleNumber;

    [ObservableProperty]
    private int idTypeDocumentSale;

    [ObservableProperty]
    private int idUsers;

    [ObservableProperty]
    private string customerDocument;

    [ObservableProperty]
    private string clientName;

    [ObservableProperty]
    private decimal subtotal;

    [ObservableProperty]
    private decimal totalTaxes;

    [ObservableProperty]
    private decimal total;

    [ObservableProperty]
    private DateTime registrationDate;

    [ObservableProperty]
    private string paymentMethod;

    [ObservableProperty]
    private string note;

    [ObservableProperty]
    private int idDiscount;

    // Whenever Subtotal or TotalTaxes changes, recalc Total
    partial void OnSubtotalChanged(decimal value)
    {
        Total = value + TotalTaxes;
    }

    partial void OnTotalTaxesChanged(decimal value)
    {
        Total = Subtotal + value;
    }
}
