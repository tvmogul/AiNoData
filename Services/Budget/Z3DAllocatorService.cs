// Services/Z3DAllocatorService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using AiNoData.Models.Budget;

namespace AiNoData.Services.Budget
{
    public class Z3DAllocatorService : IZ3DAllocatorService
    {
        private readonly Random _random = new Random();

        public List<MediaChannelAllocationResult> OptimizeBudget(
            decimal totalBudget,
            IReadOnlyList<MediaChannelInput> channels)
        {
            if (totalBudget <= 0 || channels == null || channels.Count == 0)
            {
                return new List<MediaChannelAllocationResult>();
            }

            var sanitized = channels.Select(c => new
            {
                c.Name,
                Min = Math.Max(0m, c.MinSpend),
                Max = Math.Max(0m, c.MaxSpend),
                Roi = Math.Max(0.01m, c.ExpectedReturnMultiple),
                Risk = Clamp(c.RiskWeight, 0m, 1m)
            }).ToList();

            sanitized = sanitized.Select(c => new
            {
                c.Name,
                c.Min,
                Max = Math.Max(c.Min, c.Max),
                c.Roi,
                c.Risk
            }).ToList();

            var sumMin = sanitized.Sum(c => c.Min);
            if (sumMin > totalBudget)
            {
                var scale = totalBudget / sumMin;
                sanitized = sanitized.Select(c => new
                {
                    c.Name,
                    Min = decimal.Round(c.Min * scale, 2),
                    Max = decimal.Round(c.Max * scale, 2),
                    c.Roi,
                    c.Risk
                }).ToList();
            }

            var allocations = sanitized.ToDictionary(
                c => c.Name,
                c => c.Min);

            var remainingBudget = totalBudget - allocations.Values.Sum();

            const int maxIterations = 32;
            var activeNames = sanitized.Select(c => c.Name).ToHashSet();

            for (int iter = 0; iter < maxIterations && remainingBudget > 0.01m && activeNames.Count > 0; iter++)
            {
                var utilities = new Dictionary<string, decimal>();
                foreach (var c in sanitized.Where(c => activeNames.Contains(c.Name)))
                {
                    var riskFactor = 1m - (0.7m * c.Risk);
                    var utility = c.Roi * riskFactor;

                    var current = allocations[c.Name];
                    var saturation = current / (c.Max + 1m);
                    var diminishing = 1m - 0.5m * Clamp(saturation, 0m, 1m);

                    utility *= diminishing;
                    utilities[c.Name] = Math.Max(0.0001m, utility);
                }

                var utilitySum = utilities.Values.Sum();
                if (utilitySum <= 0m) break;

                var allocationChanged = false;

                foreach (var c in sanitized.Where(c => activeNames.Contains(c.Name)))
                {
                    if (remainingBudget <= 0.01m) break;

                    var share = utilities[c.Name] / utilitySum;
                    var desiredIncrement = decimal.Round(remainingBudget * share, 2);
                    if (desiredIncrement <= 0m) continue;

                    var current = allocations[c.Name];
                    var capacity = c.Max - current;

                    if (capacity <= 0.01m)
                    {
                        activeNames.Remove(c.Name);
                        continue;
                    }

                    var actualIncrement = Math.Min(capacity, desiredIncrement);
                    if (actualIncrement <= 0m) continue;

                    allocations[c.Name] = current + actualIncrement;
                    remainingBudget -= actualIncrement;
                    allocationChanged = true;
                }

                if (!allocationChanged) break;
            }

            var results = new List<MediaChannelAllocationResult>();
            foreach (var c in sanitized)
            {
                var allocated = decimal.Round(allocations[c.Name], 2);
                var percent = totalBudget > 0 ? decimal.Round((allocated / totalBudget) * 100m, 2) : 0m;

                results.Add(new MediaChannelAllocationResult
                {
                    Name = c.Name,
                    MinSpend = decimal.Round(c.Min, 2),
                    MaxSpend = decimal.Round(c.Max, 2),
                    ExpectedReturnMultiple = decimal.Round(c.Roi, 2),
                    RiskWeight = decimal.Round(c.Risk, 2),
                    AllocatedSpend = allocated,
                    AllocationPercent = percent
                });
            }

            return results;
        }

