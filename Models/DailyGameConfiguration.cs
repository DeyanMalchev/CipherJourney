namespace CipherJourney.Models
{
    public class DailyGameConfiguration
    {
        public int Id { get; set; }
        public string Sentence { get; set; }
        public string Cipher { get; set; }
        public string Key { get; set; }
        public DateTime Date { get; set; }

    }
}
