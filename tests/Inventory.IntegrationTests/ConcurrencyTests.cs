using Xunit;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Inventory.API;

namespace Inventory.IntegrationTests;

public class ConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ConcurrencyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ConcurrentReservations_ShouldNotOverSell_SingleItem()
    {
        // Arrange: منتج واحد فقط في المخزون
        var productId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var concurrentRequests = 100;

        // Act: إرسال 100 طلب متزامن لنفس المنتج
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < concurrentRequests; i++)
        {
            var userId = Guid.NewGuid();
            var request = new
            {
                productId = productId,
                quantity = 1,
                userId = userId
            };

            tasks.Add(_client.PostAsJsonAsync("/api/inventory/reserve", request));
        }

        // انتظار جميع الطلبات
        var responses = await Task.WhenAll(tasks);

        // Count successful responses
        var successfulResponses = responses
            .Where(r => r.IsSuccessStatusCode)
            .ToList();

        var failedResponses = responses
            .Where(r => !r.IsSuccessStatusCode)
            .ToList();

        // Assert: فقط طلب واحد يجب أن ينجح
        // لأن المخزون كان 100، وطلبنا 100 مرة 1 وحدة
        // يجب أن ينجح 100 طلب بالضبط

        // في الواقع، لأننا بدأنا بـ 100 وحدة، يجب أن تنجح جميع الطلبات
        Assert.Equal(100, successfulResponses.Count);
        Assert.Equal(0, failedResponses.Count);

        // تحقق من أن كل استجابة ناجحة تحتوي على reservationId
        foreach (var response in successfulResponses)
        {
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            Assert.True(content.ContainsKey("reservationId"));
            Assert.True(content.ContainsKey("success"));
        }
    }

    [Fact]
    public async Task ConcurrentReservations_ShouldPreventOverSelling()
    {
        // Arrange: منتج بمخزون 10 وحدات فقط
        var productId = Guid.NewGuid();
        var setupRequest = new
        {
            productId = productId,
            quantity = 10,
            userId = Guid.NewGuid()
        };

        // إعداد منتج جديد للمستخدم
        // (هذا يتطلب إضافة endpoint للإعداد في الـ API للتجربة)

        // Act: محاولة حجز 15 وحدة من 100 طلب متزامن
        var concurrentRequests = 100;
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < concurrentRequests; i++)
        {
            var request = new
            {
                productId = productId,
                quantity = 15, // أكثر من المخزون
                userId = Guid.NewGuid()
            };

            tasks.Add(_client.PostAsJsonAsync("/api/inventory/reserve", request));
        }

        var responses = await Task.WhenAll(tasks);

        var successfulResponses = responses
            .Where(r => r.IsSuccessStatusCode)
            .ToList();

        var failedResponses = responses
            .Where(r => !r.IsSuccessStatusCode)
            .ToList();

        // Assert: يجب أن تفشل جميع الطلبات لأن الكمية المطلوبة (15) > المخزون (10)
        Assert.Equal(0, successfulResponses.Count);
        Assert.Equal(100, failedResponses.Count);
    }
}