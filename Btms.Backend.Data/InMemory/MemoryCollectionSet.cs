using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Btms.Model.Data;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Btms.Backend.Data.InMemory;

public class MemoryCollectionSet<T> : IMongoCollectionSet<T> where T : IDataEntity
{
    private readonly List<T> _data = [];

    private IQueryable<T> EntityQueryable => _data.AsQueryable();

    public IEnumerator<T> GetEnumerator()
    {
        return _data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Type ElementType => EntityQueryable.ElementType;
    public Expression Expression => EntityQueryable.Expression;
    public IQueryProvider Provider => EntityQueryable.Provider;
    public Task<T?> Find(string id)
    {
        return Task.FromResult(_data.Find(x => x.Id == id));
    }

    public Task<T?> Find(Expression<Func<T, bool>> query)
    {
        return Task.FromResult(_data.Find(i => query.Compile()(i)));
    }

    public Task Insert(T item, IMongoDbTransaction transaction = default!, CancellationToken cancellationToken = default)
    {
        item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
        item.Created = DateTime.UtcNow;
        item.Updated = DateTime.UtcNow;
        
        _data.Add(item);
        
        return Task.CompletedTask;
    }

    [SuppressMessage("SonarLint", "S2955",
        Justification =
            "IEquatable<T> would need to be implemented on every data entity just to stop sonar complaining about a null check. Nope.")]
    public Task Update(T item, bool setUpdated = true, IMongoDbTransaction transaction = default!,
        CancellationToken cancellationToken = default)
    {
        return Update(item, item._Etag, setUpdated, transaction, cancellationToken);
    }

    [SuppressMessage("SonarLint", "S2955",
        Justification =
            "IEquatable<T> would need to be implemented on every data entity just to stop sonar complaining about a null check. Nope.")]
    public Task Update(T item, string etag, bool setUpdated = true, IMongoDbTransaction transaction = default!, CancellationToken cancellationToken = default)
    {
        var existingItem = _data.Find(x => x.Id == item.Id);
        if (existingItem == null) return Task.CompletedTask;
        
        if (existingItem._Etag != etag)
        {
            throw new ConcurrencyException(item.Id!, etag);
        }

        item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
        
        if (setUpdated)
            item.Updated = DateTime.UtcNow;
        
        _data[_data.IndexOf(existingItem)] = item;
        
        return Task.CompletedTask;
    }

    public IAggregateFluent<T> Aggregate()
    {
        throw new NotImplementedException();
    }
}