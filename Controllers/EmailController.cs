using Microsoft.AspNetCore.Mvc;

namespace Experiments.Controllers
{
    public class EmailController : Controller
    {
        public IActionResult EmailVerification()
        {
            return View();
        }
    }
}
