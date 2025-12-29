// Models/Drone/DroneSimulationSnapshot.cs
namespace AiNoData.Models.Drone
{
    /// <summary>
    /// One discrete time sample of the Hamiltonian hover + wind demo.
    /// </summary>
    public class DroneSimulationSnapshot
    {
        public int StepIndex { get; set; }

        public DroneState State { get; set; } = new DroneState();

        /// <summary>
        /// The Hamiltonian F(q, p) = kinetic + potential at this time step.
        /// </summary>
        public decimal Energy { get; set; }

        /// <summary>
        /// True if this step is inside the wind/fan blast window.
        /// This is purely for visualization in the UI.
        /// </summary>
        public bool IsUnderWindBlast { get; set; }
    }
}
