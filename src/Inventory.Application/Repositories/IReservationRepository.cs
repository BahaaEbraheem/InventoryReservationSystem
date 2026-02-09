using Inventory.Domain.Entities;

namespace Inventory.Application.Repositories;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);

    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Reservation>> GetExpiredReservationsAsync(
        DateTime cutoffTime,
        CancellationToken cancellationToken = default);

    Task MarkAsReleasedAsync(Guid reservationId, CancellationToken cancellationToken = default);
}
