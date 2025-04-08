using CipherJourney.Models;
using CipherJourney.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using TimeZoneConverter; // For robust deserialization


namespace CipherJourney.Controllers
{
    public class GameController : Controller
    {

        private readonly CipherJourneyDBContext _context;

        public GameController(CipherJourneyDBContext context)
        {
            _context = context;
        }


        // --- Helper Method to Build Display Data ---
        // Encapsulates the logic to create the display list from the state
        private List<object> BuildSentenceDisplayData(string originalSentence, int cipherShift, Dictionary<string, bool> wordStatus)
        {
            var displayData = new List<object>(); // Initialize the list to hold display info

            // Split sentence into parts, keeping words and delimiters, removing empty parts
            var sentenceParts = Regex.Split(originalSentence, @"(\b[\w']+\b)")
                                     .Where(p => !string.IsNullOrEmpty(p))
                                     .ToList();

            foreach (string part in sentenceParts)
            {
                // Check if the current part is a word using Regex
                bool isActualWord = Regex.IsMatch(part, @"\b[\w']+\b");

                // --- CHANGE HERE ---
                // ONLY process the part if it IS an actual word
                if (isActualWord)
                {
                    // It's a word. Check if it's guessed (case-insensitive lookup)
                    bool isGuessed = wordStatus.TryGetValue(part, out bool status) && status;

                    // Determine the text: original if guessed, ciphered otherwise
                    string displayText = isGuessed ? part : Ciphers.CaesarCipher(part, cipherShift); // Assuming Ciphers class exists

                    // Add the word's display info to our list
                    displayData.Add(new { displayText, isGuessed });
                }
                // ELSE: If the part is NOT a word (it's space or punctuation),
                //       do nothing. Skip it and don't add it to displayData.
                // --- END CHANGE ---
            }

            // Return the list containing ONLY word display information
            return displayData;
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public JsonResult CheckGuess(string guess)
        {
            try
            {
                // --- 1. Load Game State from Cookie ---
                var cookieValue = Request.Cookies["DailyMode"];
                if (string.IsNullOrEmpty(cookieValue))
                {
                    return Json(new { error = "Game state not found. Please start a new game." });
                }

                var gameState = JsonConvert.DeserializeObject<Dictionary<string, object>>(cookieValue);

                if (!gameState.TryGetValue("Sentence", out object sentenceObj) || sentenceObj == null)
                {
                    return Json(new { error = "Original sentence missing from game state." });
                }
                string originalSentence = sentenceObj.ToString();

                int cipherShift = 3;
                if (gameState.TryGetValue("CipherShift", out object shiftObj) && int.TryParse(shiftObj?.ToString(), out int loadedShift))
                {
                    cipherShift = loadedShift;
                }

                long guessCount = 0;
                if (gameState.TryGetValue("GuessCount", out object countObj) && long.TryParse(countObj?.ToString(), out long loadedCount))
                {
                    guessCount = loadedCount;
                }

                // --- 2. Load or Initialize WordGuessedStatus Dictionary (with CORRECT Comparer) ---
                var wordGuessedStatus = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase); // Ensure comparer

                if (gameState.TryGetValue("WordGuessedStatus", out object statusObj) && statusObj is JObject statusJObject)
                {
                    // Deserialize into a temporary standard dictionary first
                    var tempDict = statusJObject.ToObject<Dictionary<string, bool>>();
                    if (tempDict != null)
                    {
                        // Create the final dictionary with the correct comparer, copying items
                        wordGuessedStatus = new Dictionary<string, bool>(tempDict, StringComparer.OrdinalIgnoreCase);
                    }
                    // If tempDict is null, wordGuessedStatus remains empty with the correct comparer
                }

                // Initialize if empty (either not found in cookie or failed deserialization)
                if (wordGuessedStatus.Count == 0 && !string.IsNullOrEmpty(originalSentence))
                {
                    Console.WriteLine("Initializing WordGuessedStatus dictionary..."); // Log initialization
                    var uniqueWords = Regex.Matches(originalSentence, @"\b[\w']+\b")
                                           .Cast<Match>()
                                           .Select(m => m.Value)
                                           .Distinct(StringComparer.OrdinalIgnoreCase);

                    foreach (var word in uniqueWords)
                    {
                        // Use original casing from sentence for the key, comparer handles lookup
                        string originalCasingWord = Regex.Match(originalSentence, $@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase).Value;
                        if (!wordGuessedStatus.ContainsKey(originalCasingWord))
                        {
                            wordGuessedStatus.Add(originalCasingWord, false);
                        }
                        else
                        {
                            // If the same word (ignoring case) appears multiple times with different casing,
                            // the first one encountered wins. This is usually acceptable.
                        }
                    }
                }

                // --- 3. Process the User's Guess ---
                string[] userGuessWords = string.IsNullOrWhiteSpace(guess)
                                            ? new string[0]
                                            : guess.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (userGuessWords.Length > 0)
                {
                    guessCount++;
                    foreach (string guessedWord in userGuessWords)
                    {
                        // Check and update using the case-insensitive dictionary
                        if (wordGuessedStatus.TryGetValue(guessedWord, out bool currentStatus) && !currentStatus)
                        {
                            // Need to find the original key casing to update the dictionary
                            // Find the key in the dictionary that matches the guess case-insensitively
                            string actualKey = wordGuessedStatus.Keys.FirstOrDefault(k => k.Equals(guessedWord, StringComparison.OrdinalIgnoreCase));
                            if (actualKey != null)
                            {
                                wordGuessedStatus[actualKey] = true; // Update using the actual key from the dictionary
                                Console.WriteLine($"Marked '{actualKey}' as guessed."); // Log successful guess update
                            }
                        }
                    }
                }

                // --- 4. Prepare the JSON Response Data using Helper Method ---
                var sentenceDisplayData = BuildSentenceDisplayData(originalSentence, cipherShift, wordGuessedStatus);

                // --- 5. Update the Cookie ---
                string cipher = gameState.TryGetValue("Cipher", out object cipherObj) ? cipherObj.ToString() : "Caesar"; // Get cipher name

                var updatedCookieData = new Dictionary<string, object>
                {
                    { "Cipher", cipher },
                    { "Sentence", originalSentence },
                    { "WordGuessedStatus", wordGuessedStatus }, // Save the updated dictionary
                    { "GuessCount", guessCount },
                    { "CipherShift", cipherShift }
                };

                // (Cookie expiry logic remains the same - looks fine)
                DateTime utcNow = DateTime.UtcNow;
                TimeZoneInfo londonTimeZone = TZConvert.GetTimeZoneInfo("Europe/London");
                DateTime londonNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, londonTimeZone);
                DateTime nextMidnight = londonNow.Date.AddDays(1);
                DateTime expirationUtc = TimeZoneInfo.ConvertTimeToUtc(nextMidnight, londonTimeZone);
                TimeSpan timeUntilMidnight = expirationUtc - utcNow;

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.Add(timeUntilMidnight), // Use calculated expiry
                    HttpOnly = true,
                    Secure = Request.IsHttps, // Use Request.IsHttps dynamically
                    SameSite = SameSiteMode.Strict
                };

