using Cdms.Model;
using Cdms.Model.Gvms;
using Cdms.Model.Ipaffs;

namespace Cdms.Backend.Data;

public interface IMongoDbContext
{
    IMongoCollectionSet<ImportNotification> Notifications { get; }
    IMongoCollectionSet<Movement> Movements { get; }

    IMongoCollectionSet<Gmr> Gmrs { get; }

    Task<IMongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default);

    Task ResetCollections(CancellationToken cancellationToken = default);
}