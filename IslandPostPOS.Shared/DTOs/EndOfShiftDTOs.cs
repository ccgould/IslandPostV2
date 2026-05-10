using System;
using System.Collections.Generic;

namespace IslandPostPOS.Shared.DTOs
{
    // Composite report DTO
    public class EndOfShiftReportDTO
    {
        public TodaySalesSummaryDTO Summary { get; set; }
        public List<PaymentBreakdownDTO> PaymentBreakdown { get; set; }
        public List<CashierBreakdownDTO> CashierBreakdown { get; set; }
        public List<ProductReportDTO> TopProductsByQuantity { get; set; }
        public List<ProductReportDTO> TopProductsByRevenue { get; set; }
        public List<SaleDTO> AllSalesToday { get; set; }
    }

    // 1. Sales Summary
    public class TodaySalesSummaryDTO
    {
        public decimal TotalSubtotal { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal TotalSales { get; set; }
        public int TransactionCount { get; set; }
    }

    // 2. Payment Breakdown
    public class PaymentBreakdownDTO
    {
        public string Method { get; set; }
        public decimal Amount { get; set; }
    }

    // 3. Cashier Breakdown
    public class CashierBreakdownDTO
    {
        public int CashierId { get; set; }
        public int TransactionsHandled { get; set; }
        public decimal CashierSales { get; set; }
        public string CashierName { get; set; }
    }

    // 4 & 5. Product Reports
    public class ProductReportDTO
    {
        public int ProductId { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }
}