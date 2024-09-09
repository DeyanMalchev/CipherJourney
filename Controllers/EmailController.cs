using Microsoft.AspNetCore.Mvc;

namespace CipherJourney.Controllers
{
    public class EmailController : Controller
    {
        public IActionResult EmailVerification()
        {
            return View();
        }
    }
}
