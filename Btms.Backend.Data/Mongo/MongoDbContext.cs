using Btms.Common.FeatureFlags;
using Btms.Model;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Btms.Model.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using MongoDB.Driver;

namespace Btms.Backend.Data.Mongo;

public class MongoDbContext : IMongoDbContext
{
    private readonly IFeatureManager _featureManager;
    private readonly ILoggerFactory _loggerFactory;

    public MongoDbContext(IMongoDatabase database, ILoggerFactory loggerFactory, IFeatureManager featureManager)
    {
        _featureManager = featureManager;
        _loggerFactory = loggerFactory;
        Database = database;
        Notifications = new MongoCollectionSet<ImportNotification>(this);
        Movements = new MongoCollectionSet<Movement>(this);
        Gmrs = new MongoCollectionSet<Gmr>(this);
        CdsValidationErrors = new MongoCollectionSet<CdsValidationError>(this);
    }

    internal IMongoDatabase Database { get; }
    internal MongoDbTransaction? ActiveTransaction { get; private set; }


    public IMongoCollectionSet<ImportNotification> Notifications { get; }

    public IMongoCollectionSet<Movement> Movements { get; }

    public IMongoCollectionSet<Gmr> Gmrs { get; }

    public IMongoCollectionSet<CdsValidationError> CdsValidationErrors { get; }

    public async Task<IMongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default)
    {
        var session = await Database.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();
        ActiveTransaction = new MongoDbTransaction(session);
        return ActiveTransaction;
    }

    public async Task ResetCollections(CancellationToken cancellationToken = default)
    {
        var collections =
            await (await Database.ListCollectionsAsync(cancellationToken: cancellationToken)).ToListAsync(
                cancellationToken: cancellationToken);

        foreach (var collection in collections.Where(collection => collection["name"] != "system.profile"))
        {
            await Database.DropCollectionAsync(collection["name"].ToString(), cancellationToken);
        }

        await new MongoIndexService(Database, _loggerFactory.CreateLogger<MongoIndexService>()).StartAsync(
            cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellation = default)
    {
        if (!await _featureManager.IsEnabledAsync(Features.SyncPerformanceEnhancements))
        {
            await InternalSaveChangesAsync(cancellation);
            return;
        }

        if (GetChangedRecordsCount() == 0)
        {
            return;
        }

        if (GetChangedRecordsCount() == 1)
        {
            await InternalSaveChangesAsync(cancellation);
            return;
        }

        using var transaction = await StartTransaction(cancellation);
        try
        {
            await InternalSaveChangesAsync(cancellation);
            await transaction.CommitTransaction(cancellation);
        }
        catch (Exception)
        {
            await transaction.RollbackTransaction(cancellation);
            throw;
        }
    }

    private int GetChangedRecordsCount()
    {
        return Notifications.PendingChanges + Movements.PendingChanges + Gmrs.PendingChanges;
    }

    private async Task InternalSaveChangesAsync(CancellationToken cancellation = default)
    {
        await Notifications.PersistAsync(cancellation);
        await Movements.PersistAsync(cancellation);
        await Gmrs.PersistAsync(cancellation);
        await CdsValidationErrors.PersistAsync(cancellation);
    }
}