using Microsoft.AspNetCore.Mvc;

namespace AiNoData.Controllers
{
    public class Cs2MarketController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Stub endpoint (returns placeholder results so the UI loop works)
        [HttpPost]
        public IActionResult Run([FromBody] Cs2MarketRunRequest req)
        {
            req ??= new Cs2MarketRunRequest();
            req.Items ??= new List<Cs2MarketItem>();

            // Guard rails
            var budget = req.Budget < 0 ? 0 : req.Budget;
            var feePct = req.FeePct < 0 ? 0 : req.FeePct;
            var maxPctPerItem = req.MaxPctPerItem <= 0 ? 100 : req.MaxPctPerItem;
            var minLiquidity = req.MinLiquidity < 0 ? 0 : req.MinLiquidity;
            var expectedUpsidePct = req.ExpectedUpsidePct < 0 ? 0 : req.ExpectedUpsidePct;

            // Total fee rate (example: 15% => 0.15)
            var feeRate = (double)(feePct / 100m);

            // If budget is 0, we still return scored results, but no allocations.
            var maxDollarsPerItem = budget * (maxPctPerItem / 100m);
            if (maxDollarsPerItem < 0) maxDollarsPerItem = 0;

            // Build scored list
            var scored = new List<(Cs2MarketItem item, decimal buy, decimal sell, decimal liq, decimal net, decimal profitPct, decimal score, bool sellEstimated)>();

            foreach (var it in req.Items)
            {
                var name = (it.Item ?? "").Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                var buy = it.BuyPrice;
                var sell = it.SellPrice;
                var liq = it.Liquidity;

                if (buy <= 0) continue;              // can't evaluate
                if (liq < minLiquidity) continue;    // enforce min liquidity constraint

                // If no sell price provided, we can't compute "profit" — treat as HOLD-only with low score.
                // You can later replace this with a "target sell" assumption or spread model.
                // UPDATED: If SellPrice is missing, we assume an expected gross upside of ExpectedUpsidePct (default 5%)
                // and compute fee-adjusted profit using that estimated sell price.
                decimal net;
                decimal profitPct;
                bool sellEstimated = false;
                if (sell > 0)
                {
                    // Fee-adjusted proceeds = sell * (1 - feeRate)
                    var netProceeds = sell * (decimal)(1.0 - feeRate);
                    net = netProceeds - buy;
                    profitPct = (net / buy) * 100m;
                }
                else
                {
                    sellEstimated = true;
                    var estSell = buy * (1m + (expectedUpsidePct / 100m));
                    var netProceeds = estSell * (decimal)(1.0 - feeRate);
                    net = netProceeds - buy;
                    profitPct = (net / buy) * 100m;
                }

                // Greedy score:
                //   primary = profitPct
                //   small boost for liquidity
                // NOTE: keep it deterministic and simple.
                var liqBoost = liq * 2m; // tweak: each 1.0 liquidity adds +2 score
                var score = profitPct + liqBoost;

                // If sell is missing, strongly downweight score so it won't get allocated.
                // UPDATED: Do NOT downweight; we now estimate SellPrice using ExpectedUpsidePct and allow allocation.
                if (sellEstimated) score -= 0.75m;

                scored.Add((it, buy, sell, liq, net, profitPct, score, sellEstimated));
            }

            // Sort by score descending (best first)
            scored = scored
                .OrderByDescending(x => x.score)
                .ThenByDescending(x => x.liq)
                .ToList();

            // Greedy allocate
            var remaining = budget;
            var allocations = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            if (budget > 0 && maxDollarsPerItem > 0)
            {
                foreach (var s in scored)
                {
                    if (remaining <= 0) break;

                    // Only allocate to items that have a SELL price and positive profit after fees
                    // UPDATED: Allocate even if SellPrice is missing (we estimated it), but still require positive profit after fees
                    if (s.profitPct <= 0) continue;

                    var name = (s.item.Item ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    // cap per item
                    var already = allocations.TryGetValue(name, out var a) ? a : 0m;
                    var capLeft = maxDollarsPerItem - already;
                    if (capLeft <= 0) continue;

                    var add = remaining < capLeft ? remaining : capLeft;

                    // Allocate
                    allocations[name] = already + add;
                    remaining -= add;
                }
            }

            // Build results table
            var results = new List<Cs2MarketResultRow>();

            // Include all scored items (even those not allocated) so the user sees ranking
            foreach (var s in scored)
            {
                var name = (s.item.Item ?? "").Trim();
                allocations.TryGetValue(name, out var allocDollars);

                var allocPct = (budget > 0) ? (allocDollars / budget) * 100m : 0m;

                // Action rules:
                // - BUY if allocated
                // - SELL if sell provided and profitPct is very negative (you can tune threshold)
                // - otherwise HOLD
                string action;
                if (allocDollars > 0)
                    action = "BUY";
                else if (s.sell > 0 && s.profitPct <= -5m)
                    action = "SELL";
                else
                    action = "HOLD";

                // NetAfterFees shown as the fee-adjusted net for 1 unit (buy->sell), not portfolio net
                // (Simple and understandable)
                var netAfterFees = s.net;

                var notes =
                    (s.sell > 0)
                        ? $"Profit(after fees): {s.profitPct:F2}% | Liquidity: {s.liq:F2} | Score: {s.score:F2}"
                        : $"SellPrice estimated (+{expectedUpsidePct:F2}% gross) | Profit(after fees): {s.profitPct:F2}% | Liquidity: {s.liq:F2} | Score: {s.score:F2}";

                results.Add(new Cs2MarketResultRow
                {
                    Item = name,
                    Action = action,
                    AllocationPct = decimal.Round(allocPct, 2),
                    NetAfterFees = decimal.Round(netAfterFees, 2),
                    Notes = notes
                });
            }

            // Also include any items that were filtered out (optional: show them as ignored)
            // If you want that, tell me and I’ll add an “IGNORED (low liquidity / missing buy)” row type.

            var allocatedTotal = allocations.Values.Sum();
            var usedPct = (budget > 0) ? (allocatedTotal / budget) * 100m : 0m;

            return Json(new
            {
                ok = true,
                message = $"Zero-Training AI™ greedy allocator complete. Allocated {allocatedTotal:F2} of {budget:F2} ({usedPct:F2}%).",
                summary = new
                {
                    budget,
                    feePct,
                    maxPctPerItem,
                    minLiquidity,
                    allocatedTotal,
                    remaining,
                    expectedUpsidePct
                },
                results
            });
        }
    }

    public class Cs2MarketRunRequest
    {
        public decimal Budget { get; set; }
        public decimal FeePct { get; set; } = 15;
        public decimal MaxPctPerItem { get; set; } = 20;
        public decimal MinLiquidity { get; set; } = 0;
        public decimal ExpectedUpsidePct { get; set; } = 5;

        public List<Cs2MarketItem> Items { get; set; } = new();
    }

    public class Cs2MarketItem
    {
        public string? Item { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; } // optional, can be 0
        public decimal Liquidity { get; set; } // optional, can be 0
    }

    public class Cs2MarketResultRow
    {
        public string? Item { get; set; }
        public string? Action { get; set; }       // BUY / HOLD / SELL
        public decimal AllocationPct { get; set; }
        public decimal NetAfterFees { get; set; }
        public string? Notes { get; set; }
    }
}