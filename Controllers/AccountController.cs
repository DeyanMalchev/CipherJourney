using CipherJourney.Models;
using CipherJourney.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CipherJourney.Controllers
{
    public class AccountController : Controller
    {


        private readonly CipherJourneyDBContext _context;
        private readonly Services.IEmailService _emailService;

        public AccountController(CipherJourneyDBContext context, Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Account()
        {
            if (Request.Cookies.ContainsKey("CipherJourney"))
            {
                return View("Account", AccountDetails());
            }
            return RedirectToAction("Login", "Login");
        }

        public AccountModel AccountDetails()
        {

            var cookieValue = Request.Cookies["CipherJourney"];
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(cookieValue);

            var model = new AccountModel
            {
                Username = values["Username"],
                Email = values["Email"],
                RiddlesSolved = int.Parse(values["RiddlesSolved"]),
                Score = int.Parse(values["Score"])
            };

            return model;
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var cookie = Request.Cookies["CipherJourney"];
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(cookie);

            SignUpModel signUpModel = new SignUpModel
            {
                Email = data["Email"],
                Username = data["Username"],
            };

            var user = DB_Queries.GetExistingUser(signUpModel, _context);

            var currentHashed = DB_Queries.HashPassword(model.CurrentPassword, user.Salt);
            if (user.Password != currentHashed)
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return View(model);
            }

            // Hash and set new password
            user.Password = DB_Queries.HashPassword(model.NewPassword, user.Salt);
            _context.SaveChanges();

            TempData["PassChangeSuccess"] = "Your password was changed successfully!";
            return RedirectToAction("Account");
        }

        public IActionResult NewPassword(ForgotPasswordModel forgotPasswordModel) 
        {

            if (!ModelState.IsValid)
            {
                return View("NewPassword", forgotPasswordModel);
            }

            var cookie = Request.Cookies["CipherJourney"];
            if (cookie != null) { 
            
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(cookie);
                var userId = int.Parse(data["UserId"]);
                SignUpModel signUpModel = new SignUpModel
                {
                    Email = data["Email"],
                    Username = data["Username"],
                };
                var userLogged = DB_Queries.GetExistingUser(signUpModel, _context);
                userLogged.Password = DB_Queries.HashPassword(forgotPasswordModel.ConfirmPassword, userLogged.Salt);
                _context.SaveChanges();
                TempData["PassChangeSuccess"] = "Your password was changed successfully!";
                return RedirectToAction("Account");
            }

            User user = _context.Users.FirstOrDefault(u => u.Email == forgotPasswordModel.Email);
            user.Password = DB_Queries.HashPassword(forgotPasswordModel.ConfirmPassword, user.Salt);
            _context.SaveChanges();

            UserVerificationTokens userVerificationToken = _context.UserVerificationTokens.FirstOrDefault(u => u.UserID == user.Id);
            _context.UserVerificationTokens.Remove(userVerificationToken);

            TempData["PassChangeSuccess"] = "Your password was changed successfully!";

            return RedirectToAction("Login", "Login");
        }

        public IActionResult ForgotPassword() 
        {
            var userCookie = Request.Cookies["CipherJourney"];
            if (userCookie == null)
            {
                return View("ForgotPassword");
            }

            var userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(userCookie);
            int userID = int.Parse(userData["UserId"]);

            User user = _context.Users.FirstOrDefault(u => u.Id == userID);

            var uvt = DB_Queries.GetUserVerificationTokens(user, _context);

            if (uvt == null) 
            { 
                DB_Queries.AddUserVerificationTokensPassword(user, _context);
            }

            DB_Queries.SendEmailVerification(user, _emailService, _context);

            EmailVerificationModel emailVerificationModel = new EmailVerificationModel() 
            { 
                Email = user.Email
            };

            return View("EmailVerificationPassword", emailVerificationModel);
        }

        public IActionResult VerifyCode(EmailVerificationModel emailVerificationModel) {

            if(!ModelState.IsValid) return View("EmailVerificationPassword", emailVerificationModel);

            var user = _context.Users.FirstOrDefault(u => u.Email == emailVerificationModel.Email);
            var userVerificationTokens = _context.UserVerificationTokens.FirstOrDefault(u => u.UserID == user.Id);

            if (userVerificationTokens.VerificationToken.Equals(emailVerificationModel.InputToken))
            {
                ForgotPasswordModel forgotPasswordModel = new ForgotPasswordModel()
                {
                    Email = emailVerificationModel.Email
                };

                return View("NewPassword", forgotPasswordModel);

            }

            ModelState.AddModelError(string.Empty, "Incorrect token.");
            return View("EmailVerificationPassword", emailVerificationModel);
        }

        public IActionResult SendVerificationCode(EmailVerificationModel emailVerificationModel) 
        {

            User user = _context.Users.FirstOrDefault(u => u.Email == emailVerificationModel.Email || u.Username == emailVerificationModel.Email);
            
            UserVerificationTokens userVerificationToken = _context.UserVerificationTokens.FirstOrDefault(uvt => uvt.UserID == user.Id);
            if (userVerificationToken == null) 
            {    
                DB_Queries.AddUserVerificationTokensPassword(user, _context);
            }

            DB_Queries.SendEmailVerification(user, _emailService, _context);

            return View("EmailVerificationPassword", emailVerificationModel);
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("CipherJourney");
            Response.Cookies.Delete("DailyMode");
            return RedirectToAction("Login", "Login");
        }

        public IActionResult DeleteAccount(DeleteAccountModel model) 
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cookie = Request.Cookies["CipherJourney"];
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(cookie);
            var userId = int.Parse(data["UserId"]);

            SignUpModel signUpModel = new SignUpModel
            {
                Email = data["Email"],
                Username = data["Username"],
            };

            var user = DB_Queries.GetExistingUser(signUpModel, _context);

            var currentHashed = DB_Queries.HashPassword(model.Password, user.Salt);
            if (user.Password != currentHashed)
            {
                ModelState.AddModelError("", "Incorrect Password!.");
                return View(model);
            }

            DB_Queries.DeleteAccount(user, _context);
            Response.Cookies.Delete("CipherJourney");
            Response.Cookies.Delete("DailyMode");
            return View("Goodbye"); 
        }

    }
}
