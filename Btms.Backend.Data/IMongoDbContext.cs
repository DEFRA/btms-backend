using Btms.Model;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;

namespace Btms.Backend.Data;

public interface IMongoDbContext
{
    IMongoCollectionSet<ImportNotification> Notifications { get; }
    IMongoCollectionSet<Movement> Movements { get; }

    IMongoCollectionSet<Gmr> Gmrs { get; }

    Task<IMongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default);

    Task ResetCollections(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(bool useTransaction = true, CancellationToken cancellation = default);
}