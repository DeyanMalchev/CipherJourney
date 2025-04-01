using CipherJourney.Models;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using TimeZoneConverter;

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

            DateTime utcNow = DateTime.UtcNow;

            // Get the next 00:00 London time
            TimeZoneInfo londonTimeZone = TZConvert.GetTimeZoneInfo("Europe/London");
            DateTime londonNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, londonTimeZone);

            // Calculate next midnight
            DateTime nextMidnight = londonNow.Date.AddDays(1); // Moves to next day's 00:00

            // Get the expiration time in UTC
            DateTime expirationUtc = TimeZoneInfo.ConvertTimeToUtc(nextMidnight, londonTimeZone);

            // Calculate expiration time span
            TimeSpan timeUntilMidnight = expirationUtc - utcNow;

            // Set the cookie with the calculated expiration time
            context.Response.Cookies.Append("DailyMode", cookieValue, new CookieOptions
            {
                Expires = DateTime.UtcNow.Add(timeUntilMidnight),
                HttpOnly = true,
                Secure = true, // Set to true for HTTPS
                SameSite = SameSiteMode.Strict
            });

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
