using Btms.Model.Data;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections;
using System.Linq.Expressions;

namespace Btms.Backend.Data.Mongo;

public class MongoCollectionSet<T>(MongoDbContext dbContext, string collectionName = null!)
    : IMongoCollectionSet<T> where T : IDataEntity
{
    private readonly IMongoCollection<T> collection = string.IsNullOrEmpty(collectionName)
        ? dbContext.Database.GetCollection<T>(typeof(T).Name)
        : dbContext.Database.GetCollection<T>(collectionName);

    private readonly List<T> _entitiesToInsert = [];
    private readonly List<(T Item, string Etag)> _entitiesToUpdate = [];

    private IMongoQueryable<T> EntityQueryable => collection.AsQueryable();
        
    public IEnumerator<T> GetEnumerator()
    {
        return EntityQueryable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return EntityQueryable.GetEnumerator();
    }

    public Type ElementType => EntityQueryable.ElementType;
    public Expression Expression => EntityQueryable.Expression;
    public IQueryProvider Provider => EntityQueryable.Provider;

    public async Task<T?> Find(string id)
    {
        return await EntityQueryable.SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<T?> Find(Expression<Func<T, bool>> query)
    {
        return await EntityQueryable.FirstOrDefaultAsync(query);
    }

    public async Task PersistAsync(CancellationToken cancellationToken)
    {
        if (_entitiesToInsert.Any())
        {
            foreach (var item in _entitiesToInsert)
            {
                item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
                item.Created = DateTime.UtcNow;
                item.UpdatedEntity = DateTime.UtcNow;
                await collection.InsertOneAsync(dbContext.ActiveTransaction?.Session, item, cancellationToken: cancellationToken);
            }

            _entitiesToInsert.Clear();
        }

        var builder = Builders<T>.Filter;

        if (_entitiesToUpdate.Any())
        {
            foreach (var item in _entitiesToUpdate)
            {
                var filter = builder.Eq(x => x.Id, item.Item.Id) & builder.Eq(x => x._Etag, item.Etag);

                item.Item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
                item.Item.UpdatedEntity = DateTime.UtcNow;
                var session = dbContext.ActiveTransaction?.Session;
                var updateResult = session is not null
                    ? await collection.ReplaceOneAsync(session, filter, item.Item,
                        cancellationToken: cancellationToken)
                    : await collection.ReplaceOneAsync(filter, item.Item,
                        cancellationToken: cancellationToken);

                if (updateResult.ModifiedCount == 0)
                {
                    throw new ConcurrencyException(item.Item.Id!, item.Etag);
                }
            }

            _entitiesToUpdate.Clear();
        }
    }

    public Task Insert(T item, CancellationToken cancellationToken = default)
    {
        _entitiesToInsert.Add(item);
        return Task.CompletedTask;
    }

    public async Task Update(T item, CancellationToken cancellationToken = default)
    {
        await Update(item, item._Etag, cancellationToken);
    }

    public Task Update(T item, string etag,  CancellationToken cancellationToken = default)
    {
        if (_entitiesToInsert.Exists(x => x.Id == item.Id))
        {
            return Task.CompletedTask;
        }

        ArgumentNullException.ThrowIfNull(etag);
        _entitiesToUpdate.RemoveAll(x => x.Item.Id == item.Id);
        _entitiesToUpdate.Add(new ValueTuple<T, string>(item, etag));
        return Task.CompletedTask;
    }

    public IAggregateFluent<T> Aggregate()
    {
        return collection.Aggregate();
    }
}