                string updatedJson = JsonConvert.SerializeObject(updatedCookieData);
                Response.Cookies.Append("DailyMode", updatedJson, cookieOptions);

                // --- 6. Return JSON ---
                return Json(new
                {
                    success = true, // Indicate the operation succeeded
                    sentenceDisplay = sentenceDisplayData, // Send the prepared display data
                    currentGuessCount = guessCount // **** ADD THIS LINE ****
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckGuess: {ex.ToString()}"); // Log full exception
                return Json(new { error = "An unexpected server error occurred processing your guess." });
            }
        }

        public IActionResult Play()
        {
            // Check for user login cookie first
            if (!Request.Cookies.ContainsKey("CipherJourney"))
            {
                Console.WriteLine("User cookie not found. Redirecting to login.");
                return RedirectToAction("Login", "Login");
            }

            // Check for game state cookie
            if (!Request.Cookies.TryGetValue("DailyMode", out string cookieValue) || string.IsNullOrEmpty(cookieValue))
            {
                Console.WriteLine("DailyMode cookie not found or empty. Generating new game.");
                // You might want GenerateGameDaily to return void and just set the cookie
                GenerateGameDaily(); // Sets the cookie internally
                // Redirect to Play again to load the newly set cookie
                return RedirectToAction("Play");
            }

            try
            {
                // --- Load State (Similar to CheckGuess Start) ---
                var gameState = JsonConvert.DeserializeObject<Dictionary<string, object>>(cookieValue);

                if (!gameState.TryGetValue("Sentence", out object sentenceObj) || sentenceObj == null)
                {
                    // Handle corrupted state - maybe clear cookie and start again?
                    Console.WriteLine("Original sentence missing from game state cookie in Play action.");
                    Response.Cookies.Delete("DailyMode"); // Clear bad cookie
                    return RedirectToAction("Play"); // Try again (will regenerate)
                }
                string originalSentence = sentenceObj.ToString();
                Console.Write(originalSentence);

                int cipherShift = 3;
                if (gameState.TryGetValue("CipherShift", out object shiftObj) && int.TryParse(shiftObj?.ToString(), out int loadedShift))
                {
                    cipherShift = loadedShift;
                }

                // Load the dictionary with the correct comparer
                var wordGuessedStatus = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                if (gameState.TryGetValue("WordGuessedStatus", out object statusObj) && statusObj is JObject statusJObject)
                {
                    var tempDict = statusJObject.ToObject<Dictionary<string, bool>>();
                    if (tempDict != null)
                    {
                        wordGuessedStatus = new Dictionary<string, bool>(tempDict, StringComparer.OrdinalIgnoreCase);
                    }
                }
                // Note: No need to initialize here typically, as GenerateGameDaily should have done it.
                // If initialization IS needed, add the same logic as in CheckGuess.

                // --- ADD CODE TO GET GUESS COUNT ---
                long guessCount = 0; // Default if not found
                if (gameState.TryGetValue("GuessCount", out object countObj) && long.TryParse(countObj?.ToString(), out long loadedCount))
                {
                    guessCount = loadedCount;
                }
                // Pass the count to the View using ViewData
                ViewData["InitialGuessCount"] = guessCount;
                // --- Build the Model for the View using the Helper ---
                var displayModel = BuildSentenceDisplayData(originalSentence, cipherShift, wordGuessedStatus);

                Console.WriteLine($"Passing display model with {displayModel.Count} items to Play view."); // Log model info
                return View(displayModel); // Pass the List<object> to the view
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Play view state: {ex.ToString()}");
                // Handle error - maybe clear cookie and retry?
                Response.Cookies.Delete("DailyMode");
                return RedirectToAction("Play"); // Or show an error view
            }
        }

