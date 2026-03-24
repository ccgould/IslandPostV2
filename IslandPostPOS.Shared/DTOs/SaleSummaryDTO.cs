using System;
using System.Collections.Generic;
using System.Text;

namespace IslandPostPOS.Shared.DTOs
{
    public class SalesSummaryDTO
    {
        public List<SaleReportDTO> DailyTotals { get; set; }
        public List<SaleReportDTO> WeeklyDailyTotals { get; set; }
        public List<SaleReportDTO> MonthlyTotals { get; set; }

        public decimal TodayTotal { get; set; }
        public decimal WeeklyTotal { get; set; }
        public decimal MonthlyTotal { get; set; }
    }
}
