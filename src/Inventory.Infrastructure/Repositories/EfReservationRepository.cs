using Inventory.Application.Repositories;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class EfReservationRepository(ApplicationDbContext context) : IReservationRepository
{
    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await context.Reservations.AddAsync(reservation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Reservation>> GetExpiredReservationsAsync(
        DateTime cutoffTime,
        CancellationToken cancellationToken = default)
    {
        return await context.Reservations
            .Where(r => r.ExpiresAt < cutoffTime && !r.IsReleased)
            .ToListAsync(cancellationToken);
    }

    public async Task ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await context.Reservations.FindAsync(new object[] { reservationId }, cancellationToken);
        if (reservation == null || reservation.IsReleased)
            return;

        reservation.Release();
        context.Reservations.Update(reservation);

        // تحديث المخزون أيضاً
        var product = await context.Products.FindAsync(new object[] { reservation.ProductId }, cancellationToken);
        if (product != null)
        {
            product.ReleaseReservation(reservation.Quantity);
            context.Products.Update(product);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}