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
                    if (existingUser.Username == existingUser.Username)
                        throw new InvalidOperationException("Username already exists.");
                    if (existingUser.Email == existingUser.Email)
                        throw new InvalidOperationException("Email already exists.");
                }

                DB_Queries.AddUser(signUpModel, _context);

                User user = _context.Users.FirstOrDefault(u => u.Email == signUpModel.Email);
                DB_Queries.VerifyUserEmail(user, _emailService, _context);

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
                    var userUnverified = _context.UsersUnverified.FirstOrDefault(u => u.UserID == user.Id);

                    if (userUnverified.VerificationToken.Equals(emailVerificationModel.InputToken))
                    {
                        user.IsEmailVerified = true;
                        _context.UsersUnverified.Remove(userUnverified);
                        _context.SaveChanges();

                        var userPoints = new UserPoints
                        {
                            UserId = user.Id,
                            DailyScore = 0,
                            WeeklyScore = 0,
                            DailiesDone = 0,
                            WeekliesDone = 0
                        };

                        _context.UserPoints.Add(userPoints);
                        _context.SaveChanges();

                        var leaderboard = new Leaderboard
                        {
                            UserId = user.Id,
                            Username = user.Username,
                            TotalPoints = userPoints.DailyScore + userPoints.WeeklyScore
                        };

                        _context.Leaderboard.Add(leaderboard);
                        _context.SaveChanges();

                        Cookies.CreateCookieAccount(user, userPoints, Response);

                        return View("..\\Email\\VerificationSuccess"); // Redirect to a success page
                    }
                }
            }
            return View("..\\Email\\EmailVerification", emailVerificationModel);
        }

    }
}
