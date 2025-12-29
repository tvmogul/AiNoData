using Microsoft.AspNetCore.Mvc;

namespace AiNoData.Controllers
{
    public class RobotController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
