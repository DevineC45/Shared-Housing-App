using Microsoft.EntityFrameworkCore;
using SharedHousingApp.Models;

namespace SharedHousingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Chore> Chores { get; set; }
    }
}