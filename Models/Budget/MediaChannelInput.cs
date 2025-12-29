namespace AiNoData.Models.Budget
{
    public class MediaChannelInput
    {
        public string Name { get; set; } = string.Empty;
        public decimal MinSpend { get; set; }
        public decimal MaxSpend { get; set; }
        public decimal ExpectedReturnMultiple { get; set; }
        public decimal RiskWeight { get; set; }
    }
}
