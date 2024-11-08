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

        public IActionResult Logout()
        {
            Response.Cookies.Delete("Authenticated");
            return View("Login");
        }

        public IActionResult LoginUser(LoginModel loginModel)
        {

            if (ModelState.IsValid)
            {
                User? user = DB_Queries.LoginUser(loginModel.Username, loginModel.Password, _context);
                if (user == null)
                {

                    ModelState.AddModelError(string.Empty, "Invalid Username or Password. Are you sure you are signed up?");
                    return View("Login", loginModel);

                }

                Response.Cookies.Append("Authenticated", user.Id.ToString());
                return View("LoginSuccess");
            }
            return View("Login", loginModel);
        }
    }
}
