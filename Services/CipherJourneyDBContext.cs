using Experiments.Models;
using Microsoft.EntityFrameworkCore;

namespace CipherJourney.Services
{
    public class CipherJourneyDBContext : DbContext
    {
        public CipherJourneyDBContext(DbContextOptions<CipherJourneyDBContext> options) : base(options) { 
        
        }

        public DbSet<User> Users { get; set; }
        public DbSet<User> UserPoints { get; set; }

    }
}
