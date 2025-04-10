namespace CipherJourney.Models
{
    public class AccountModel
    {
        public string Username { get; set; }
        public string Email { get; set; }

        public int DailyPoints { get; set; }
        public int WeeklyPoints { get; set; }

        public int DailiesDone { get; set; }
        public int WeekliesDone { get; set; }
    }
}
