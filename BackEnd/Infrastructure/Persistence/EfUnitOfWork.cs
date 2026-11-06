using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace OnlineShop.Infrastructure.Persistence
{
    public sealed class EfUnitOfWork(AppDbContext context) : IUnitOfWork
    {
        public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            // Excel import runs each row's command inside its own transaction and savepoint:
            // a command that starts one there takes part in it, and the outer owner decides.
            if (context.Database.CurrentTransaction is not null)
                return new JoinedTransaction();

            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            return new EfAppTransaction(transaction);
        }

        private sealed class JoinedTransaction : IAppTransaction
        {
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
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
