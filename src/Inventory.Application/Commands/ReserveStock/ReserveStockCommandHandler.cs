using MediatR;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;
using Inventory.Application.Repositories;
using Inventory.Application.Events;

namespace Inventory.Application.Commands.ReserveStock;

public class ReserveStockCommandHandler(
    IProductRepository productRepository,
    IReservationRepository reservationRepository,
    IEventPublisher eventPublisher  
) : IRequestHandler<ReserveStockCommand, ReserveStockResult>
{
    private readonly TimeSpan _reservationDuration = TimeSpan.FromMinutes(2);

    public async Task<ReserveStockResult> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. جلب المنتج مع قفل الصف لمنع السباق (Row Locking)
            var product = await productRepository.GetByIdWithLockAsync(request.ProductId, cancellationToken);
            if (product == null)
                return new ReserveStockResult(false, ErrorMessage: "Product not found");

            // 2. تنفيذ منطق الحجز في الـ Domain
            product.ReserveStock(request.Quantity);

            // 3. إنشاء الحجز
            var reservation = Reservation.Create(
                request.ProductId,
                request.UserId,
                request.Quantity,
                _reservationDuration
            );

            // 4. الحفظ في قاعدة البيانات
            await reservationRepository.AddAsync(reservation, cancellationToken);
            await productRepository.UpdateAsync(product, cancellationToken);

            // 5. نشر الحدث باستخدام الواجهة الصحيحة
            await eventPublisher.PublishAsync(new InventoryReservedEvent(
                reservation.Id,
                reservation.ProductId,
                reservation.UserId,
                reservation.Quantity,
                reservation.ExpiresAt
            ), cancellationToken);

            return new ReserveStockResult(true, reservation.Id);
        }
        catch (InsufficientStockException ex)
        {
            return new ReserveStockResult(false, ErrorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            return new ReserveStockResult(false, ErrorMessage: "Failed to reserve stock: " + ex.Message);
        }
    }
}