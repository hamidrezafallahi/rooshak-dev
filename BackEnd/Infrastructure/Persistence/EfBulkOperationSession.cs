using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OnlineShop.Infrastructure.Persistence
{
    public sealed class EfBulkOperationSession(AppDbContext context) : IBulkOperationSession
    {
        public Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
            new EfUnitOfWork(context).BeginTransactionAsync(cancellationToken);

        public Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default) =>
            Current.CreateSavepointAsync(name, cancellationToken);

        public Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default) =>
            Current.ReleaseSavepointAsync(name, cancellationToken);

        public async Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
        {
            await Current.RollbackToSavepointAsync(name, cancellationToken);
            // Entities modified by the failed item must not leak into the next SaveChanges.
            context.ChangeTracker.Clear();
        }

        private IDbContextTransaction Current =>
            context.Database.CurrentTransaction
            ?? throw new InvalidOperationException("No active transaction; call BeginTransactionAsync first.");
    }
}
