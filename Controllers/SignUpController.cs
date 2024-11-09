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
                _emailService.SendEmailAsync(signUpModel.Email);
                return View("..\\Email\\EmailVerification", signUpModel.Email);
            }

            // If ModelState is not valid, return the SignUp view with errors
            return View("SignUp", signUpModel);
        }
    }
}
