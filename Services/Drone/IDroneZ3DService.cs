// Services/Drone/IDroneZ3DService.cs
using System.Collections.Generic;
using AiNoData.Models.Drone;

namespace AiNoData.Services.Drone
{
    public interface IDroneZ3DService
    {
        /// <summary>
        /// Simulate a hover-stabilization scenario using an energy-based attitude model
        /// over a reduced state (roll, pitch, yaw).
        /// </summary>
        /// <param name="initialState">Initial roll/pitch/yaw and rates.</param>
        /// <param name="env">Inertias, stiffness, and disturbances.</param>
        /// <param name="timeSteps">Number of discrete integration steps.</param>
        /// <param name="dt">Time step size.</param>
        /// <returns>Sequence of snapshots showing attitude and energy.</returns>
        List<DroneSimulationSnapshot> SimulateHover(
            DroneState initialState,
            DroneEnvironmentParameters env,
            int timeSteps,
            decimal dt);
    }
}
