using System.ComponentModel.DataAnnotations;

namespace CipherJourney.Models
{
    public class LoginUsernameModel : LoginModel
    {

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

    }
}
