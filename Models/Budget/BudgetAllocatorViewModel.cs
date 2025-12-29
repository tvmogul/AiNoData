using System.Collections.Generic;
using System.Linq;

namespace AiNoData.Models.Budget
{
    public class BudgetAllocatorViewModel
    {
        public decimal TotalBudget { get; set; }

        public List<MediaChannelInput> Channels { get; set; } = new();

        public List<MediaChannelAllocationResult> AllocationResults { get; set; } = new();

        public bool HasResults => AllocationResults.Count > 0;

        public decimal TotalAllocated =>
            HasResults ? decimal.Round(AllocationResults.Sum(c => c.AllocatedSpend), 2) : 0m;

        public decimal TotalExpectedGrossReturn =>
            HasResults ? decimal.Round(AllocationResults.Sum(c => c.ExpectedGrossReturn), 2) : 0m;

        // Number of months to simulate dynamic TV-station reallocation and compounding.
        public int MonthsToSimulate { get; set; } = 12;

        // Number of new TV stations added (tested) per month.
        public int NewStationsPerMonth { get; set; } = 40;

        // Fraction of buys that are cancelled before airing (e.g., 0.06 = 6%).
        public decimal CancellationRate { get; set; } = 0.06m;

        // Infomercial show type selected by the user: A, B, C, or D.
        public string ShowType { get; set; } = "C";

        // One-month supply price of the diet (user input).
        public decimal MonthlyPrice { get; set; } = 59.95m;

        // One-year supply upsell price of the diet (user input).
        public decimal YearlyPrice { get; set; } = 499.00m;

        // Parameters ("knobs") for the Z3D engine.

        // α: how aggressively we reward high-ROI stations.
        public decimal Alpha { get; set; } = 0.8m;

        // λ: strength of the budget-sum constraint (Σq ≈ 1).
        public decimal Lambda { get; set; } = 5.0m;

        // Damping: how quickly momentum decays (stability vs. responsiveness).
        public decimal Damping { get; set; } = 0.3m;

        // Steps: number of discrete time steps per month.
        public int TimeSteps { get; set; } = 64;

        // Month-by-month evolution of the budget and TV-station activity.
        public List<MonthlyAllocationSnapshot> MonthlyTimeline { get; set; } = new();

        public bool HasMonthlyTimeline => MonthlyTimeline.Count > 0;
    }
}





