using MongoDB.Driver;

namespace Cdms.Backend.Data.Mongo;

public class MongoDbTransaction(IClientSessionHandle session) : IMongoDbTransaction
{
    public IClientSessionHandle Session { get; private set; } = session;

    public Task CommitTransaction(CancellationToken cancellationToken = default)
    {
        return Session.CommitTransactionAsync(cancellationToken);
    }

    public Task RollbackTransaction(CancellationToken cancellationToken = default)
    {
        return Session.AbortTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && Session != null)
        {
            Session.Dispose();
        }
    }
}