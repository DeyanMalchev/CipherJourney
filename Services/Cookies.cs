using Azure;
using CipherJourney.Models;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using TimeZoneConverter;

namespace CipherJourney.Services
{
    public class Cookies
    {
        public static void CreateCookieAccount(User user, UserPoints userPoints , HttpResponse response)
        {

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(10),
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
                { "RiddlesSolved", userPoints.RiddlesSolved.ToString()},
                { "Score", userPoints.Score.ToString()},
            };

            string serializedData = JsonConvert.SerializeObject(cookieData);

            response.Cookies.Append("CipherJourney", serializedData, cookieOptions);

            Console.WriteLine(serializedData);
        }

        public static void SetDailyModeCookie(string cipher, string sentence, Dictionary<string, bool> wordGuessedStatus, long guessCount, string cipherShift, bool isGameComplete, HttpResponse response, HttpRequest request)
        {
            // Calculate expiry (same logic as before)
            DateTime utcNow = DateTime.UtcNow;
            TimeZoneInfo londonTimeZone = TZConvert.GetTimeZoneInfo("Europe/London");
            DateTime londonNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, londonTimeZone);
            DateTime nextMidnight = londonNow.Date.AddDays(1);
            DateTime expirationUtc = TimeZoneInfo.ConvertTimeToUtc(nextMidnight, londonTimeZone);
            TimeSpan timeUntilMidnight = expirationUtc - utcNow;

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.Add(timeUntilMidnight),
                HttpOnly = true,
                Secure = request.IsHttps, // Use the passed-in request
                SameSite = SameSiteMode.Strict
            };

            // Create the cookie data dictionary with all required fields and correct types
            var cookieData = new Dictionary<string, object>
            {
                { "Cipher", cipher },
                { "Sentence", sentence },
                // Ensure wordGuessedStatus has ORIGINAL words as keys when passed in
                { "WordGuessedStatus", wordGuessedStatus },
                { "GuessCount", guessCount }, // Use the long value
                { "CipherShift", cipherShift },
                { "IsGameComplete", isGameComplete}
            };

            string serializedData = JsonConvert.SerializeObject(cookieData);
            response.Cookies.Append("DailyMode", serializedData, cookieOptions); // Use the constant name
            Console.WriteLine($"DailyMode cookie created/updated: {serializedData}");
        }

        public static void SetLeaderboardCookie(LeaderboardViewModel leaderboardViewModel, HttpResponse response) 
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(15),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            string json = JsonConvert.SerializeObject(leaderboardViewModel);
            
            response.Cookies.Append("LeaderboardData", json, cookieOptions);
            Console.WriteLine(json);
        }
    }
}
