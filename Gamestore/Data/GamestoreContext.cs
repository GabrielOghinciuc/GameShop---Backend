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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameDto>(entity =>
            {
                // Ensure the table name is explicitly set
                entity.ToTable("Games");

                entity.Property(e => e.Platforms)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
                    .HasColumnType("nvarchar(max)");

                // Ensure unique constraint on Name
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Games_Name");

                // Configure required properties
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Genre)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}
