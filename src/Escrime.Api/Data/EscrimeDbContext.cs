using Escrime.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Escrime.Api.Data;

public class EscrimeDbContext : DbContext
{
    public EscrimeDbContext(DbContextOptions<EscrimeDbContext> options) : base(options) { }

    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    public DbSet<MatchResultEntity> Matches => Set<MatchResultEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerEntity>(player =>
        {
            player.Property(p => p.Name).IsRequired();
            player.HasMany(p => p.Matches)
                .WithOne()
                .HasForeignKey(m => m.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MatchResultEntity>(match =>
        {
            // Stocké en chaîne ("Win") : lisible directement dans la base
            match.Property(m => m.Result).HasConversion<string>();
        });
    }
}
