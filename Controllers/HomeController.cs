using CipherJourney.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CipherJourney.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Home()
        {
            if (Request.Cookies.TryGetValue("CipherJourney", out string userIDString))
            {
                return View("HomePage");
            }
            return View("../Login/Login");
        }

        public IActionResult Privacy() {
            return View("Privacy");
        }
        public IActionResult Contacts()
        {
            return View("Contacts");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
