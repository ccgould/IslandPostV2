using System;

namespace IslandPostPOS.Shared.DTOs
{
    public class TransactionDTO
    {
        public int IdTransaction { get; set; }
        public int IdPaymentMethod { get; set; }
        public DateTime RegisteredDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethodName { get; set; }

    }
}