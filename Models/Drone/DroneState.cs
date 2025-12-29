// Models/Drone/DroneState.cs
namespace AiNoData.Models.Drone
{
    /// <summary>
    /// Reduced drone attitude state for the Hamiltonian hover controller.
    /// q = (roll, pitch, yaw) in radians (or small-angle degrees if you prefer).
    /// </summary>
    public class DroneState
    {
        public decimal Roll { get; set; }      // q0
        public decimal Pitch { get; set; }     // q1
        public decimal Yaw { get; set; }       // q2

        public decimal RollRate { get; set; }  // d(Roll)/dt (for display only)
        public decimal PitchRate { get; set; } // d(Pitch)/dt
        public decimal YawRate { get; set; }   // d(Yaw)/dt
    }
}
