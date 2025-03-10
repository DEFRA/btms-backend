using Btms.Model;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Btms.Model.Validation;

namespace Btms.Backend.Data.InMemory;

public class MemoryMongoDbContext : IMongoDbContext
{
    public IMongoCollectionSet<ImportNotification> Notifications { get; } = new MemoryCollectionSet<ImportNotification>();
    public IMongoCollectionSet<Movement> Movements { get; } = new MemoryCollectionSet<Movement>();
    public IMongoCollectionSet<Gmr> Gmrs { get; } = new MemoryCollectionSet<Gmr>();

    public IMongoCollectionSet<ValidationErrorEntity> ValidationErrors { get; } = new MemoryCollectionSet<ValidationErrorEntity>();
    public Task<IMongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IMongoDbTransaction>(new EmptyMongoDbTransaction());
    }

    public Task ResetCollections(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellation = default)
    {
        return Task.CompletedTask;
    }
}