        // BACKWARD-COMPATIBLE ENTRY POINT
        public List<MonthlyAllocationSnapshot> RunSimulation(
            decimal initialBudget,
            int months,
            int newStationsPerMonth,
            decimal cancellationRate,
            char showTypeChar,
            decimal monthlyPrice,
            decimal yearlyPrice,
            int timeSteps)
        {
            var timeline = new List<MonthlyAllocationSnapshot>();

            if (initialBudget <= 0 || months <= 0 || newStationsPerMonth <= 0)
            {
                return timeline;
            }

            if (cancellationRate < 0m) cancellationRate = 0m;
            if (cancellationRate > 1m) cancellationRate = 1m;

            if (timeSteps < 1) timeSteps = 1;
            if (timeSteps > 64) timeSteps = 64;

            // IMPORTANT FIX (REALISM + CORRECT PRICE SENSITIVITY):
            // We do NOT model Pull Ratio as a fixed number that ignores the product price.
            // Instead, each station is assigned an underlying "orders per $1 of media" rate (OrdersPerDollar),
            // derived from a PR range at a reference monthly price.
            // Then:
            //  - Front-end sales per $1 = OrdersPerDollar * monthlyPrice  (so changing monthlyPrice changes totals)
            //  - PullRatio(front-end only) is implicit: PR = OrdersPerDollar * monthlyPrice
            //  - Upsells are computed from ORDERS, not from PullRatio directly.
            const decimal referenceMonthlyPrice = 59.95m;

            var activeStations = new List<TvStation>();
            var currentBudget = initialBudget;
            var nextStationId = 1;

            // REALISTIC INVENTORY CAP PER STATION PER MONTH (HALF-HOURS)
            const int maxSpotsPerStationPerMonth = 4;

            for (int month = 1; month <= months; month++)
            {
                var startingBudget = currentBudget;

                // Determine PR range for this show type so we can correlate upsells to pull ratio.
                GetPullRatioRange(showTypeChar, out var prMin, out var prMax);
                var prRange = prMax - prMin;
                if (prRange <= 0m) prRange = 1m;

                for (int i = 0; i < newStationsPerMonth; i++)
                {
                    // Station PR is assigned once (first time we test a station).
                    var prAtReferencePrice = GeneratePullRatio(showTypeChar);

                    // Guard: if referenceMonthlyPrice is ever 0, keep safe.
                    var opd = (referenceMonthlyPrice > 0m) ? (prAtReferencePrice / referenceMonthlyPrice) : 0m;
                    if (opd < 0.0000001m) opd = 0.0000001m;

                    // IMPORTANT (REAL-WORLD MODELING):
                    // Upsell rate varies wildly per station, but is generally correlated with Pull Ratio.
                    // Range: 10% to 90%, with a median around 40% across "typical" stations.
                    // We compute a base upsell from normalized PR, then add a small random perturbation.
                    var prNorm = (prAtReferencePrice - prMin) / prRange; // 0..1
                    if (prNorm < 0m) prNorm = 0m;
                    if (prNorm > 1m) prNorm = 1m;

                    // Choose exponent so prNorm=0.5 maps to 40% (median target).
                    // base = 0.10 + 0.80*(prNorm^p), where p ≈ 1.415 gives 0.40 at prNorm=0.5.
                    const decimal p = 1.4150375m;

                    var baseFrac = 0.10m + 0.80m * Pow(prNorm, p);

                    // Add mild randomness while preserving correlation (±10 pts typical), then clamp.
                    var jitter = ((decimal)_random.NextDouble() * 0.20m) - 0.10m;
                    var upsellRate = baseFrac + jitter;

                    if (upsellRate < 0.10m) upsellRate = 0.10m;
                    if (upsellRate > 0.90m) upsellRate = 0.90m;

                    var station = new TvStation
                    {
                        Id = nextStationId++,
                        SpotCost = GenerateTestCost(),

                        // PullRatio is retained for compatibility/visibility, but is not used as a fixed driver anymore.
                        // The "true" behavior is driven by OrdersPerDollar and current monthlyPrice.
                        PullRatio = prAtReferencePrice,

                        OrdersPerDollar = opd,

                        // Station-specific upsell rate assigned once at first air (and reused for 12 months).
                        UpsellRate = upsellRate,

                        FirstMonth = month,
                        LastActiveMonth = month + 11,
                        IsActive = true
                    };

                    activeStations.Add(station);
                }

                var candidates = activeStations
                    .Where(s => s.IsActive && month >= s.FirstMonth && month <= s.LastActiveMonth)
                    .ToList();

                if (candidates.Count == 0 || currentBudget <= 0m)
                {
                    timeline.Add(new MonthlyAllocationSnapshot
                    {
                        PeriodIndex = month,
                        StartingBudget = startingBudget,
                        TotalSpend = 0m,
                        TotalSales = 0m,
                        EndingBudget = currentBudget,
                        ChannelAllocations = new List<MediaChannelAllocationResult>()
                    });
                    continue;
                }

                var realizedFactor = 1m - cancellationRate;
                if (realizedFactor < 0m) realizedFactor = 0m;
                if (realizedFactor > 1m) realizedFactor = 1m;

                decimal totalMedia = 0m;
                decimal totalGross = 0m;

                int n = candidates.Count;

                // Start neutral, then allow the allocator to shift weight toward higher TOTAL SALES per dollar
                var q = Enumerable.Repeat(1m / n, n).ToArray();

                // INTERNAL REBALANCE STEPS (kept math-opaque; improves credibility by selecting on TOTAL SALES)
                for (int t = 0; t < timeSteps; t++)
                {
                    var scores = new decimal[n];
                    decimal scoreSum = 0m;

                    for (int i = 0; i < n; i++)
                    {
                        var s = candidates[i];

                        // FRONT-END SALES per $1 of media depends on monthlyPrice
                        // pr = OrdersPerDollar * monthlyPrice
                        var opd = s.OrdersPerDollar;
                        if (opd < 0.0000001m) opd = 0.0000001m;

                        var frontEndSalesPerDollar = (monthlyPrice > 0m) ? (opd * monthlyPrice) : 0m;
                        if (frontEndSalesPerDollar < 0.0000001m) frontEndSalesPerDollar = 0.0000001m;

                        // MONTHLY upsell value (do NOT add YearlyPrice every month)
                        var monthlyUpsellValue = (yearlyPrice > 0m) ? (yearlyPrice / 12m) : 0m;

                        // TOTAL SALES per $1 of media:
                        // orders per $1 = OrdersPerDollar
                        // upsell sales per $1 = OrdersPerDollar * UpsellRate * monthlyUpsellValue
                        var upsellSalesPerDollar = (monthlyUpsellValue > 0m && s.UpsellRate > 0m)
                            ? (opd * s.UpsellRate * monthlyUpsellValue)
                            : 0m;

                        var totalSalesPerDollar = frontEndSalesPerDollar + upsellSalesPerDollar;

                        var score = Math.Max(0.0000001m, totalSalesPerDollar);
                        scores[i] = score;
                        scoreSum += score;
                    }

                    if (scoreSum <= 0m) break;

                    // Smooth update so weights don't jump violently
                    const decimal beta = 0.35m;

                    for (int i = 0; i < n; i++)
                    {
                        var target = scores[i] / scoreSum;
                        q[i] = (1m - beta) * q[i] + beta * target;
                    }

                    // Normalize q
                    var qSum = q.Sum();
                    if (qSum > 0m)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            q[i] = q[i] / qSum;
                        }
                    }
                }

