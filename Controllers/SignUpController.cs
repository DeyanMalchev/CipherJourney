using Experiments.Models;
using Experiments.Services;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;

namespace Experiments.Controllers
{
    public class SignUpController : Controller
    {
        public IActionResult SignUp()
        {
            return View();
        }


        private readonly SignUpContext _context;
        private readonly Services.IEmailService _emailService;

        public SignUpController(SignUpContext context, Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        public IActionResult SignUpUser(SignUpModel signUpModel)
        {
            if (ModelState.IsValid)
            {

                return View("../Email/EmailVerification");/*
                DB_Querries.AddUser(signUpModel,_context);*/
            }

            // If ModelState is not valid, return the SignUp view with errors
            return View("SignUp", signUpModel);
        }
    }
}
