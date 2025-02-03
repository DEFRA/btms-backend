using System.Linq.Expressions;
using Btms.Model.Data;
using MongoDB.Driver;

namespace Btms.Backend.Data;

public interface IMongoCollectionSet<T> : IQueryable<T> where T : IDataEntity
{
    Task<T?> Find(string id);
    Task<T?> Find(Expression<Func<T, bool>> query);
    
    Task Insert(T item, CancellationToken cancellationToken = default);

    Task Update(T item, CancellationToken cancellationToken = default);
    
    Task Update(T item, string etag, CancellationToken cancellationToken = default);
    
    IAggregateFluent<T> Aggregate();

    Task PersistAsync(CancellationToken cancellationToken);
}