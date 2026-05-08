using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Inventory.IntegrationTests
{
    public class ExpirationTests : IClassFixture<CustomWebApplicationFactory_WithBackground>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory_WithBackground _factory;
        public ExpirationTests(CustomWebApplicationFactory_WithBackground factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
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
}
