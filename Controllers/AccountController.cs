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

        public AccountController(CipherJourneyDBContext context)
        {
            _context = context;
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
            var userId = int.Parse(data["UserId"]);

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
