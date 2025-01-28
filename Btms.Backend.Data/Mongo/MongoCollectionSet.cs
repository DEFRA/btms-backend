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
    private readonly IMongoCollection<T> _collection = string.IsNullOrEmpty(collectionName)
        ? dbContext.Database.GetCollection<T>(typeof(T).Name)
        : dbContext.Database.GetCollection<T>(collectionName);

    private IMongoQueryable<T> EntityQueryable => _collection.AsQueryable();
    
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

    public Task Insert(T item, IMongoDbTransaction? transaction, CancellationToken cancellationToken = default)
    {
        item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
        item.Created = DateTime.UtcNow;
        item.Updated = DateTime.UtcNow;
        
        var session = transaction is null ? dbContext.ActiveTransaction?.Session : transaction.Session;
        
        return session is not null
            ? _collection.InsertOneAsync(session, item, cancellationToken: cancellationToken)
            : _collection.InsertOneAsync(item, cancellationToken: cancellationToken);
    }

    public async Task Update(T item, bool setUpdated = true, IMongoDbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        await Update(item, item._Etag, setUpdated, transaction, cancellationToken);
    }

    public async Task Update(T item, string etag, bool setUpdated = true, IMongoDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(etag);
        var builder = Builders<T>.Filter;
        var filter = builder.Eq(x => x.Id, item.Id) & builder.Eq(x => x._Etag, etag);

        item._Etag = BsonObjectIdGenerator.Instance.GenerateId(null, null).ToString()!;
        item.Updated = DateTime.UtcNow;
        
        var session = transaction is null ? dbContext.ActiveTransaction?.Session : transaction.Session;
        var updateResult = session is not null
            ? await _collection.ReplaceOneAsync(session, filter, item,
                cancellationToken: cancellationToken)
            : await _collection.ReplaceOneAsync(filter, item,
                cancellationToken: cancellationToken);

        if (updateResult.ModifiedCount == 0)
        {
            throw new ConcurrencyException(item.Id!, etag);
        }
    }

    public IAggregateFluent<T> Aggregate()
    {
        return _collection.Aggregate();
    }
}