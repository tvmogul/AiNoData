// Controllers/DroneController.cs
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AiNoData.Models.Drone;
using AiNoData.Services.Drone;

namespace AiNoData.Controllers
{
    public class DroneController : Controller
    {
        private readonly IDroneZ3DService _droneService;

        public DroneController(IDroneZ3DService droneService)
        {
            _droneService = droneService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = CreateDefaultViewModel();

            vm.Timeline = _droneService.SimulateHover(
                vm.InitialState,
                vm.Environment,
                vm.TimeSteps,
                vm.Dt);

            return View(vm); // Views/Drone/Index.cshtml
        }

        [HttpPost]
        public IActionResult Index(DroneHoverViewModel model)
        {
            if (model.TimeSteps <= 0)
            {
                model.TimeSteps = 120;
            }

            if (model.Dt <= 0m)
            {
                model.Dt = 0.05m;
            }

            // Clamp blast window to valid range.
            if (model.Environment.BlastStartStep < 0)
            {
                model.Environment.BlastStartStep = 0;
            }

            if (model.Environment.BlastEndStep < model.Environment.BlastStartStep)
            {
                model.Environment.BlastEndStep = model.Environment.BlastStartStep;
            }

            model.Timeline = _droneService.SimulateHover(
                model.InitialState,
                model.Environment,
                model.TimeSteps,
                model.Dt);

            return View(model);
        }

        private DroneHoverViewModel CreateDefaultViewModel()
        {
            var vm = new DroneHoverViewModel
            {
                InitialState = new DroneState
                {
                    // Small initial tilt (in radians if you like),
                    // but you can change this to degrees in the UI if you prefer.
                    Roll = 0.15m,   // ~8.6 degrees
                    Pitch = 0.05m,  // small tilt
                    Yaw = 0.0m,
                    RollRate = 0.0m,
                    PitchRate = 0.0m,
                    YawRate = 0.0m
                },
                Environment = new DroneEnvironmentParameters
                {
                    InertiaRoll = 1.0m,
                    InertiaPitch = 1.0m,
                    InertiaYaw = 1.0m,
                    StiffnessRoll = 2.5m,
                    StiffnessPitch = 2.5m,
                    StiffnessYaw = 1.0m,

                    BaselineDisturbanceRoll = 0.0m,
                    BaselineDisturbancePitch = 0.0m,
                    BaselineDisturbanceYaw = 0.0m,

                    BlastStartStep = 20,
                    BlastEndStep = 40,
                    BlastDisturbanceRoll = 0.6m,
                    BlastDisturbancePitch = 0.0m,
                    BlastDisturbanceYaw = 0.0m
                },
                TimeSteps = 120,
                Dt = 0.05m,
                Timeline = new List<DroneSimulationSnapshot>()
            };

            return vm;
        }
    }
}
