﻿using CipherJourney.Models;
using CipherJourney.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CipherJourney.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly CipherJourneyDBContext _context;
        public LeaderboardController(CipherJourneyDBContext context)
        {
            _context = context;
        }

        public IActionResult Leaderboard()
        {
            if (Request.Cookies.TryGetValue("LeaderboardStats", out var cachedJson))
            {
                var cached = JsonConvert.DeserializeObject<LeaderboardViewModel>(cachedJson);
                return View(cached);
            }

            var leaderboardData = BuildLeaderboardData();
            var options = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(10),
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict
            };

            var serialized = JsonConvert.SerializeObject(leaderboardData);
            Response.Cookies.Append("LeaderboardStats", serialized, options);

            return View(leaderboardData);
        }

        // This method gets the leaderboard data
        private LeaderboardViewModel BuildLeaderboardData()
        {
            // Step 1: Get all users ordered by score (Consider performance for large datasets)
            // Fetching all users might be inefficient. Consider adding filtering or pagination
            // if the number of users is very large.
            var allUsersRanked = (from up in _context.UserPoints
                                  join u in _context.Users on up.UserId equals u.Id
                                  orderby up.Score descending
                                  select new
                                  {
                                      up.UserId,
                                      u.Username,
                                      up.Score
                                  }).ToList(); // Materialize the full ranked list once

            // Step 2: Create Top 10 with Rank
            var top10 = allUsersRanked
                .Take(10)
                .Select((entry, index) => new LeaderboardEntry
                {
                    Rank = index + 1, // Rank is 1-based index within the top 10 slice
                    Username = entry.Username,
                    Score = entry.Score
                })
                .ToList();

            // Step 3: Get Current User Info
            // Consider adding error handling for cookie parsing
            var userCookie = Request.Cookies["CipherJourney"];
            if (userCookie == null)
            {
                // Handle case where cookie is missing - perhaps redirect to login or return error
                // For now, let's assume it exists or return an empty model / throw exception
                // depending on your application flow.
                // Example: return new LeaderboardViewModel { Top10 = top10, SurroundingEntries = new List<LeaderboardEntry>(), CurrentUsername = null };
                throw new InvalidOperationException("User cookie not found.");
            }
            var userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(userCookie);
            // It's safer to use TryGetValue
            if (!userData.TryGetValue("Username", out var currentUsername))
            {
                throw new InvalidOperationException("Username not found in cookie data.");
            }
            // Similarly for UserId if needed later, though it's not used in the surrounding logic directly
            // int userId = int.Parse(userData["UserId"]);

            // Step 4: Find Current User's Rank and Index in the *full* list
            // Use the materialized allUsersRanked list
            var userRankInfo = allUsersRanked
                .Select((entry, index) => new { entry.Username, Rank = index + 1, Index = index })
                .FirstOrDefault(e => e.Username == currentUsername);

            List<LeaderboardEntry> surroundingEntries = new List<LeaderboardEntry>();
            int currentUserRank = -1; // Default if user not found

            if (userRankInfo != null)
            {
                currentUserRank = userRankInfo.Rank;
                int userIndex = userRankInfo.Index;

                // Step 5: Get Surrounding Entries from the *full* list
                int startIndex = Math.Max(0, userIndex - 2); // Start index for the slice
                int count = 5; // Number of items to take

                // Adjust count if near the end of the list
                if (startIndex + count > allUsersRanked.Count)
                {
                    count = allUsersRanked.Count - startIndex;
                }

                // Get the slice of anonymous types
                var surroundingAnonymous = allUsersRanked
                    .Skip(startIndex)
                    .Take(count);

                // Step 6: Convert the surrounding slice to LeaderboardEntry with correct Rank
                // *** This is the corrected part ***
                surroundingEntries = surroundingAnonymous
                    .Select((entry, index) => new LeaderboardEntry
                    {
                        // The Rank is the startIndex + the index within the slice + 1
                        Rank = startIndex + index + 1,
                        Username = entry.Username,
                        Score = entry.Score
                    })
                    .ToList();
            }
            // else: Handle the case where the current user wasn't found in the ranked list.
            //       surroundingEntries will remain empty.

            // Step 7: Build the ViewModel
            var viewModel = new LeaderboardViewModel
            {
                Top10 = top10,
                SurroundingEntries = surroundingEntries, // Now holds List<LeaderboardEntry>
                CurrentUsername = currentUsername
            };

            return viewModel;
        }
    }
}
