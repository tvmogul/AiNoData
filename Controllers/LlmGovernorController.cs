using Microsoft.AspNetCore.Mvc;

namespace AiNoData.Controllers
{
    public class LlmGovernorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
