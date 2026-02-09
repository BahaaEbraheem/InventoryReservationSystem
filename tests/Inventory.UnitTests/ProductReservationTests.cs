using Xunit;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;

namespace Inventory.UnitTests;

public class ProductReservationTests
{
    private readonly Guid _productId = Guid.NewGuid();
    private readonly string _productName = "Test Product";

    [Fact]
    public void ReserveStock_WithSufficientStock_ShouldReduceAvailableStock()
    {
        // Arrange
        var product = Product.Create(_productId, _productName, initialStock: 100);

        // Act
        product.ReserveStock(10);

        // Assert
        Assert.Equal(90, product.AvailableStock);
        Assert.Equal(10, product.ReservedStock);
    }

    [Fact]
    public void ReserveStock_WithInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var product = Product.Create(_productId, _productName, initialStock: 5);

        // Act & Assert
        var exception = Assert.Throws<InsufficientStockException>(() =>
            product.ReserveStock(10));

        Assert.Equal(_productId, exception.ProductId);
        Assert.Equal(5, exception.AvailableStock);
        Assert.Equal(10, exception.RequestedQuantity);
    }

    [Fact]
    public void ReserveStock_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var product = Product.Create(_productId, _productName, initialStock: 10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.ReserveStock(0));
    }

    [Fact]
    public void ReserveStock_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var product = Product.Create(_productId, _productName, initialStock: 10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.ReserveStock(-5));
    }

    [Fact]
    public void ReleaseReservation_ShouldIncreaseAvailableStock()
    {
        // Arrange
        var product = Product.Create(_productId, _productName, initialStock: 100);
        product.ReserveStock(20);

        // Act
        product.ReleaseReservation(10);

        // Assert
        Assert.Equal(90, product.AvailableStock);
        Assert.Equal(10, product.ReservedStock);
    }

    [Fact]
    public void ReleaseReservation_WithInvalidQuantity_ShouldThrowException()
    {
        // Arrange
        var product = Product.Create(_productId, _productName, initialStock: 100);
        product.ReserveStock(10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.ReleaseReservation(15));
    }

    [Fact]
    public void ConfirmPurchase_ShouldReduceReservedStock()
    {
        // Arrange
        var product = Product.Create(_productId, _productName, initialStock: 100);
        product.ReserveStock(30);

        // Act
        product.ConfirmPurchase(20);

        // Assert
        Assert.Equal(70, product.AvailableStock);
        Assert.Equal(10, product.ReservedStock);
    }
}