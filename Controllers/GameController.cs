using Microsoft.AspNetCore.Mvc;

namespace CipherJourney.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Play()
        {
            return View();
        }
    }
}
