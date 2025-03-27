using CipherJourney.Models;
using CipherJourney.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CipherJourney.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Services.IEmailService _emailService;


        public HomeController(ILogger<HomeController> logger, Services.IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public IActionResult Home()
        {
            if (Request.Cookies.TryGetValue("CipherJourney", out string userIDString))
            {
                return View("HomePage");
            }
            return View("../Login/Login");
        }

        public IActionResult Privacy() {
            return View("Privacy");
        }
        public IActionResult Contacts()
        {
            if(Request.Cookies.ContainsKey("CipherJourney")){

                var values = Request.Cookies["CipherJourney"];
                var valuesDeserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(values);

                FeedbackModel feedbackModel = new FeedbackModel();
                feedbackModel.Email = valuesDeserialized["Email"];

                return View("Contacts", feedbackModel);
            }
            return View("Contacts");
        }


        public async Task<IActionResult> SendFeedback(FeedbackModel feedbackModel)
        {
            await _emailService.SendFeedbackEmail(feedbackModel.Email, feedbackModel.Feedback);
            Console.WriteLine(feedbackModel);

            TempData["FeedbackSuccess"] = "Your feedback has been sent successfully!";

            return RedirectToAction("Contacts");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
