using Inventory.API;
using Inventory.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.IntegrationTests;

public class CustomWebApplicationFactory_WithBackground : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // تشغيل BackgroundService أثناء الاختبار
            services.AddHostedService<ReservationExpirationService>();
        });
    }
}
