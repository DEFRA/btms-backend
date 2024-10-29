using Cdms.Model;
using Cdms.Model.Ipaffs;
using Cdms.Model.VehicleMovement;
using MongoDB.Driver;

namespace Cdms.Backend.Data;

public interface IMongoDbContext
{
    MongoCollectionSet<ImportNotification> Notifications { get; }
    MongoCollectionSet<Movement> Movements { get; }

    MongoCollectionSet<Gmr> Gmrs { get; }
    Task<MongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default);
}

public class MongoDbContext(IMongoDatabase database) : IMongoDbContext
{
    internal IMongoDatabase Database { get; private set; } = database;
    internal MongoDbTransaction ActiveTransaction { get; private set; }


    public MongoCollectionSet<ImportNotification> Notifications => new(this);

    public MongoCollectionSet<Movement> Movements => new(this);

    public MongoCollectionSet<Gmr> Gmrs => new(this);

    public async Task<MongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default)
    {
        var session = await Database.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();
        ActiveTransaction = new MongoDbTransaction(session);
        return ActiveTransaction;
    }
}