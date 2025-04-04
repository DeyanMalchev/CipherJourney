using CipherJourney.Models;
using CipherJourney.Services;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace CipherJourney.Controllers
{
    public class GameController : Controller
    {

        private readonly CipherJourneyDBContext _context;

        public GameController(CipherJourneyDBContext context)
        {
            _context = context;
        }

        public Dictionary<string, bool> ProccessGameData(string cookieData)
        {

            var cookieValue = Request.Cookies["DailyMode"];
            if (cookieValue != null)
            {
                var deserializedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(cookieValue);

                string cipher = deserializedData["Cipher"].ToString();
                string sentence = deserializedData["Sentence"].ToString();

                // Deserialize guesses dictionary
                var guessesJson = deserializedData["Guesses"].ToString();
                var guessedWords = JsonConvert.DeserializeObject<Dictionary<string, bool>>(guessesJson);

                Console.WriteLine($"Cipher: {cipher}");
                Console.WriteLine($"Sentence: {sentence}");
                Console.WriteLine($"Guessed Words: {string.Join(", ", guessedWords.Select(kv => $"{kv.Key}: {kv.Value}"))}");

                return guessedWords;
            }


            return null;
        }

        public IActionResult Play()
        {

            if (Request.Cookies.TryGetValue("CipherJourney", out string userIDString))
            {
                if (!Request.Cookies.ContainsKey("DailyMode"))
                {
                    Console.WriteLine("Doesnt");
                    GenerateGameDaily();

                    return RedirectToAction("Play");
                }


                var values = Request.Cookies["DailyMode"];

                var sentenceArray = ProccessGameData(values);

                return View(sentenceArray);
            }

            return RedirectToAction("Login", "Login");
        }

        public string[] GenerateGameDaily()
        {
            var values = DB_Queries.GetDailyModeConfiguration(_context);
            Cookies.DailyModeCookie(values[0], values[1], Response);

            return values;
        }
    }
}
