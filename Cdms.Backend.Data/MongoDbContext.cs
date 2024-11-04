using Cdms.Model;
using Cdms.Model.Gvms;
using Cdms.Model.Ipaffs;
using MongoDB.Driver;

namespace Cdms.Backend.Data;

public interface IMongoDbContext
{
    MongoCollectionSet<ImportNotification> Notifications { get; }
    MongoCollectionSet<Movement> Movements { get; }

    MongoCollectionSet<Gmr> Gmrs { get; }

    MongoCollectionSet<Inbox> Inbox { get; }

    Task<MongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default);
}

public class MongoDbContext(IMongoDatabase database) : IMongoDbContext
{
    internal IMongoDatabase Database { get; private set; } = database;
    internal MongoDbTransaction? ActiveTransaction { get; private set; }


    public MongoCollectionSet<ImportNotification> Notifications => new(this, nameof(Notifications));

    public MongoCollectionSet<Movement> Movements => new(this, nameof(Movements));

    public MongoCollectionSet<Gmr> Gmrs => new(this, nameof(Gmrs));

    public MongoCollectionSet<Inbox> Inbox => new(this, nameof(Inbox));

    public async Task<MongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default)
    {
        var session = await Database.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();
        ActiveTransaction = new MongoDbTransaction(session);
        return ActiveTransaction;
    }
}