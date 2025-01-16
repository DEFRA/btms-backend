using System.Linq.Expressions;
using Btms.Model.Data;
using MongoDB.Driver;

namespace Btms.Backend.Data;

public interface IMongoCollectionSet<T> : IQueryable<T> where T : IDataEntity
{
    Task<T?> Find(string id);
    Task<T?> Find(Expression<Func<T, bool>> query);
    
    Task Insert(T item, IMongoDbTransaction transaction = default!, CancellationToken cancellationToken = default);

    Task Update(T item, string? etag = default, IMongoDbTransaction transaction = default!,
        CancellationToken cancellationToken = default);
    
    IAggregateFluent<T> Aggregate();
}