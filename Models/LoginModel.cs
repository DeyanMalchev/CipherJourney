using System.ComponentModel.DataAnnotations;

namespace Experiments.Models
{
    public class LoginModel
    {

        [Required]
        [StringLength(50)]
        public string Username { get; set; }


        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }
    }
}
