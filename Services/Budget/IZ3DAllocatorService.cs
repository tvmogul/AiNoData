// Services/IZ3DAllocatorService.cs
using System.Collections.Generic;
using AiNoData.Models.Budget;

namespace AiNoData.Services.Budget
{
    public interface IZ3DAllocatorService
    {
        List<MediaChannelAllocationResult> OptimizeBudget(
            decimal totalBudget,
            IReadOnlyList<MediaChannelInput> channels);

        // ORIGINAL SIGNATURE (kept for backward compatibility)
        List<MonthlyAllocationSnapshot> RunSimulation(
            decimal initialBudget,
            int months,
            int newStationsPerMonth,
            decimal cancellationRate,
            char showTypeChar,
            decimal monthlyPrice,
            decimal yearlyPrice,
            int timeSteps);
    }
}
