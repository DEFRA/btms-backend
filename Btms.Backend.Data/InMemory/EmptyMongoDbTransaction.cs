using MongoDB.Driver;

namespace Btms.Backend.Data.InMemory;

public class EmptyMongoDbTransaction : IMongoDbTransaction
{
    public IClientSessionHandle Session => null!;

    public Task CommitTransaction(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RollbackTransaction(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}