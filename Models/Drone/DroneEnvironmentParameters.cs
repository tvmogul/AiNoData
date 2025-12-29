// Models/Drone/DroneEnvironmentParameters.cs
namespace AiNoData.Models.Drone
{
    /// <summary>
    /// Structural parameters for the drone dynamics:
    /// inertias, stiffness (how strongly it is pulled back to level),
    /// and a time-dependent wind/fan disturbance.
    /// </summary>
    public class DroneEnvironmentParameters
    {
        // Rotational inertias around each axis (simplified).
        public decimal InertiaRoll { get; set; } = 1.0m;
        public decimal InertiaPitch { get; set; } = 1.0m;
        public decimal InertiaYaw { get; set; } = 1.0m;

        // "Stiffness" terms pulling the drone back to level (q = 0).
        public decimal StiffnessRoll { get; set; } = 2.0m;
        public decimal StiffnessPitch { get; set; } = 2.0m;
        public decimal StiffnessYaw { get; set; } = 1.0m;

        // Baseline constant disturbances (e.g., slight bias).
        public decimal BaselineDisturbanceRoll { get; set; } = 0.0m;
        public decimal BaselineDisturbancePitch { get; set; } = 0.0m;
        public decimal BaselineDisturbanceYaw { get; set; } = 0.0m;

        // Fan/wind blast disturbance: extra torque applied only between
        // BlastStartStep and BlastEndStep (inclusive).
        public int BlastStartStep { get; set; } = 20;
        public int BlastEndStep { get; set; } = 40;

        public decimal BlastDisturbanceRoll { get; set; } = 0.5m;
        public decimal BlastDisturbancePitch { get; set; } = 0.0m;
        public decimal BlastDisturbanceYaw { get; set; } = 0.0m;
    }
}
