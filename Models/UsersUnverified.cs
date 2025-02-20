using System.ComponentModel.DataAnnotations;

namespace CipherJourney.Models
{
    public class UsersUnverified
    {
        public int Id { get; set; }
        public int UserID { get; set; }

        public string? VerificationToken { get; set; }
        public DateTime DateOfCreation { get; set; }
    }
}

