using CipherJourney.Services;
using CipherJourney.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace CipherJourney.Controllers
{
    public class LoginController : Controller
    {

        private readonly CipherJourneyDBContext _context;
        private readonly Services.IEmailService _emailService;


        public LoginController(CipherJourneyDBContext context, Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View("Login");
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

                if (user.IsEmailVerified == false) 
                {

                    DB_Queries.SendEmailVerification(user, _emailService, _context);

                    var emailVerificationModel = new EmailVerificationModel
                    {
                        Email = user.Email
                    };

                    return View("../Email/EmailVerification", emailVerificationModel);
                }

                UserPoints userPoints = DB_Queries.GetUserPoints(user, _context);
                Cookies.CreateCookieAccount(user, userPoints, Response);

                return RedirectToAction("Home", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid Username or Password. Are you sure you are signed up?");
            return View("Login", loginModel);
        }
    }
}
