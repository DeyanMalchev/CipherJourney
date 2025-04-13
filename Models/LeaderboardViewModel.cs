namespace CipherJourney.Models
{
    public class LeaderboardViewModel
    {
        public List<LeaderboardEntry> Top10 { get; set; } = new();
        public List<LeaderboardEntry> SurroundingEntries { get; set; } = new();
        public string CurrentUsername { get; set; } = string.Empty;
    }
}