        // Ensure this method sets the cookie correctly, including an initialized WordGuessedStatus
        public void GenerateGameDaily() // Changed to void as it modifies Response directly
        {
            // Assuming GetDailyModeConfiguration returns sentence and cipher details
            // Let's simulate getting values
            string sentence = "The quick brown fox jumps over the lazy dog"; // Example sentence
            string cipherType = "Caesar";
            int shift = 3; // Example shift

            // Initialize the guessed status dictionary - all false initially
            var initialWordStatus = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var uniqueWords = Regex.Matches(sentence, @"\b[\w']+\b")
                                      .Cast<Match>()
                                      .Select(m => m.Value)
                                      .Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var word in uniqueWords)
            {
                string originalCasingWord = Regex.Match(sentence, $@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase).Value;
                if (!initialWordStatus.ContainsKey(originalCasingWord))
                {
                    initialWordStatus.Add(originalCasingWord, false);
                }
            }

            // Create the cookie data
            var cookieData = new Dictionary<string, object> {
                { "Cipher", cipherType },
                { "Sentence", sentence },
                { "WordGuessedStatus", initialWordStatus },
                { "GuessCount", 0L }, // Start with 0 guesses
                { "CipherShift", shift }
             };

            // Calculate expiry (same logic as in CheckGuess)
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
                Secure = Request.IsHttps, // Use Request.IsHttps
                SameSite = SameSiteMode.Strict
            };

            string jsonValue = JsonConvert.SerializeObject(cookieData);
            Response.Cookies.Append("DailyMode", jsonValue, cookieOptions);
            Console.WriteLine("Generated new DailyMode cookie.");
        }
    }
}
