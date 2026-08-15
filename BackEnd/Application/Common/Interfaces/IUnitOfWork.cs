namespace Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }

    public interface IAppTransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
