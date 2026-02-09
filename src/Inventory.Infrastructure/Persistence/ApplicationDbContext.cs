using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities;

namespace Inventory.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureProduct(modelBuilder);
        ConfigureReservation(modelBuilder);
    }

    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.AvailableStock)
                .IsRequired();

            entity.Property(p => p.ReservedStock)
                .IsRequired();

            entity.Property(p => p.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();


            entity.HasIndex(p => p.Name);
        });
    }

    private static void ConfigureReservation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.Quantity)
                .IsRequired();

            entity.Property(r => r.ReservedAt)
                .IsRequired();

            entity.Property(r => r.ExpiresAt)
                .IsRequired();

            entity.Property(r => r.IsReleased)
                .IsRequired();

            // Foreign Key Relationship
            entity.HasOne(r => r.Product)
                 .WithMany()
                 .HasForeignKey(r => r.ProductId)
                 .OnDelete(DeleteBehavior.Restrict);


            // Indexes for background job performance
            entity.HasIndex(r => r.ExpiresAt);
            entity.HasIndex(r => new { r.ProductId, r.UserId, r.IsReleased });
        });
    }
}
