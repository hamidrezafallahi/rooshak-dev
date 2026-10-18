namespace Application.Common.Interfaces
{
    /// <summary>
    /// Transaction with per-item savepoints for bulk operations (e.g. Excel import): one failing item
    /// is rolled back to its savepoint without aborting the whole transaction.
    /// </summary>
    public interface IBulkOperationSession
    {
        Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default);

        Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>Rolls the database back to the savepoint and discards pending tracked changes.</summary>
        Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default);
    }
}
