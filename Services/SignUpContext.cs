using Experiments.Models;
using Microsoft.EntityFrameworkCore;

namespace Experiments.Services
{
    public class SignUpContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public SignUpContext(DbContextOptions<SignUpContext> options) : base(options)
        {

        }
    }

}
