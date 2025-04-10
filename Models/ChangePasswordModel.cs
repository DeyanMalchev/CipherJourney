using System.ComponentModel.DataAnnotations;

namespace CipherJourney.Models
{
    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string ConfirmPassword { get; set; }
    }
}
