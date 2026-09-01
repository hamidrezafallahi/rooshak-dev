using Application.Dtos;
using Application.Queries;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests;

public class PaymentQueryHandlersTests
{
    [Fact]
    public async Task GetAllPaymentsQuery_returns_paginated_payment_rows()
    {
        var repo = new FakePaymentRepository(
        [
            PaymentFactory.Create(1, 200_000m, 1, 1),
            PaymentFactory.Create(2, 350_000m, 1, 1),
            PaymentFactory.Create(3, 500_000m, 1, 1),
            PaymentFactory.Create(4, 900_000m, 1, 1)
        ]);

        var handler = new PaymentQueryHandlers(repo, new FakeEntityConfigRepository());
        var result = await handler.Handle(new GetAllPaymentsQuery { page = 1, pageSize = 2 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(4, result.Data.TotalCount);
        Assert.Equal(2, result.Data.Records.Count);
        Assert.Equal(2, result.Data.PageSize);
    }

    [Fact]
    public async Task GetAllPaymentsQuery_filters_by_transaction_id_when_q_is_provided()
    {
        var repo = new FakePaymentRepository(
        [
            PaymentFactory.Create(1, 200_000m, 1, 1),
            PaymentFactory.Create(2, 350_000m, 1, 1)
        ]);

        var first = repo.Items[0];
        first.SetTransactionId("TRX-ALPHA", 1);
        var second = repo.Items[1];
        second.SetTransactionId("TRX-BETA", 1);

        var handler = new PaymentQueryHandlers(repo, new FakeEntityConfigRepository());
        var result = await handler.Handle(new GetAllPaymentsQuery { Q = "ALPHA", page = 1, pageSize = 10 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data.Records);
        Assert.Equal("TRX-ALPHA", result.Data.Records[0].TransactionId);
    }

    [Fact]
    public async Task GetPaymentByIdQuery_returns_the_matching_payment()
    {
        var repo = new FakePaymentRepository(
        [
            PaymentFactory.Create(1, 200_000m, 1, 1),
            PaymentFactory.Create(2, 350_000m, 1, 1)
        ]);

        repo.Items[1].SetTransactionId("TRX-102", 1);
        var handler = new PaymentQueryHandlers(repo, new FakeEntityConfigRepository());

        var result = await handler.Handle(new GetPaymentByIdQuery { Id = 2 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Id);
        Assert.Equal("TRX-102", result.Data.TransactionId);
    }

    private static class PaymentFactory
    {
        public static Payment Create(int id, decimal amount, int orderId, int userId)
        {
            var payment = Payment.Create(orderId, amount, 1, userId);
            typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(payment, id);
            return payment;
        }
    }

    private sealed class FakePaymentRepository(List<Payment> items) : IPaymentRepository
    {
        public List<Payment> Items { get; } = items;

        public Task<List<Payment>> GetAllAsync(Expression<Func<Payment, bool>>? predicate = null) =>
            Task.FromResult(predicate == null ? Items : Items.Where(predicate.Compile()).ToList());

        public Task<Payment?> GetByIdAsync(int id) =>
            Task.FromResult(Items.FirstOrDefault(p => p.Id == id));

        public Task<List<Payment>> GetByOrderIdAsync(int orderId) =>
            Task.FromResult(Items.Where(p => p.OrderId == orderId).ToList());

        public Task<Payment?> GetByTransactionIdAsync(string transactionId) =>
            Task.FromResult(Items.FirstOrDefault(p => p.TransactionId == transactionId));

        public IQueryable<Payment> Query(Expression<Func<Payment, bool>>? predicate = null)
        {
            IQueryable<Payment> result = new AsyncEnumerableQuery<Payment>(Items);
            if (predicate != null) result = result.Where(predicate);
            return result;
        }

        public Task AddAsync(Payment entity) { Items.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(Payment entity) => Task.CompletedTask;
        public Task<bool> DeleteAsync(int id) { var item = Items.FirstOrDefault(p => p.Id == id); if (item == null) return Task.FromResult(false); Items.Remove(item); return Task.FromResult(true); }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task<List<int>> GetAllIds() => Task.FromResult(Items.Select(p => p.Id).ToList());
    }

    private sealed class FakeEntityConfigRepository : IEntityConfigRepository
    {
        public Task<EntityConfig?> GetByEntityNameAsync(string entityName) => Task.FromResult<EntityConfig?>(null);
        public Task<FormReadModel> GetFormByUrlAsync() => Task.FromResult(new FormReadModel());
        public Task<EntityConfig?> GetByIdAsync(int id) => Task.FromResult<EntityConfig?>(null);
        public Task<List<EntityConfig>> GetAllAsync(Expression<Func<EntityConfig, bool>>? predicate = null) => Task.FromResult(new List<EntityConfig>());
        public IQueryable<EntityConfig> Query(Expression<Func<EntityConfig, bool>>? predicate = null)
        {
            IQueryable<EntityConfig> result = new AsyncEnumerableQuery<EntityConfig>(Enumerable.Empty<EntityConfig>());
            if (predicate != null) result = result.Where(predicate);
            return result;
        }
        public Task AddAsync(EntityConfig entity) => Task.CompletedTask;
        public Task UpdateAsync(EntityConfig entity) => Task.CompletedTask;
        public Task<bool> DeleteAsync(int id) => Task.FromResult(true);
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task<List<int>> GetAllIds() => Task.FromResult(new List<int>());
    }

    private sealed class AsyncEnumerableQuery<T> : IQueryable<T>, IAsyncEnumerable<T>, IAsyncQueryProvider
    {
        private readonly IQueryable<T> _source;

        public AsyncEnumerableQuery(IEnumerable<T> source)
        {
            _source = source.AsQueryable();
        }

        public Type ElementType => _source.ElementType;
        public Expression Expression => _source.Expression;
        public IQueryProvider Provider => this;

        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new AsyncEnumeratorWrapper<T>(_source.GetEnumerator());

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments().FirstOrDefault() ?? typeof(T);
            var enumerableType = typeof(AsyncEnumerableQuery<>).MakeGenericType(elementType);
            var items = _source.Provider.CreateQuery(expression);
            return (IQueryable)Activator.CreateInstance(enumerableType, items)!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var items = _source.Provider.CreateQuery<TElement>(expression);
            return new AsyncEnumerableQuery<TElement>(items);
        }

        public object? Execute(Expression expression) => _source.Provider.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => _source.Provider.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var result = Execute(expression);
            if (result is TResult typed) return typed;

            var genericTaskType = typeof(TResult);
            if (genericTaskType.IsGenericType && genericTaskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var itemType = genericTaskType.GetGenericArguments()[0];
                var fromResultMethod = typeof(Task).GetMethods()
                    .Single(m => m.Name == nameof(Task.FromResult) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1)
                    .MakeGenericMethod(itemType);

                return (TResult)fromResultMethod.Invoke(null, [result])!;
            }

            return (TResult)result!;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator() => GetAsyncEnumerator(CancellationToken.None);

        private sealed class AsyncEnumeratorWrapper<TItem>(IEnumerator<TItem> enumerator) : IAsyncEnumerator<TItem>
        {
            public TItem Current => enumerator.Current;

            public ValueTask DisposeAsync() { enumerator.Dispose(); return ValueTask.CompletedTask; }
            public ValueTask<bool> MoveNextAsync() => new(enumerator.MoveNext());
        }
    }
}
