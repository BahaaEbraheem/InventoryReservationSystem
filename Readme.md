# Inventory Reservation System

High-throughput inventory reservation engine for flash sales with concurrency control.

## Architecture
- **Domain Layer**: Business logic & entities (Product, Reservation)
- **Application Layer**: Use cases (MediatR commands/handlers)
- **Infrastructure Layer**: EF Core, background services, Polly resilience
- **API Layer**: REST endpoints with Swagger

## Concurrency Strategy
✅ **Database Row-Level Locking** with `WITH (ROWLOCK, UPDLOCK, READPAST)`  
✅ **ACID Transactions** via EF Core  
✅ **Optimistic Concurrency** fallback with row versioning  
✅ **Background Service** for automatic reservation expiration (every 30s)

## How to Run

### Prerequisites
- .NET 10 SDK
- SQL Server LocalDB (or Docker)

### Steps
```bash
# Restore dependencies
dotnet restore

# Run API (auto-creates database)
dotnet run --project src/Inventory.API