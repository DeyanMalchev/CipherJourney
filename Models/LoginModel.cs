using System.ComponentModel.DataAnnotations;

namespace CipherJourney.Models
{
    public class LoginModel
    {
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }
    }
}
