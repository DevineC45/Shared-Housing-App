using Microsoft.EntityFrameworkCore;
using SharedHousingApp.Models;

namespace SharedHousingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Chore> Chores { get; set; }

        public DbSet<Expense> Expenses { get; set; } // ✅ New DbSet for Expenses

        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-many: Expense shared with many users
            modelBuilder.Entity<Expense>()
                .HasMany(e => e.SharedWithUsers)
                .WithMany(u => u.SharedExpenses);

            // One-to-many: Expense has one payer (PaidByUser), and a user can pay many expenses
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.PaidByUser)
                .WithMany()
                .HasForeignKey(e => e.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict); // Optional, avoids cascading deletes
        }
    }
}