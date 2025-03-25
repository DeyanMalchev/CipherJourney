using CipherJourney.Models;
using CipherJourney.Services;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.Text.Json;
using Newtonsoft.Json;



namespace CipherJourney.Controllers
{
    public class GameController : Controller
    {

        private readonly CipherJourneyDBContext _context;

        public GameController(CipherJourneyDBContext context)
        {
            _context = context;
        }

        public IActionResult Play()
        {

            if (!Request.Cookies.ContainsKey("DailyMode")) {
                Console.WriteLine("Doesnt");
                GenerateGameDaily();

                return RedirectToAction("Play");
            }


            var values = Request.Cookies["DailyMode"];
            var valuesDeserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(values);

            string[] valuesArray = valuesDeserialized.Values.ToArray();
            Console.WriteLine(valuesDeserialized.GetType());

            return View(valuesArray);
        }

        public string[] GenerateGameDaily()
        {
            var values = DB_Queries.GetDailyModeConfiguration(_context);
            Cookies.DailyModeCookie(values[0], values[1], Response);

            //values[1] = Ciphers.CeaserCipher(values[1], 3);

            return values;
        }
    }
}
