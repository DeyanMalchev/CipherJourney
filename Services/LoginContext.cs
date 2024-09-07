using Experiments.Models;
using Microsoft.EntityFrameworkCore;

namespace Experiments.Services
{
    public class LoginContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public LoginContext(DbContextOptions<LoginContext> options) : base(options)
        {

        }
    }
}
