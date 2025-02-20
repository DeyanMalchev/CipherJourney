namespace CipherJourney.Models
{
    public class UserPoints
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int DailyScore { get; set; }
        public int WeeklyScore { get; set; }
        public int DailiesDone { get; set; }
        public int WeekliesDone { get; set; }
    }
}
