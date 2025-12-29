namespace AiNoData.Models.Budget
{
    public class MediaChannelAllocationResult
    {
        public string Name { get; set; } = string.Empty;

        public decimal MinSpend { get; set; }
        public decimal MaxSpend { get; set; }
        public decimal ExpectedReturnMultiple { get; set; }
        public decimal RiskWeight { get; set; }

        public decimal AllocatedSpend { get; set; }
        public decimal AllocationPercent { get; set; }

        public decimal ExpectedGrossReturn => AllocatedSpend * ExpectedReturnMultiple;
    }
}
