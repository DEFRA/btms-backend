using Btms.Model;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Btms.Model.Validation;

namespace Btms.Backend.Data;

public interface IMongoDbContext
{
    IMongoCollectionSet<ImportNotification> Notifications { get; }
    IMongoCollectionSet<Movement> Movements { get; }

    IMongoCollectionSet<Gmr> Gmrs { get; }

    IMongoCollectionSet<ValidationErrorEntity> ValidationErrors { get; }

    Task<IMongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default);

    Task ResetCollections(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellation = default);
}