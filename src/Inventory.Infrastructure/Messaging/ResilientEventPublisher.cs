using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Inventory.Application.Events;

namespace Inventory.Infrastructure.Messaging;

public class ResilientEventPublisher : IEventPublisher
{
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<ResilientEventPublisher> _logger;

    public ResilientEventPublisher(ILogger<ResilientEventPublisher> logger)
    {
        _logger = logger;

        // استراتيجية إعادة المحاولة: 3 محاولات مع تأخير أسي
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, delay, attempt, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Event publishing failed. Retry {Attempt}/3 in {Delay}s",
                        attempt, delay.TotalSeconds);
                });
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            // في بيئة حقيقية: نشر الحدث عبر RabbitMQ/Kafka
            // هنا نُسجل فقط لأغراض العرض
            _logger.LogInformation("Published event: {@Event}", @event);

            // محاكاة فشل عشوائي لاختبار Polly (10% فرصة)
            if (Random.Shared.Next(100) < 10)
                throw new Exception("Simulated transient failure");
        });
    }
}