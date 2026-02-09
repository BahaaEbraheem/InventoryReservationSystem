using Inventory.Application.Repositories;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class EfReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;

    public EfReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.Reservations.AddAsync(reservation, cancellationToken);
    }

    // الحجوزات المنتهية
    public async Task<List<Reservation>> GetExpiredReservationsAsync(
        DateTime cutoffTime,
        CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(r => r.ExpiresAt <= cutoffTime && !r.IsReleased)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task MarkAsReleasedAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);

        if (reservation == null)
            return;

        reservation.Release();

        _context.Reservations.Update(reservation);
    }
}
