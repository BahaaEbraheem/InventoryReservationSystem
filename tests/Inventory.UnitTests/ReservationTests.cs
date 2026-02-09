using Xunit;
using Inventory.Domain.Entities;

namespace Inventory.UnitTests;

public class ReservationTests
{
    [Fact]
    public void CreateReservation_ShouldSetCorrectProperties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var quantity = 5;
        var duration = TimeSpan.FromMinutes(2);

        // Act
        var reservation = Reservation.Create(productId, userId, quantity, duration);

        // Assert
        Assert.Equal(productId, reservation.ProductId);
        Assert.Equal(userId, reservation.UserId);
        Assert.Equal(quantity, reservation.Quantity);
        Assert.False(reservation.IsReleased);
        Assert.Null(reservation.ReleasedAt);
        Assert.True(reservation.ExpiresAt > DateTime.UtcNow);
        Assert.True(reservation.ExpiresAt <= DateTime.UtcNow.Add(duration));
    }

    [Fact]
    public void CreateReservation_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var duration = TimeSpan.FromMinutes(2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Reservation.Create(productId, userId, 0, duration));
    }

    [Fact]
    public void ReleaseReservation_ShouldMarkAsReleased()
    {
        // Arrange
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            TimeSpan.FromMinutes(2));

        // Act
        reservation.Release();

        // Assert
        Assert.True(reservation.IsReleased);
        Assert.NotNull(reservation.ReleasedAt);
    }

    [Fact]
    public void ReleaseReservation_Twice_ShouldThrowException()
    {
        // Arrange
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            TimeSpan.FromMinutes(2));
        reservation.Release();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => reservation.Release());
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenNotExpired()
    {
        // Arrange
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            TimeSpan.FromMinutes(2));

        // Act & Assert
        Assert.False(reservation.IsExpired);
    }
}