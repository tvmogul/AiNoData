using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AiNoData.Services.Budget;
using AiNoData.Models.Budget;

namespace AiNoData.Controllers
{
    public class BudgetController : Controller
    {
        private readonly IZ3DAllocatorService _allocator;

        public BudgetController(IZ3DAllocatorService allocator)
        {
            _allocator = allocator;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = CreateDefaultViewModel();
            return View(vm); // Views/Budget/Index.cshtml
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Index(BudgetAllocatorViewModel model)
        {
            if (model.TotalBudget <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please provide a total budget greater than zero.");
                model = CreateDefaultViewModel();
                return View(model);
            }

            if (model.MonthsToSimulate <= 0)
            {
                model.MonthsToSimulate = 12;
            }

            if (model.NewStationsPerMonth <= 0)
            {
                model.NewStationsPerMonth = 40;
            }

            if (model.CancellationRate < 0m)
            {
                model.CancellationRate = 0m;
            }

            if (model.CancellationRate > 1m)
            {
                model.CancellationRate = 1m;
            }

            // Ensure reasonable defaults for prices if user leaves them blank or zero.
            if (model.MonthlyPrice <= 0m)
            {
                model.MonthlyPrice = 59.95m;
            }

            if (model.YearlyPrice <= 0m)
            {
                model.YearlyPrice = 499.00m;
            }

            // Determine show type character from user input (A, B, C, or D).
            char showTypeChar = 'C';
            if (!string.IsNullOrWhiteSpace(model.ShowType))
            {
                showTypeChar = char.ToUpperInvariant(model.ShowType[0]);
            }

            // Preserve the original starting budget so the "Starting Budget" field and labels
            // always reflect the true initial budget, not the final budget after simulation.
            var originalStartingBudget = model.TotalBudget;

            // Simulate 12 months of TV station buys with constant station PR,
            // auction-based test costs, cancellation rate, and profit rollover.
            var timeline = _allocator.RunSimulation(
                originalStartingBudget,
                model.MonthsToSimulate,
                model.NewStationsPerMonth,
                model.CancellationRate,
                showTypeChar,
                model.MonthlyPrice,
                model.YearlyPrice,
                model.TimeSteps);

            model.MonthlyTimeline = timeline;

            // Not used for the monthly TV-station summary, but kept for compatibility.
            model.AllocationResults = new List<MediaChannelAllocationResult>();

            return View(model);
        }

        private BudgetAllocatorViewModel CreateDefaultViewModel()
        {
            return new BudgetAllocatorViewModel
            {
                TotalBudget = 1_000m,
                Channels = new List<MediaChannelInput>(),
                MonthlyPrice = 59.95m,
                YearlyPrice = 499.00m
            };
        }
    }
}

