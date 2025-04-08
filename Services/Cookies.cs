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

            //Time until exparation
            TimeSpan timeUntilMidnight = expirationUtc - utcNow;

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.Add(timeUntilMidnight),
                HttpOnly = true, // Prevents JavaScript access for security
                Secure = true, // Enforces HTTPS
                SameSite = SameSiteMode.Strict
            };


            string[] words = Regex.Split(sentence, @"\W+").Where(w => w.Length > 0).ToArray();
            Console.WriteLine(words);

            // Cipher each word separately
            string[] cipheredWords = new string[words.Length];
            Dictionary<string, bool> word_bool = new Dictionary<string, bool>();

            for (int i = 0; i < words.Length; i++) {
                cipheredWords[i] = Ciphers.CaesarCipher(words[i], 3);
                word_bool.Add(cipheredWords[i], false);
            }

            var cookieData = new Dictionary<string, object>
            {
                { "Cipher", cipher },
                { "Sentence", sentence },
                { "WordGuessedStatus", word_bool },
                { "GuessCount", 0 },
                { "CipherShift", 3 } 

            };

            string serializedData = JsonConvert.SerializeObject(cookieData);

            response.Cookies.Append("DailyMode", serializedData, cookieOptions);

            Console.WriteLine(serializedData);
        }
    }
}