                // IMPORTANT FIX:
                // The previous version could spend almost nothing when the station count got large (q becomes tiny),
                // causing flat charts and making price / upsells appear to "do nothing".
                // This allocator now spends the available cash by ranking stations on expected TOTAL SALES per dollar,
                // buying up to inventory caps and never exceeding available cash.
                var monthlyUpsellValueForSpend = (yearlyPrice > 0m) ? (yearlyPrice / 12m) : 0m;

                var ranked = candidates
                    .Select((s, i) =>
                    {
                        var opd = s.OrdersPerDollar;
                        if (opd < 0.0000001m) opd = 0.0000001m;

                        var frontEndSalesPerDollar = (monthlyPrice > 0m) ? (opd * monthlyPrice) : 0m;
                        if (frontEndSalesPerDollar < 0.0000001m) frontEndSalesPerDollar = 0.0000001m;

                        var upsellSalesPerDollar = (monthlyUpsellValueForSpend > 0m && s.UpsellRate > 0m)
                            ? (opd * s.UpsellRate * monthlyUpsellValueForSpend)
                            : 0m;

                        // Use q as the "Z3D-weight" (math opaque) and expected total sales per $ as the tie-breaker.
                        // This keeps the demo credible while ensuring we actually BUY media.
                        var expectedTotalSalesPerDollar = frontEndSalesPerDollar + upsellSalesPerDollar;

                        return new
                        {
                            Station = s,
                            Weight = q[i],
                            Score = expectedTotalSalesPerDollar
                        };
                    })
                    .OrderByDescending(x => x.Weight)
                    .ThenByDescending(x => x.Score)
                    .ToList();

