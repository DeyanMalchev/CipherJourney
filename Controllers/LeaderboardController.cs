using Microsoft.AspNetCore.Mvc;

namespace CipherJourney.Controllers
{
    public class LeaderboardController : Controller
    {
        public IActionResult Leaderboard()
        {
            return View();
        }
    }
}
