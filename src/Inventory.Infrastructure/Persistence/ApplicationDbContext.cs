using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities;
using Inventory.Application.Repositories;

namespace Inventory.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.AvailableStock).IsRequired();
            entity.Property(p => p.ReservedStock).IsRequired();
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.ExpiresAt);
            entity.HasIndex(r => new { r.ProductId, r.UserId, r.IsReleased });
        });
    }
}