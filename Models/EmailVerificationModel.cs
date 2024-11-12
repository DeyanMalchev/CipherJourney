using System.ComponentModel.DataAnnotations;


namespace CipherJourney.Models
{
    public class EmailVerificationModel
    {
        [Required]
        public string Email { get; set; }

        public string InputToken { get; set; }
    }
}
