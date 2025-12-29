// Models/MonthlyAllocationSnapshot.cs
using System.Collections.Generic;

namespace AiNoData.Models.Budget
{
    public class MonthlyAllocationSnapshot
    {
        public int PeriodIndex { get; set; }              // 1, 2, 3, ... (months)
        public decimal StartingBudget { get; set; }       // Budget at start of month
        public decimal TotalSpend { get; set; }           // Sum of TV media spends this month
        public decimal TotalSales { get; set; }           // Sum of PR * spend for all TV stations
        public decimal EndingBudget { get; set; }         // New budget after reinvestment

        public List<MediaChannelAllocationResult> ChannelAllocations { get; set; } = new();
    }
}
