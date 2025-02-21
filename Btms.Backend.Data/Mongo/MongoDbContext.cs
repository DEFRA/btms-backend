using Btms.Model;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Btms.Backend.Data.Mongo;

public class MongoDbContext : IMongoDbContext
{
    private readonly ILoggerFactory _loggerFactory;
#pragma warning disable S1144
    private readonly Guid _instance = Guid.NewGuid();
#pragma warning restore S1144

    public MongoDbContext(IMongoDatabase database, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        Database = database;
        Notifications = new MongoCollectionSet<ImportNotification>(this);
        Movements = new MongoCollectionSet<Movement>(this);
        Gmrs = new MongoCollectionSet<Gmr>(this);
    }

    internal IMongoDatabase Database { get; }
    internal MongoDbTransaction? ActiveTransaction { get; private set; }


    public IMongoCollectionSet<ImportNotification> Notifications { get; }

    public IMongoCollectionSet<Movement> Movements { get; }

    public IMongoCollectionSet<Gmr> Gmrs { get; }

    public async Task<IMongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default)
    {
        var session = await Database.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();
        ActiveTransaction = new MongoDbTransaction(session);
        return ActiveTransaction;
    }

    public async Task ResetCollections(CancellationToken cancellationToken = default)
    {
        var collections = await (await Database.ListCollectionsAsync(cancellationToken: cancellationToken)).ToListAsync(cancellationToken: cancellationToken);

        foreach (var collection in collections.Where(collection => collection["name"] != "system.profile"))
        {
            await Database.DropCollectionAsync(collection["name"].ToString(), cancellationToken);
        }

        await new MongoIndexService(Database, _loggerFactory.CreateLogger<MongoIndexService>()).StartAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(bool useTransaction = true, CancellationToken cancellation = default)
    {
        if (useTransaction)
        {
            using var transaction = await StartTransaction(cancellation);
            try
            {
                await Notifications.PersistAsync(cancellation);
                await Movements.PersistAsync(cancellation);
                await Gmrs.PersistAsync(cancellation);
                await transaction.CommitTransaction(cancellation);
            }
            catch (Exception)
            {
                await transaction.RollbackTransaction(cancellation);
                throw;
            }
        }
        else
        {
            await Notifications.PersistAsync(cancellation);
            await Movements.PersistAsync(cancellation);
            await Gmrs.PersistAsync(cancellation);
        }
    }
}