using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Entities;

public class Reservation
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime ReservedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsReleased { get; private set; }
    public DateTime? ReleasedAt { get; private set; }

    public static Reservation Create(Guid productId, Guid userId, int quantity, TimeSpan duration)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new Reservation
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            Quantity = quantity,
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(duration),
            IsReleased = false
        };
    }

    public void Release()
    {
        if (IsReleased)
            throw new InvalidOperationException("Reservation already released");

        IsReleased = true;
        ReleasedAt = DateTime.UtcNow;
    }

    public bool IsExpired => !IsReleased && DateTime.UtcNow > ExpiresAt;
}