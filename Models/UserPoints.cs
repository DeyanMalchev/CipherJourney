namespace CipherJourney.Models
{
    public class UserPoints
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int RiddlesSolved { get; set; }
        public int Score { get; set; }

        public int GuessCount { get; set; }
    }
}
