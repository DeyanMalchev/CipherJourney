using CipherJourney.Models;
using System.Text.Json;

namespace CipherJourney.Services
{
    public class Cookies
    {
        public static void CreateCookieAccount(User user, UserPoints userPoints , HttpResponse response)
        {
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

            string serializedData = JsonSerializer.Serialize(cookieData);

            response.Cookies.Append("CipherJourney", serializedData);

            Console.WriteLine(serializedData);
        }
    }
}
