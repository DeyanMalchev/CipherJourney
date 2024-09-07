using Experiments.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Experiments.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult HomePage()
        {
            if (Request.Cookies.TryGetValue("Authenticated", out string userIDString))
            {
                return View("HomePage");
            }
            return View("../Login/Login");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
