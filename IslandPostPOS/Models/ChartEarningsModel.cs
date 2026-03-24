using System;

namespace IslandPostPOS.Models
{
    public class ChartEarningsModel
    {
        public DateTime Date { get; set; }       // for chart axis
        public string DateLabel => Date.ToString("yyyy-MM-dd"); // optional label
        public double Amount { get; set; }       // for chart values
    }
}
