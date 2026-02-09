using Inventory.Application;
using Inventory.Infrastructure;
using Inventory.Infrastructure.Persistence;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// ============================================
// CORS Configuration
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ============================================
// Application Layer
// ============================================
builder.Services.AddApplication();

// ============================================
// Infrastructure Layer
// ============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=InventoryDb;Trusted_Connection=True;MultipleActiveResultSets=true;";

builder.Services.AddInfrastructure(connectionString);

// ============================================
// Controllers
// ============================================
builder.Services.AddControllers();

// ============================================
// OpenAPI/Swagger Configuration (.NET 10 syntax)
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventory Reservation API",
        Version = "v1",
        Description = "High-throughput inventory reservation system with concurrency control"
    });
});

var app = builder.Build();

// ============================================
// Middleware Pipeline
// ============================================
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ============================================
// Swagger UI
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Reservation API v1");
    });
}

// ============================================
// Database Initialization
// ============================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Create database if not exists
    await dbContext.Database.EnsureCreatedAsync();

    // Seed test data if database is empty
    if (!await dbContext.Products.AnyAsync())
    {
        dbContext.Products.Add(Inventory.Domain.Entities.Product.Create(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Flash Sale Item",
            100
        ));

        await dbContext.SaveChangesAsync();
        Console.WriteLine("✅ Database seeded with test product");
    }
    else
    {
        Console.WriteLine("✅ Database already has data");
    }
}

// ============================================
// Run Application
// ============================================
Console.WriteLine("🚀 Inventory Reservation API is running...");
Console.WriteLine($"📖 Swagger UI: https://localhost:{app.Environment.ApplicationName}/swagger");

app.Run();