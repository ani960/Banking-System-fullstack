using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Otp> Otps => Set<Otp>(); // 🔥 NEW

        // Fluent API Configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Account → User (Many-to-One)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal Precision (IMPORTANT for money 💰)
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            // 🔥 OTP CONFIGURATION
            modelBuilder.Entity<Otp>(entity =>
            {
                entity.ToTable("Otps");

                entity.Property(o => o.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(o => o.Code)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(o => o.ExpiryTime)
                    .IsRequired();

                // 🚀 Index for faster lookup
                entity.HasIndex(o => o.Email);
            });

            // Table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<Transaction>().ToTable("Transactions");
        }
    }
}