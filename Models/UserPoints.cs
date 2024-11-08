namespace CipherJourney.Models
{
    public class UserPoints
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int Score { get; set; }
        public int DailyAmountDone { get; set; }
        public int WeeklyAmountDone { get; set; }
    }
}