                var remainingCash = currentBudget;

                // Track per-station purchased spots so we can do multiple passes without exceeding caps.
                var spotsBought = ranked.ToDictionary(x => x.Station.Id, _ => 0);

                // PASS 1: attempt proportional buying (based on weight) to get a realistic distribution
                foreach (var item in ranked)
                {
                    if (remainingCash <= 0.01m) break;

                    var station = item.Station;
                    if (station.SpotCost <= 0m) continue;

                    var spendPerSpot = station.SpotCost * realizedFactor;
                    if (spendPerSpot <= 0m) continue;

                    var capRemaining = maxSpotsPerStationPerMonth - spotsBought[station.Id];
                    if (capRemaining <= 0) continue;

                    // target spend based on weight (ensures q matters)
                    var targetSpend = currentBudget * item.Weight;
                    if (targetSpend <= 0m) continue;

                    var desiredSpots = (int)decimal.Floor(targetSpend / station.SpotCost);
                    if (desiredSpots <= 0) continue;

                    // affordability based on actual cash out (after cancellations)
                    var affordableSpots = (int)decimal.Floor(remainingCash / spendPerSpot);
                    if (affordableSpots <= 0) continue;

                    var spots = desiredSpots;
                    if (spots > affordableSpots) spots = affordableSpots;
                    if (spots > capRemaining) spots = capRemaining;
                    if (spots <= 0) continue;

                    var spend = spots * station.SpotCost * realizedFactor;
                    if (spend <= 0m) continue;
                    if (spend > remainingCash) continue;

                    // ORDERS are driven by OrdersPerDollar (station-specific) and spend
                    var orders = spend * station.OrdersPerDollar;

                    // FRONT-END SALES are orders * monthlyPrice (price MUST change totals)
                    var baseGross = (monthlyPrice > 0m) ? (orders * monthlyPrice) : 0m;

                    // MONTHLY upsell value (yearly price amortized over 12 months)
                    var monthlyUpsellValue = (yearlyPrice > 0m) ? (yearlyPrice / 12m) : 0m;

                    // Upsell income added AFTER front-end sales to create TOTAL SALES
                    // upsellOrders = orders * UpsellRate
                    var upsellGross = (monthlyUpsellValue > 0m && station.UpsellRate > 0m)
                        ? (orders * station.UpsellRate * monthlyUpsellValue)
                        : 0m;

                    totalMedia += spend;
                    totalGross += baseGross + upsellGross;

                    remainingCash -= spend;
                    spotsBought[station.Id] += spots;
                }

