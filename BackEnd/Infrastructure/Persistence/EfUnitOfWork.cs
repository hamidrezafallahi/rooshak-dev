using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace OnlineShop.Infrastructure.Persistence
{
    public sealed class EfUnitOfWork(AppDbContext context) : IUnitOfWork
    {
        public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            return new EfAppTransaction(transaction);
        }

        private sealed class EfAppTransaction(IDbContextTransaction transaction) : IAppTransaction
        {
            private bool _completed;

            public async Task CommitAsync(CancellationToken cancellationToken = default)
            {
                await transaction.CommitAsync(cancellationToken);
                _completed = true;
            }

            public async Task RollbackAsync(CancellationToken cancellationToken = default)
            {
                if (_completed)
                    return;

                await transaction.RollbackAsync(cancellationToken);
                _completed = true;
            }

            public async ValueTask DisposeAsync()
            {
                if (!_completed)
                    await transaction.RollbackAsync();

                await transaction.DisposeAsync();
            }
        }
    }
}
