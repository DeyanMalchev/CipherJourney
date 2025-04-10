using System.ComponentModel.DataAnnotations;

namespace CipherJourney.Models
{
    public class DeleteAccountModel
    {

        [Required(ErrorMessage = "Password is required to confirm deletion.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Current Password")]
        public string Password { get; set; }

        [Required] // Ensure the checkbox is actually required
        [Display(Name = "Confirm Deletion Consent")]
        // This validation ensures the checkbox MUST be checked
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm you understand the consequences and agree to delete your account.")]
        public bool ConfirmConsent { get; set; }
    }
}