                // PASS 2: spend remaining cash by buying single spots on top-ranked stations (prevents flatlines)
                if (remainingCash > 0.01m)
                {
                    // Compute minimum spendPerSpot among ranked to stop when no purchase is possible.
                    var minSpendPerSpot = ranked
                        .Select(x => x.Station.SpotCost * realizedFactor)
                        .Where(v => v > 0m)
                        .DefaultIfEmpty(0m)
                        .Min();

                    while (minSpendPerSpot > 0m && remainingCash >= minSpendPerSpot)
                    {
                        var boughtAny = false;

                        foreach (var item in ranked)
                        {
                            if (remainingCash <= 0.01m) break;

                            var station = item.Station;
                            if (station.SpotCost <= 0m) continue;

                            var spendPerSpot = station.SpotCost * realizedFactor;
                            if (spendPerSpot <= 0m) continue;

                            var capRemaining = maxSpotsPerStationPerMonth - spotsBought[station.Id];
                            if (capRemaining <= 0) continue;

                            if (remainingCash < spendPerSpot) continue;

                            // buy 1 spot
                            var spend = station.SpotCost * realizedFactor;
                            if (spend <= 0m) continue;
                            if (spend > remainingCash) continue;

                            var orders = spend * station.OrdersPerDollar;

                            var baseGross = (monthlyPrice > 0m) ? (orders * monthlyPrice) : 0m;

                            var monthlyUpsellValue = (yearlyPrice > 0m) ? (yearlyPrice / 12m) : 0m;

                            var upsellGross = (monthlyUpsellValue > 0m && station.UpsellRate > 0m)
                                ? (orders * station.UpsellRate * monthlyUpsellValue)
                                : 0m;

                            totalMedia += spend;
                            totalGross += baseGross + upsellGross;

                            remainingCash -= spend;
                            spotsBought[station.Id] += 1;

                            boughtAny = true;

                            if (remainingCash < minSpendPerSpot) break;
                        }

                        if (!boughtAny) break;
                    }
                }

                currentBudget = startingBudget - totalMedia + totalGross;

                timeline.Add(new MonthlyAllocationSnapshot
                {
                    PeriodIndex = month,
                    StartingBudget = startingBudget,
                    TotalSpend = totalMedia,
                    TotalSales = totalGross,
                    EndingBudget = currentBudget,
                    ChannelAllocations = new List<MediaChannelAllocationResult>()
                });
            }

            return timeline;
        }

        private decimal GenerateTestCost()
        {
            return decimal.Round(20m + (decimal)_random.NextDouble() * 180m, 2);
        }

        private decimal GeneratePullRatio(char showType)
        {
            decimal min, max;
            switch (char.ToUpperInvariant(showType))
            {
                case 'A': min = 0m; max = 2m; break;
                case 'B': min = 4m; max = 12m; break;
                case 'C': min = 12m; max = 60m; break;
                case 'D': min = 60m; max = 100m; break;
                default: min = 12m; max = 60m; break;
            }
            return decimal.Round(min + (decimal)_random.NextDouble() * (max - min), 2);
        }

        private decimal GenerateUpsellRate()
        {
            return decimal.Round(0.20m + (decimal)_random.NextDouble() * 0.20m, 4);
        }

        private void GetPullRatioRange(char showType, out decimal min, out decimal max)
        {
            switch (char.ToUpperInvariant(showType))
            {
                case 'A': min = 0m; max = 2m; break;
                case 'B': min = 4m; max = 12m; break;
                case 'C': min = 12m; max = 60m; break;
                case 'D': min = 60m; max = 100m; break;
                default: min = 12m; max = 60m; break;
            }
        }

        private decimal Pow(decimal x, decimal p)
        {
            // Safe decimal power using double math (acceptable for simulation).
            if (x <= 0m) return 0m;
            var xd = (double)x;
            var pd = (double)p;
            var yd = Math.Pow(xd, pd);
            if (double.IsNaN(yd) || double.IsInfinity(yd)) return 0m;
            return (decimal)yd;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private class TvStation
        {
            public int Id { get; set; }
            public decimal SpotCost { get; set; }

            // Kept (legacy / informational)
            public decimal PullRatio { get; set; }

            // NEW: station-specific underlying response rate (orders per $1 of media)
            public decimal OrdersPerDollar { get; set; }

            public decimal UpsellRate { get; set; }
            public int FirstMonth { get; set; }
            public int LastActiveMonth { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
