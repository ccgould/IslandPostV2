using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IslandPostApi.Models;

[Table("PaymentType")]
public class Transaction
{
    [Key]
    public int IdTransaction { get; set; }
    public int IdPaymentMethod { get; set; }
    public DateTime RegisteredDate { get; set; }
    public decimal Amount { get; set; }
}
