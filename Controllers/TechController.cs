using Microsoft.AspNetCore.Mvc;

namespace AiNoData.Controllers
{
    public class TechController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
