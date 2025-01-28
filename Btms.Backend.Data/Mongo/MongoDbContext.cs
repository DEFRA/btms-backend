using Btms.Model;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Btms.Backend.Data.Mongo;

public class MongoDbContext(IMongoDatabase database, TimeProvider timeProvider, ILoggerFactory loggerFactory) : IMongoDbContext
{
    internal IMongoDatabase Database { get; } = database;
    internal TimeProvider TimeProvider { get; } = timeProvider;
    internal MongoDbTransaction? ActiveTransaction { get; private set; }


    public IMongoCollectionSet<ImportNotification> Notifications => new MongoCollectionSet<ImportNotification>(this);

    public IMongoCollectionSet<Movement> Movements => new MongoCollectionSet<Movement>(this);

    public IMongoCollectionSet<Gmr> Gmrs => new MongoCollectionSet<Gmr>(this);

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

        await new MongoIndexService(Database, loggerFactory.CreateLogger<MongoIndexService>()).StartAsync(cancellationToken);
    }
}