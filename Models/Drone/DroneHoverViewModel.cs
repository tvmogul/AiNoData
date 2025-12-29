// Models/Drone/DroneHoverViewModel.cs
using System.Collections.Generic;

namespace AiNoData.Models.Drone
{
    public class DroneHoverViewModel
    {
        public DroneState InitialState { get; set; } = new DroneState();

        public DroneEnvironmentParameters Environment { get; set; } = new DroneEnvironmentParameters();

        /// <summary>
        /// Number of Hamiltonian integration steps.
        /// </summary>
        public int TimeSteps { get; set; } = 120;

        /// <summary>
        /// Time step size (e.g., 0.05 seconds).
        /// </summary>
        public decimal Dt { get; set; } = 0.05m;

        /// <summary>
        /// Simulation timeline: roll/pitch/yaw and energy over time.
        /// </summary>
        public List<DroneSimulationSnapshot> Timeline { get; set; } = new List<DroneSimulationSnapshot>();
    }
}
