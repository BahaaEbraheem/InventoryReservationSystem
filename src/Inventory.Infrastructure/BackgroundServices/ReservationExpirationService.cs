using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Inventory.Application.Repositories;

namespace Inventory.Infrastructure.BackgroundServices;

public class ReservationExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationExpirationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public ReservationExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reservation expiration service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // إنشاء نطاق جديد للخدمات المُسجّلة كـ Scoped
                using (var scope = _scopeFactory.CreateScope())
                {
                    var reservationRepository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();

                    var expiredReservations = await reservationRepository
                        .GetExpiredReservationsAsync(DateTime.UtcNow, stoppingToken);

                    foreach (var reservation in expiredReservations)
                    {
                        await reservationRepository.ReleaseReservationAsync(reservation.Id, stoppingToken);
                        _logger.LogInformation("Released expired reservation {ReservationId} for product {ProductId}",
                            reservation.Id, reservation.ProductId);
                    }

                    if (expiredReservations.Any())
                    {
                        _logger.LogInformation("Released {Count} expired reservations", expiredReservations.Count);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // التوقف الطبيعي عند إلغاء الـ CancellationToken
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reservation expiration service");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // التوقف عند إلغاء الـ CancellationToken
                break;
            }
        }

        _logger.LogInformation("Reservation expiration service stopped");
    }
}