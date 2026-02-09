using Inventory.Domain.Entities;
using Inventory.Application.Repositories;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class EfProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public EfProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Product?> GetByIdWithLockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FromSqlRaw("SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id = {0}", id)
            .AsTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }
}
