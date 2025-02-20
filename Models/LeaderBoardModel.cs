using System.ComponentModel.DataAnnotations;

namespace CipherJourney.Models
{
    public class LeaderboardModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; }

        public int TotalPoints { get; set; }
    }
}

