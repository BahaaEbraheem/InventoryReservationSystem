using System.Net.Http.Json;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Inventory.IntegrationTests;

public class ConcurrencyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ConcurrencyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.Timeout = TimeSpan.FromSeconds(60);
    }

    [Fact]
    public async Task ConcurrentReservations_ShouldNotOverSell()
    {
        var productId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Products.Add(Product.Create(productId, "Test Product", 10));
            await dbContext.SaveChangesAsync();
        }

        var tasks = Enumerable.Range(0, 11)
            .Select(_ => _client.PostAsJsonAsync("/api/inventory/reserve", new
            {
                productId,
                quantity = 1,
                userId = Guid.NewGuid()
            }))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        Assert.Equal(10, responses.Count(r => r.IsSuccessStatusCode));
        Assert.Equal(1, responses.Count(r => !r.IsSuccessStatusCode));

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await dbContext.Products.FindAsync(productId);

            Assert.NotNull(product);
            Assert.Equal(0, product.AvailableStock);
            Assert.Equal(10, product.ReservedStock);
        }
    }

    [Fact]
    public async Task ExpiredReservations_ShouldBeReleased()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Products.Add(Product.Create(productId, "Test Product", 5));
            await dbContext.SaveChangesAsync();
        }

        var reserveRequest = new
        {
            productId,
            quantity = 3,
            userId
        };

        var response = await _client.PostAsJsonAsync("/api/inventory/reserve", reserveRequest);
        Assert.True(response.IsSuccessStatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await dbContext.Products.FindAsync(productId);

            Assert.Equal(2, product.AvailableStock);
            Assert.Equal(3, product.ReservedStock);
        }

        await Task.Delay(5000);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var product = await dbContext.Products.FindAsync(productId);

            Assert.Equal(5, product.AvailableStock);
            Assert.Equal(0, product.ReservedStock);
        }
    }
}
