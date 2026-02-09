using Inventory.Domain.Exceptions;

namespace Inventory.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int AvailableStock { get; private set; }
    public int ReservedStock { get; private set; }      // الكمية المجوزة مؤقتا
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; }   //لمنع التحديث المتزامن


    private Product() { }

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
    //  تتم عملية الحجز الحقيقية
    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (AvailableStock < quantity) // عملية التحقق من ان المخزون يكفي
            throw new InsufficientStockException(Id, AvailableStock, quantity);

        AvailableStock -= quantity; // يقلل المتوفر
        ReservedStock += quantity;  // يزيد المحجوز
        UpdatedAt = DateTime.UtcNow;
    }
    // عند انتهء الحجز او إلغاؤه
    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0 || ReservedStock == 0)
            return;

        var actualRelease = Math.Min(quantity, ReservedStock);

        AvailableStock += actualRelease; // يعيد الكمية
        ReservedStock -= actualRelease; // يقلل الكمية المحجوزة
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmPurchase(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (quantity > ReservedStock)
            throw new InvalidOperationException(
                $"Cannot confirm purchase of {quantity} items. Only {ReservedStock} reserved.");

        ReservedStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
