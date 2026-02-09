using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Repositories;
using Inventory.Application.Events; 
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Repositories;
using Inventory.Infrastructure.BackgroundServices;
using Inventory.Infrastructure.Messaging;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // قاعدة البيانات
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IProductRepository, EfProductRepository>();
        services.AddScoped<IReservationRepository, EfReservationRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddHostedService<ReservationExpirationService>();
        // Event Publisher مع Polly
        services.AddScoped<IEventPublisher, ResilientEventPublisher>();

        return services;

    }
}