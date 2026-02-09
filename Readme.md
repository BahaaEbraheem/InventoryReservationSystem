# Inventory Reservation System

High-throughput inventory reservation engine for flash sales with concurrency control.
# Inventory Reservation Engine

High-throughput inventory reservation system for flash sales with robust concurrency control to prevent over-selling under extreme load.

---

## ✅ Completed Features

- **Concurrency Control**: Database row-level locking (`UPDLOCK` + `ROWLOCK`) prevents race conditions — only one reservation succeeds even with 100+ concurrent requests for the last item
- **Temporary Reservations**: 2-minute hold period with automatic background release via `IHostedService`
- **Resilient Event Publishing**: Polly retry policy (3 attempts, exponential backoff) for transient failure handling
- **Clean Architecture**: Strict separation of concerns across Domain, Application, Infrastructure, and API layers
- **ACID Transactions**: Full transaction support with explicit commit/rollback via `IUnitOfWork`
- **Optimistic Concurrency Fallback**: `RowVersion` column for additional safety
- **Comprehensive Testing**:
  - Unit tests for domain logic (stock reservation/release/validation)
  - Integration test simulating 100 concurrent requests without over-selling
  - Integration test verifying automatic reservation expiration
- **API Documentation**: Swagger/OpenAPI with request/response examples
- **Production-Ready Configuration**: Environment-aware settings (2-min expiration in prod, 5-min in testing)

---

## 📋 Prerequisites

- **.NET 10 SDK**
- **SQL Server** (LocalDB, SQL Server Express, or Docker container)
- **Visual Studio 2022** / **VS Code** / **Rider** (optional)

---

## 🏗️ Architecture

## Architecture
- **Domain Layer**: Business logic & entities (Product, Reservation)
- **Application Layer**: Use cases (MediatR commands/handlers)
- **Infrastructure Layer**: EF Core, background services, Polly resilience
- **API Layer**: REST endpoints with Swagger



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