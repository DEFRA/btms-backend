using Cdms.Model;
using Cdms.Model.Gvms;
using Cdms.Model.Ipaffs;

namespace Cdms.Backend.Data.InMemory;

public class MemoryMongoDbContext : IMongoDbContext
{
    public IMongoCollectionSet<ImportNotification> Notifications { get; } = new MemoryCollectionSet<ImportNotification>();
    public IMongoCollectionSet<Movement> Movements { get; } = new MemoryCollectionSet<Movement>();
    public IMongoCollectionSet<Gmr> Gmrs { get; } = new MemoryCollectionSet<Gmr>();
    public Task<IMongoDbTransaction> StartTransaction(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IMongoDbTransaction>(new EmptyMongoDbTransaction());
    }
}