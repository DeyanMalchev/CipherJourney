using Microsoft.AspNetCore.Mvc;

namespace Experiments.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult About()
        {
            return View("About");
        }
    }
}
