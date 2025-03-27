using System.ComponentModel.DataAnnotations;


namespace CipherJourney.Models
{
    public class FeedbackModel
    {
        [Required]

        public string Email { get; set; }

        [Required]

        public string Feedback {  get; set; }
    }
}
