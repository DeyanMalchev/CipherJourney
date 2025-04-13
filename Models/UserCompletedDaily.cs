namespace CipherJourney.Models
{
    public class UserCompletedDaily
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CompletionDate { get; set; }
    }
}
