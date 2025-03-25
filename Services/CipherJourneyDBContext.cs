using CipherJourney.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;

namespace CipherJourney.Services
{
    public class CipherJourneyDBContext : DbContext
    {
        public CipherJourneyDBContext(DbContextOptions<CipherJourneyDBContext> options) : base(options) { 
        
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserPoints> UserPoints { get; set; }
        public DbSet<UsersUnverified> UsersUnverified { get; set; }
        public DbSet<LeaderboardModel> Leaderboard { get; set; }

        public DbSet<CipherModel> Ciphers { get; set; }
        public DbSet<SentenceDailyModel> SentencesDaily { get; set; }
    }
}
