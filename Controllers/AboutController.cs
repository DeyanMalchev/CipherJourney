using Microsoft.AspNetCore.Mvc;

namespace CipherJourney.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult About()
        {
            return View("About");
        }
    }
}
