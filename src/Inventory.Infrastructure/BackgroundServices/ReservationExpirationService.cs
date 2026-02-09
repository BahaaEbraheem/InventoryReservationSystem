using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Inventory.Application.Repositories;

namespace Inventory.Infrastructure.BackgroundServices;

public class ReservationExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationExpirationService> _logger;
    private readonly TimeSpan _checkInterval;

    public ReservationExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationExpirationService> logger,
        IHostEnvironment env)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        // في الاختبار: كل ثانية
        // في الإنتاج: كل 30 ثانية
        _checkInterval = env.IsEnvironment("Testing")
            ? TimeSpan.FromMinutes(5)
            : TimeSpan.FromSeconds(30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reservation expiration service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var reservationRepository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
                var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // جلب الحجوزات المنتهية
                var expiredReservations = await reservationRepository.GetExpiredReservationsAsync(DateTime.UtcNow, stoppingToken);

                if (expiredReservations.Any())
                {
                    // تعديل المنتج وتعديل الحجز معا
                    await unitOfWork.BeginTransactionAsync(stoppingToken);

                    try
                    {
                        foreach (var reservation in expiredReservations)
                        {
                            var product = await productRepository.GetByIdWithLockAsync(reservation.ProductId, stoppingToken);

                            if (product != null)
                            {
                                // إعادة المخزون , يزيد المتاح ويقلل المحجوز
                                product.ReleaseReservation(reservation.Quantity);
                                await productRepository.UpdateAsync(product, stoppingToken);
                            }

                            await reservationRepository.MarkAsReleasedAsync(reservation.Id, stoppingToken);
                        }

                        await unitOfWork.SaveChangesAsync(stoppingToken);
                        await unitOfWork.CommitAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        await unitOfWork.RollbackAsync(stoppingToken);
                        _logger.LogError(ex, "Error while releasing expired reservations");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reservation expiration service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Reservation expiration service stopped");
    }
}
