using CipherJourney.Services;
using CipherJourney.Models;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;

namespace CipherJourney.Controllers
{
    public class SignUpController : Controller
    {
        public IActionResult SignUp()
        {
            return View();
        }


        private readonly CipherJourneyDBContext _context;
        private readonly Services.IEmailService _emailService;

        public SignUpController(CipherJourneyDBContext context, Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        public IActionResult SignUpUser(SignUpModel signUpModel)
        {
            if (ModelState.IsValid)
            {
                var existingUser = DB_Queries.GetExistingUser(signUpModel, _context);
                if (existingUser != null)
                {
                    if (existingUser.Username == signUpModel.Username)
                    {
                        ModelState.AddModelError(string.Empty, "Username already exists.");
                        return View("SignUp", signUpModel);
                    }
                    else if (existingUser.Email == signUpModel.Email)
                    {
                        ModelState.AddModelError(string.Empty, "Email already exists.");
                        return View("SignUp", signUpModel);
                    }
                }

                DB_Queries.AddUser(signUpModel, _context);

                User user = DB_Queries.GetExistingUser(signUpModel, _context);
                DB_Queries.SendEmailVerification(user, _emailService, _context);

                var emailVerificationModel = new EmailVerificationModel
                {
                    Email = user.Email
                };

                return View("../Email/EmailVerification", emailVerificationModel);
            }

            // If ModelState is not valid, return the SignUp view with errors
            return View("SignUp", signUpModel);
        }

        public IActionResult VerifyCode(EmailVerificationModel emailVerificationModel)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == emailVerificationModel.Email);
                if (user != null)
                {
                    var userVerificationTokens = _context.UserVerificationTokens.FirstOrDefault(u => u.UserID == user.Id);

                    if (userVerificationTokens.VerificationToken.Equals(emailVerificationModel.InputToken))
                    {
                        var userPoints = DB_Queries.ConfigureTablesAfterVerification(user, userVerificationTokens, _context);

                        Cookies.CreateCookieAccount(user, userPoints, Response);

                        return View("..\\Email\\VerificationSuccess"); // Redirect to a success page
                    }
                }
            }
            return View("..\\Email\\EmailVerification", emailVerificationModel);
        }

    }
}
