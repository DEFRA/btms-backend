using System.Collections;
using System.Linq.Expressions;
using Cdms.Model.Data;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Cdms.Backend.Data.InMemory;

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
    public Task<T> Find(string id)
    {
        return Task.FromResult(data.Find(x => x.Id == id))!;
    }

    public Task Insert(T item, IMongoDbTransaction transaction = default!, CancellationToken cancellationToken = default)
    {
        item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
        data.Add(item);
        return Task.CompletedTask;
    }

    public Task Update(T item, string etag, IMongoDbTransaction transaction = default!, CancellationToken cancellationToken = default)
    {
        item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;

        var existingItem = data.Find(x => x.Id == item.Id);

        if (existingItem?._Etag != etag)
        {
            throw new ConcurrencyException("Concurrency Error, change this to a Concurrency exception");
        }

        data[data.IndexOf(existingItem)] = item;
        return Task.CompletedTask;
    }
}