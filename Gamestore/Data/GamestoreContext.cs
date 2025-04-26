using Microsoft.EntityFrameworkCore;
using Gamestore.Models;
using System.Text.Json;

namespace Gamestore.Data
{
    public class GamestoreContext : DbContext
    {
        public GamestoreContext(DbContextOptions<GamestoreContext> options)
            : base(options)
        {
        }

        public DbSet<GameDto> Games { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.BoughtGames)
                .HasConversion(
                    v => string.Join(',', v ?? new List<int>()),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                );

            modelBuilder.Entity<GameDto>(entity =>
            {
                entity.ToTable("Games");

                entity.Property(e => e.Platforms)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
                    .HasColumnType("nvarchar(max)");

                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Games_Name");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Genre)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine); // Log SQL queries to the console
            base.OnConfiguring(optionsBuilder);
        }
    }
}
