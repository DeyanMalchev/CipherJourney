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
                DB_Queries.CheckIfUserExists(signUpModel, _context);

                var verificationToken = DB_Queries.GenerateVerificationToken();
                DB_Queries.AddUser(signUpModel, _context, verificationToken);
                _emailService.SendEmailAsync(signUpModel.Email, verificationToken);

                var emailVerificationModel = new EmailVerificationModel
                {
                    Email = signUpModel.Email
                };

                return View("EmailVerification", emailVerificationModel);
            }

            // If ModelState is not valid, return the SignUp view with errors
            return View("SignUp", signUpModel);
        }

        public IActionResult VerifyCode(EmailVerificationModel emailVerificationModel)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine("model is valid");

                var user = _context.Users.FirstOrDefault(u => u.Email == emailVerificationModel.Email);
                if (user != null)
                {
                    Console.WriteLine("user found");

                    if (user.VerificationToken.Equals(emailVerificationModel.InputToken))
                    {
                        Console.WriteLine("verification token matches");

                        user.IsEmailVerified = true;
                        _context.SaveChanges();
                        return View("SignUpSuccess"); // Redirect to a success page
                    }
                }
                else
                {
                    Console.WriteLine("user not found");
                }
            }
            else
            {
                Console.WriteLine("model is invalid");
            }

            return View("..\\Email\\EmailVerification", emailVerificationModel);
        }

    }
}
