using CipherJourney.Models;
using Newtonsoft.Json;
using System.Text.Json;

namespace CipherJourney.Services
{
    public class Cookies
    {
        public static void CreateCookieAccount(User user, UserPoints userPoints , HttpResponse response)
        {

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Prevents JavaScript access for security
                Secure = true, // Enforces HTTPS
                SameSite = SameSiteMode.Strict
            };

            var cookieData = new Dictionary<string, string>
            {
                { "Authenticated", "true"},
                { "UserId", user.Id.ToString() },
                { "Username", user.Username },
                { "Email", user.Email },
                { "DailyPoints", userPoints.DailyScore.ToString()},
                { "WeeklyPoints", userPoints.WeeklyScore.ToString()},
                { "Daily", userPoints.DailiesDone.ToString()},
                { "Weekly", userPoints.WeekliesDone.ToString()},
            };

            string serializedData = JsonConvert.SerializeObject(cookieData);

            response.Cookies.Append("CipherJourney", serializedData, cookieOptions);

            Console.WriteLine(serializedData);
        }

        public static void DailyModeCookie(string cipher, string sentence, HttpResponse response)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.Date.AddDays(1), // Expires at midnight UTC
                HttpOnly = true, // Prevents JavaScript access for security
                Secure = true, // Enforces HTTPS
                SameSite = SameSiteMode.Strict
            };

            var cookieData = new Dictionary<string, string>
            {
                { "Cipher", cipher},
                { "Sentence", sentence}
            };


            string serializedData = JsonConvert.SerializeObject(cookieData);

            response.Cookies.Append("DailyMode", serializedData, cookieOptions);

            Console.WriteLine(serializedData);
        }
    }
}
