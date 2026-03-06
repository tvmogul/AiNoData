using Microsoft.AspNetCore.Mvc;

namespace AiNoData.Controllers
{
    public class SpaceController : Controller
    {
        // View: /Space
        public IActionResult Index()
        {
            return View();
        }

        // Data endpoint: /Space/Data
        // Returns a "worldline" in 3D where Z is time.
        // Each point includes a scalar "s" used for tube radius + color (constraint pressure / risk).
        [HttpGet]
        public IActionResult Data(int n = 320, double dt = 0.06)
        {
            if (n < 40) n = 40;
            if (n > 2000) n = 2000;
            if (dt <= 0) dt = 0.05;

            // --- Synthetic deterministic dynamics (demo) ---
            // Interpret this as a damped system settling into a constrained minimum.
            // q ~ (x,y) is "decision state", v ~ "decision momentum".
            // s ~ constraint pressure / violation energy (we'll keep it >= 0).
            double x = -2.2, y = 1.6;
            double vx = 0.0, vy = 0.0;

            // moving target / "value pull" to make it look alive
            double tx = 1.2, ty = -0.8;

            // damping & stiffness
            double gamma = 0.32;   // damping
            double k = 1.35;       // attraction to target
            double swirl = 0.55;   // adds a nice "worldline" twist
            double constraintK = 1.8; // constraint pressure stiffness

            // A simple constraint: keep radius <= R (soft constraint)
            double R = 2.2;

            var points = new object[n];

            for (int i = 0; i < n; i++)
            {
                double t = i * dt;

                // Slowly move the target over time (represents changing environment)
                tx = 1.2 * System.Math.Cos(0.23 * t) + 0.35 * System.Math.Sin(0.11 * t);
                ty = -0.9 * System.Math.Sin(0.19 * t) + 0.25 * System.Math.Cos(0.13 * t);

                // "Forces" toward target (least-action-ish settling)
                double ax = k * (tx - x);
                double ay = k * (ty - y);

                // add a gentle swirl so the tube reads like a worldline
                ax += -swirl * vy;
                ay += swirl * vx;

                // Soft constraint pressure if outside radius R
                double r = System.Math.Sqrt(x * x + y * y);
                double viol = System.Math.Max(0.0, r - R); // violation amount

                // Push back inward when violating
                if (viol > 0.0 && r > 1e-9)
                {
                    double nx = x / r;
                    double ny = y / r;
                    ax += -constraintK * viol * nx;
                    ay += -constraintK * viol * ny;
                }

                // Damping
                ax += -gamma * vx;
                ay += -gamma * vy;

                // Integrate (semi-implicit Euler)
                vx += ax * dt;
                vy += ay * dt;
                x += vx * dt;
                y += vy * dt;

                // Scalar "s": constraint pressure / risk / penalty energy
                // Make it always positive and visually meaningful
                double s = viol * viol + 0.05 * (vx * vx + vy * vy);

                // Worldline coordinate: Z is time (scaled for aesthetics)
                double z = t * 0.7;

                points[i] = new
                {
                    x,
                    y,
                    z,
                    s
                };
            }

            return Json(new { n, dt, points });
        }
    }
}