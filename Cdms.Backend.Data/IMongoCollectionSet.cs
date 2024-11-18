using Cdms.Model.Data;

namespace Cdms.Backend.Data;

public interface IMongoCollectionSet<T> : IQueryable<T> where T : IDataEntity
{
    Task<T> Find(string id);
    Task Insert(T item, IMongoDbTransaction transaction = null, CancellationToken cancellationToken = default);

    Task Update(T item, string etag, IMongoDbTransaction transaction = null,
        CancellationToken cancellationToken = default);
}