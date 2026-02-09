using MediatR;
using Microsoft.Extensions.Logging;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;
using Inventory.Application.Repositories;
using Inventory.Application.Events;
using Microsoft.Extensions.Hosting;

namespace Inventory.Application.Commands.ReserveStock;

public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, ReserveStockResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ReserveStockCommandHandler> _logger;

    public ReserveStockCommandHandler(
        IProductRepository productRepository,
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        IHostEnvironment env,
        ILogger<ReserveStockCommandHandler> logger)
    {
        _productRepository = productRepository;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _env = env;
        _logger = logger;
    }

    public async Task<ReserveStockResult> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // منع البيع الزائد
            var product = await _productRepository.GetByIdWithLockAsync(request.ProductId, cancellationToken);
            if (product == null)
                return new ReserveStockResult(false, ErrorMessage: "Product not found");

            // حجز الكمية
            product.ReserveStock(request.Quantity);

            // تحديد مدة الحجز
            var duration = _env.IsEnvironment("Testing")
                ? TimeSpan.FromMinutes(5)   // في الاختبار
                : TimeSpan.FromMinutes(2);  // في الإنتاج

            var reservation = Reservation.Create(
                request.ProductId,
                request.UserId,
                request.Quantity,
                duration
            );

            //  حفظ التغييرات
            await _reservationRepository.AddAsync(reservation, cancellationToken);
            await _productRepository.UpdateAsync(product, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Polly نشر الحدث باستخدام 
            var evt = new InventoryReservedEvent(
                reservation.Id,
                request.ProductId,
                request.UserId,
                request.Quantity,
                reservation.ExpiresAt
            );

            await _eventPublisher.PublishAsync(evt, cancellationToken);

            _logger.LogInformation("Reservation {ReservationId} created and event published.", reservation.Id);

            return new ReserveStockResult(true, reservation.Id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error while reserving stock");
            return new ReserveStockResult(false, ErrorMessage: ex.Message);
        }
    }
}
