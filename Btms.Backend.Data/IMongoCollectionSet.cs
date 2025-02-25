using System.Linq.Expressions;
using Btms.Model.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Btms.Backend.Data;

public interface IMongoCollectionSet<T> : IQueryable<T> where T : IDataEntity
{
    IQueryable<T> WithHint(string hint);
    Task<T?> Find(string id, CancellationToken cancellationToken = default);
    Task<T?> Find(Expression<Func<T, bool>> query, CancellationToken cancellationToken = default);

    Task Insert(T item, CancellationToken cancellationToken = default);

    Task Update(T item, CancellationToken cancellationToken = default);

    Task Update(List<T> items, CancellationToken cancellationToken = default);

    Task Update(T item, string etag, CancellationToken cancellationToken = default);

    IAggregateFluent<T> Aggregate();

    Task PersistAsync(CancellationToken cancellationToken);
}