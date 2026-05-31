using Microsoft.AspNetCore.Mvc;

namespace AiNetProfit.Controllers
{
    public class Z3DDecisionSpaceModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DecisionSpaceName { get; set; } = "Media Buying Optimizer";
        public string TemplateKey { get; set; } = "media";
        public List<Z3DDimensionModel> Dimensions { get; set; } = new();
        public List<string> Actions { get; set; } = new();
        public int Version { get; set; } = 1;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public List<string> AuditLog { get; set; } = new();
    }

    public class Z3DDimensionModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Units { get; set; } = string.Empty;
        public string Objective { get; set; } = "Minimize";
        public double Weight { get; set; } = 50;
        public double CurrentValue { get; set; } = 0.5;
        public double SafeMin { get; set; } = 0;
        public double SafeMax { get; set; } = 1;
        public double Warning { get; set; } = 0.6;
        public double Critical { get; set; } = 0.8;
        public string Curve { get; set; } = "Linear";
        public double Uncertainty { get; set; } = 0.1;
        public bool HardConstraint { get; set; } = false;
        public string Relationship { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public bool Validated { get; set; } = true;
    }

    public class DecisionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Decision()
        {
            return View();
        }

        [HttpPost("save")]
        public IActionResult Save([FromBody] Z3DDecisionSpaceModel model)
        {
            if (model == null) return BadRequest();

            model.LastModified = DateTime.UtcNow;
            model.AuditLog.Add($"Configuration saved at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

            // TODO: persist to DB or Z3DDecisionService here

            return Json(new { success = true, modelId = model.Id, version = model.Version });
        }
    }
}
