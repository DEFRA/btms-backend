using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Btms.Model.Data;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Btms.Backend.Data.InMemory;

public class MemoryCollectionSet<T> : IMongoCollectionSet<T> where T : IDataEntity
{
    private readonly List<T> data = [];

    private IQueryable<T> EntityQueryable => data.AsQueryable();

    public IEnumerator<T> GetEnumerator()
    {
        return data.GetEnumerator();
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
        return Task.FromResult(data.Find(x => x.Id == id));
    }

    public Task<T?> Find(Expression<Func<T, bool>> query)
    {
        return Task.FromResult(data.Find(i => query.Compile()(i)));
    }

    public Task Insert(T item, CancellationToken cancellationToken = default)
    {
        item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
        data.Add(item);
        return Task.CompletedTask;
    }

    [SuppressMessage("SonarLint", "S2955",
        Justification =
            "IEquatable<T> would need to be implemented on every data entity just to stop sonar complaining about a null check. Nope.")]
    public Task Update(T item, CancellationToken cancellationToken = default)
    {
        return Update(item, item._Etag, cancellationToken);
    }

    [SuppressMessage("SonarLint", "S2955",
        Justification =
            "IEquatable<T> would need to be implemented on every data entity just to stop sonar complaining about a null check. Nope.")]
    public Task Update(T item, string etag,CancellationToken cancellationToken = default)
    {
        etag = etag ?? item._Etag;
        
        var existingItem = data.Find(x => x.Id == item.Id);
        if (existingItem == null) return Task.CompletedTask;
        
        if ((existingItem._Etag) != etag)
        {
            throw new ConcurrencyException(item.Id!, etag);
        }

        item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
        data[data.IndexOf(existingItem)] = item;
        return Task.CompletedTask;
    }

    public IAggregateFluent<T> Aggregate()
    {
        throw new NotImplementedException();
    }

    public Task PersistAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}