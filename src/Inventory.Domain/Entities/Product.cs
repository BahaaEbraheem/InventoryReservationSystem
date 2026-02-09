using Inventory.Domain.Exceptions;

namespace Inventory.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int AvailableStock { get; private set; }
    public int ReservedStock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Product Create(Guid id, string name, int initialStock)
    {
        if (initialStock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(initialStock));

        return new Product
        {
            Id = id,
            Name = name,
            AvailableStock = initialStock,
            ReservedStock = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // منطق الحجز - يُنفذ في الـ Domain (DDD)
    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (AvailableStock < quantity)
            throw new InsufficientStockException(Id, AvailableStock, quantity);

        AvailableStock -= quantity;
        ReservedStock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // إطلاق الحجز المؤقت
    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0 || quantity > ReservedStock)
            throw new ArgumentException("Invalid release quantity", nameof(quantity));

        AvailableStock += quantity;
        ReservedStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // تحديث المخزون بعد الشراء النهائي
    public void ConfirmPurchase(int quantity)
    {
        if (quantity <= 0 || quantity > ReservedStock)
            throw new ArgumentException("Invalid purchase quantity", nameof(quantity));

        ReservedStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}