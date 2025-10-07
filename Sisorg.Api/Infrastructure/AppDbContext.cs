using Microsoft.EntityFrameworkCore;
using Sisorg.Api.Domain.Entities;

namespace Sisorg.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

    public DbSet<Registry> Registries => Set<Registry>();
    public DbSet<RegistryRow> RegistryRows => Set<RegistryRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Registry>(e =>
        {
            e.ToTable("registries");
            e.HasKey(x => x.Id);
            e.Property(x => x.TimestampUtc).IsRequired();

            e.HasMany(x => x.Rows)
             .WithOne(r => r.Registry)
             .HasForeignKey(r => r.RegistryId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RegistryRow>(e =>
        {
            e.ToTable("registry_rows");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Value).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(x => x.Color).HasMaxLength(6).IsRequired();
        });
    }
}
