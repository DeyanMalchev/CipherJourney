using CipherJourney.Models;
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
            LeaderboardViewModel leaderboardData;
            if (!Request.Cookies.TryGetValue("CipherJourney", out var userCookie)) {

                leaderboardData = BuildLeaderboardData(userCookie);
                return View(leaderboardData);
            }

            if (Request.Cookies.TryGetValue("LeaderboardStats", out var cachedJson))
            {
                var cached = JsonConvert.DeserializeObject<LeaderboardViewModel>(cachedJson);
                return View(cached);
            }

            

            leaderboardData = BuildLeaderboardData(userCookie);
            Cookies.SetLeaderboardCookie(leaderboardData, Response);

            return View(leaderboardData);
        }

        // This method gets the leaderboard data
        private LeaderboardViewModel BuildLeaderboardData(string userCookie)
        {

            var allUsersRanked = (from up in _context.Leaderboard
                                  join u in _context.Users on up.UserId equals u.Id
                                  orderby up.TotalPoints descending
                                  select new
                                  {
                                      up.UserId,
                                      u.Username,
                                      up.TotalPoints
                                  }).ToList(); // Materialize the full ranked list once

            var top10 = allUsersRanked
                .Take(10)
                .Select((entry, index) => new LeaderboardEntry
                {
                    Rank = index + 1, // Rank is 1-based index within the top 10 slice
                    Username = entry.Username,
                    TotalPoints = entry.TotalPoints
                })
                .ToList();

            LeaderboardViewModel viewModel;

            if (userCookie == null)
            {

                viewModel = new LeaderboardViewModel
                {
                    Top10 = top10
                };

                return viewModel;
            }
            
            var userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(userCookie);

            string currentUsername = userData["Username"];


            var userRankInfo = allUsersRanked
                .Select((entry, index) => new { entry.Username, Rank = index + 1, Index = index })
                .FirstOrDefault(e => e.Username == currentUsername);

            List<LeaderboardEntry> surroundingEntries = new List<LeaderboardEntry>();
            int currentUserRank = -1; // Default if user not found

            if (userRankInfo != null)
            {
                currentUserRank = userRankInfo.Rank;
                int userIndex = userRankInfo.Index;

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

                surroundingEntries = surroundingAnonymous
                    .Select((entry, index) => new LeaderboardEntry
                    {
                        Rank = startIndex + index + 1,
                        Username = entry.Username,
                        TotalPoints = entry.TotalPoints
                    })
                    .ToList();
            }

            viewModel = new LeaderboardViewModel
            {
                Top10 = top10,
                SurroundingEntries = surroundingEntries, 
                CurrentUsername = currentUsername
            };

            return viewModel;
        }

        public IActionResult RefreshLeaderboard() {
            Response.Cookies.Delete("LeaderboardData");
            Console.WriteLine("Cooke leaderboard deleted");
            Console.WriteLine(Request.Cookies["LeaderboardData"]);
            return RedirectToAction("Leaderboard");
        }
    }
}
