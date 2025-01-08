using CipherJourney.Services;
using CipherJourney.Models;
using Microsoft.AspNetCore.Mvc;

namespace CipherJourney.Controllers
{
    public class LoginController : Controller
    {

        private readonly CipherJourneyDBContext _context;
        public LoginController(CipherJourneyDBContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View("Login");
        }

        public IActionResult LoginSuccess() 
        {
            return View("LoginSuccess");
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("CipherJourney");
            return RedirectToAction("Login");
        }

        public IActionResult LoginUser(LoginModel loginModel)
        {

            if (ModelState.IsValid)
            {
                User? user = DB_Queries.Login(loginModel, _context);
                if (user == null)
                {

                    ModelState.AddModelError(string.Empty, "Invalid Username or Password. Are you sure you are signed up?");
                    return View("Login", loginModel);

                }

                UserPoints userPoints = DB_Queries.GetUserPoints(user, _context);

                Cookies.CreateCookie(user, userPoints, Response);
                return RedirectToAction("LoginSuccess");
            }
            return View("Login", loginModel);
        }
    }
}
