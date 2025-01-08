using CipherJourney.Models;
using System.Text.Json;

namespace CipherJourney.Services
{
    public class Cookies
    {
        public static void CreateCookie(User user, UserPoints userPoints , HttpResponse response)
        {
            var cookieData = new Dictionary<string, string>
            {
                { "Authenticated", "true"},
                { "UserId", user.Id.ToString() },
                { "Username", user.Username },
                { "Email", user.Email },
                { "Points", userPoints.Score.ToString()},
                { "Daily", userPoints.DailyAmountDone.ToString()},
                { "Weekly", userPoints.WeeklyAmountDone.ToString()}
            };

            string serializedData = JsonSerializer.Serialize(cookieData);

            response.Cookies.Append("CipherJourney", serializedData);

            Console.WriteLine(serializedData);
        }
    }
}
