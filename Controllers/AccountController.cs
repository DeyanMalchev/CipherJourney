using CipherJourney.Models;
using CipherJourney.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CipherJourney.Controllers
{
    public class AccountController : Controller
    {


        private readonly CipherJourneyDBContext _context;

        public AccountController(CipherJourneyDBContext context)
        {
            _context = context;
        }

        public IActionResult AccountDetails()
        {
            return View("Account", GetInfo());
        }

        public IActionResult GetInfo() 
        {
            if (Request.Cookies.TryGetValue("CipherJourney", out string userIdString) && int.TryParse(userIdString, out int userId))
            {
                // Query the user based on the retrieved ID
                User user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    // Return user info or a view with the user's details
                    return Ok(new
                    {
                        user.Username,
                        user.Email,
                        user.IsEmailVerified,

                    });
                }
            }
            return null;
        }
    }
}
