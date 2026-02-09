using Inventory.Domain.Entities;

namespace Inventory.Application.Repositories;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<List<Reservation>> GetExpiredReservationsAsync(DateTime cutoffTime, CancellationToken cancellationToken = default);
    Task ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
}