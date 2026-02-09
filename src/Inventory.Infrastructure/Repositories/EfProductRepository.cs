using Inventory.Application.Repositories;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class EfProductRepository(ApplicationDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Products.FindAsync(new object[] { id }, cancellationToken);
    }

    // ⚠️ النقطة الأهم: قفل الصف أثناء القراءة لمنع السباق
    public async Task<Product?> GetByIdWithLockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // استخدام ROWLOCK و UPDLOCK لمنع عمليات القراءة المتزامنة من تجاوز بعضها
        return await context.Products
            .FromSqlRaw("SELECT * FROM Products WITH (ROWLOCK, UPDLOCK, READPAST) WHERE Id = {0}", id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}