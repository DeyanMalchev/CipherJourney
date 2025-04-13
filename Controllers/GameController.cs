﻿using CipherJourney.Models;
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
            var displayData = new List<object>();
            if (string.IsNullOrEmpty(originalSentence) || wordStatus == null) return displayData;

            var sentenceParts = Regex.Split(originalSentence, @"(\b[\w']+\b)")
                                      .Where(p => !string.IsNullOrEmpty(p))
                                      .ToList();
            foreach (string part in sentenceParts)
            {
                bool isActualWord = Regex.IsMatch(part, @"\b[\w']+\b");
                if (isActualWord)
                {
                    bool isGuessed = wordStatus.TryGetValue(part, out bool status) && status;
                    string displayText = isGuessed ? part : Ciphers.CaesarCipher(part, cipherShift);
                    displayData.Add(new { displayText, isGuessed });
                }
            }
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
                var wordGuessedStatus = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

                if (gameState.TryGetValue("WordGuessedStatus", out object statusObj) && statusObj is JObject statusJObject)
                {
                    var tempDict = statusJObject.ToObject<Dictionary<string, bool>>();
                    if (tempDict != null)
                    {
                        wordGuessedStatus = new Dictionary<string, bool>(tempDict, StringComparer.OrdinalIgnoreCase);
                    }
                }

                // Initialize if empty (either not found in cookie or failed deserialization)
                if (wordGuessedStatus.Count == 0 && !string.IsNullOrEmpty(originalSentence))
                {
                    Console.WriteLine("Initializing WordGuessedStatus dictionary...");
                    var uniqueWords = Regex.Matches(originalSentence, @"\b[\w']+\b")
                                                .Cast<Match>()
                                                .Select(m => m.Value)
                                                .Distinct(StringComparer.OrdinalIgnoreCase);

                    foreach (var word in uniqueWords)
                    {
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
                        if (wordGuessedStatus.TryGetValue(guessedWord, out bool currentStatus) && !currentStatus)
                        {
                            string actualKey = wordGuessedStatus.Keys.FirstOrDefault(k => k.Equals(guessedWord, StringComparison.OrdinalIgnoreCase));
                            if (actualKey != null)
                            {
                                wordGuessedStatus[actualKey] = true;
                                Console.WriteLine($"Marked '{actualKey}' as guessed.");
                            }
                        }
                    }
                }

                // --- 4. Prepare the JSON Response Data using Helper Method ---
                var sentenceDisplayData = BuildSentenceDisplayData(originalSentence, cipherShift, wordGuessedStatus);

                // --- 5. Update the Cookie (using the Cookies class method) ---
                string cipher = gameState.TryGetValue("Cipher", out object cipherObj) ? cipherObj.ToString() : "Caesar"; // Get cipher name

                // Call the Cookies class method to handle the cookie update
                Cookies.SetDailyModeCookie(cipher, originalSentence, wordGuessedStatus, guessCount, cipherShift, Response, Request);

                // --- 6. Return JSON ---
                return Json(new
                {
                    success = true,
                    sentenceDisplay = sentenceDisplayData,
                    currentGuessCount = guessCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckGuess: {ex.ToString()}");
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
                // Use the Cookies class method here
                GenerateGameDaily();
                return RedirectToAction("Play");
            }

            try
            {
                // --- Load State (Similar to CheckGuess Start) ---
                var gameState = JsonConvert.DeserializeObject<Dictionary<string, object>>(cookieValue);

                if (!gameState.TryGetValue("Sentence", out object sentenceObj) || sentenceObj == null)
                {
                    // Handle corrupted state
                    Console.WriteLine("Original sentence missing from game state cookie in Play action.");
                    Response.Cookies.Delete("DailyMode");
                    return RedirectToAction("Play");
                }
                string originalSentence = sentenceObj.ToString();
                Console.Write(originalSentence);

                int cipherShift = 3;
                if (gameState.TryGetValue("CipherShift", out object shiftObj) && int.TryParse(shiftObj?.ToString(), out int loadedShift))
                {
                    cipherShift = loadedShift;
                }

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
                bool isGameComplete = wordGuessedStatus.Count > 0 && wordGuessedStatus.Values.All(guessed => guessed);
                ViewData["IsComplete"] = isGameComplete;

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

                Console.WriteLine($"Passing display model with {displayModel.Count} items to Play view.");
                return View(displayModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Play view state: {ex.ToString()}");
                // Handle error
                Response.Cookies.Delete("DailyMode");
                return RedirectToAction("Play");
            }
        }

        // Ensure this method sets the cookie correctly, including an initialized WordGuessedStatus
        public void GenerateGameDaily()
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

            // Use the Cookies class method to create the cookie
            Cookies.SetDailyModeCookie(cipherType, sentence, initialWordStatus, 0, shift, Response, Request);
        }
    }
}
