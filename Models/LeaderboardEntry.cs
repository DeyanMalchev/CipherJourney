namespace CipherJourney.Models
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }       // The leaderboard position (1, 2, 3...)
        public string Username { get; set; }
        public int Score { get; set; }
    }

}
