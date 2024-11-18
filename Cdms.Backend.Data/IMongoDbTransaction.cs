using MongoDB.Driver;

namespace Cdms.Backend.Data;

public interface IMongoDbTransaction : IDisposable
{
    IClientSessionHandle Session { get; }
    Task CommitTransaction(CancellationToken cancellationToken = default);
    Task RollbackTransaction(CancellationToken cancellationToken = default);
}