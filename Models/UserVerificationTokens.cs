using System.ComponentModel.DataAnnotations;

namespace CipherJourney.Models
{
    public class UserVerificationTokens
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public string? VerificationToken { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Purpose { get; set; }  // "email_verification", "password_reset", etc.
    }
}

