// Services/Drone/DroneZ3DService.cs
using System;
using System.Collections.Generic;
using AiNoData.Models.Drone;

namespace AiNoData.Services.Drone
{
    /// <summary>
    /// Z3D-based drone hover controller with a wind/fan disturbance:
    ///
    /// Implements discrete energy-based dynamics for
    ///   F(q, p) = 1/2 Σ (p_i^2 / I_i) + 1/2 Σ (k_i q_i^2)
    ///
    /// where:
    ///   q = (roll, pitch, yaw)
    ///   p = conjugate momenta
    ///
    /// The evolution follows canonical relations
    ///   dq_i/dt = ∂F/∂p_i = p_i / I_i
    ///   dp_i/dt = -∂F/∂q_i = -k_i q_i
    ///
    /// A time-dependent external torque term models a wind/fan blast
    /// between BlastStartStep and BlastEndStep, then the system naturally
    /// returns to hover as the dynamics re-level the drone.
    /// </summary>
    public class DroneZ3DService : IDroneZ3DService
    {
        public List<DroneSimulationSnapshot> SimulateHover(
            DroneState initialState,
            DroneEnvironmentParameters env,
            int timeSteps,
            decimal dt)
        {
            var snapshots = new List<DroneSimulationSnapshot>();

            if (timeSteps <= 0 || dt <= 0m)
            {
                return snapshots;
            }

            // Generalized coordinates q = (roll, pitch, yaw)
            decimal qRoll = initialState.Roll;
            decimal qPitch = initialState.Pitch;
            decimal qYaw = initialState.Yaw;

            // Conjugate momenta p (start from something proportional to rates).
            // For a rigid body, p ≈ I * angularRate, so we can use that mapping.
            decimal pRoll = env.InertiaRoll * initialState.RollRate;
            decimal pPitch = env.InertiaPitch * initialState.PitchRate;
            decimal pYaw = env.InertiaYaw * initialState.YawRate;

            // Convenience locals.
            decimal Iroll = env.InertiaRoll;
            decimal Ipitch = env.InertiaPitch;
            decimal Iyaw = env.InertiaYaw;

            decimal kRoll = env.StiffnessRoll;
            decimal kPitch = env.StiffnessPitch;
            decimal kYaw = env.StiffnessYaw;

            // Baseline disturbances.
            decimal baseRollDisturbance = env.BaselineDisturbanceRoll;
            decimal basePitchDisturbance = env.BaselineDisturbancePitch;
            decimal baseYawDisturbance = env.BaselineDisturbanceYaw;

            // Blast disturbance amplitudes (extra torque during fan blast).
            decimal blastRollDisturbance = env.BlastDisturbanceRoll;
            decimal blastPitchDisturbance = env.BlastDisturbancePitch;
            decimal blastYawDisturbance = env.BlastDisturbanceYaw;

            int blastStart = Math.Max(0, env.BlastStartStep);
            int blastEnd = Math.Max(blastStart, env.BlastEndStep);

            for (int step = 0; step <= timeSteps; step++)
            {
                bool isBlast =
                    step >= blastStart &&
                    step <= blastEnd;

                // Compute current angular rates from dq/dt = ∂F/∂p = p / I.
                decimal rollRate = Iroll != 0m ? pRoll / Iroll : 0m;
                decimal pitchRate = Ipitch != 0m ? pPitch / Ipitch : 0m;
                decimal yawRate = Iyaw != 0m ? pYaw / Iyaw : 0m;

                // Energy function: kinetic + potential
                decimal kinetic =
                    0.5m * (Iroll != 0m ? (pRoll * pRoll) / Iroll : 0m) +
                    0.5m * (Ipitch != 0m ? (pPitch * pPitch) / Ipitch : 0m) +
                    0.5m * (Iyaw != 0m ? (pYaw * pYaw) / Iyaw : 0m);

                decimal potential =
                    0.5m * (kRoll * qRoll * qRoll) +
                    0.5m * (kPitch * qPitch * qPitch) +
                    0.5m * (kYaw * qYaw * qYaw);

                decimal energy = kinetic + potential;

                // Record snapshot.
                snapshots.Add(new DroneSimulationSnapshot
                {
                    StepIndex = step,
                    State = new DroneState
                    {
                        Roll = qRoll,
                        Pitch = qPitch,
                        Yaw = qYaw,
                        RollRate = rollRate,
                        PitchRate = pitchRate,
                        YawRate = yawRate
                    },
                    Energy = energy,
                    IsUnderWindBlast = isBlast
                });

                if (step == timeSteps)
                {
                    break;
                }

                // --- Discrete update (Euler integration) ---

                // dq/dt = ∂F/∂p = p / I
                decimal dqRoll = rollRate;
                decimal dqPitch = pitchRate;
                decimal dqYaw = yawRate;

                // External torque = baseline + optional blast.
                decimal extraRoll = isBlast ? blastRollDisturbance : 0m;
                decimal extraPitch = isBlast ? blastPitchDisturbance : 0m;
                decimal extraYaw = isBlast ? blastYawDisturbance : 0m;

                decimal torqueRoll = baseRollDisturbance + extraRoll;
                decimal torquePitch = basePitchDisturbance + extraPitch;
                decimal torqueYaw = baseYawDisturbance + extraYaw;

                // dp/dt = -∂F/∂q + externalTorque
                //       = -k * q   + torque
                decimal dpRoll = -(kRoll * qRoll) + torqueRoll;
                decimal dpPitch = -(kPitch * qPitch) + torquePitch;
                decimal dpYaw = -(kYaw * qYaw) + torqueYaw;

                // Step forward in time.
                qRoll += dt * dqRoll;
                qPitch += dt * dqPitch;
                qYaw += dt * dqYaw;

                pRoll += dt * dpRoll;
                pPitch += dt * dpPitch;
                pYaw += dt * dpYaw;
            }

            return snapshots;
        }
    }
}
