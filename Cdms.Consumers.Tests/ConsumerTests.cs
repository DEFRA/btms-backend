using Cdms.Backend.Data;
using Cdms.Backend.Data.InMemory;

namespace Cdms.Consumers.Tests;

public abstract class ConsumerTests
{
    protected static IMongoDbContext CreateDbContext()
    {
        return new MemoryMongoDbContext();
    }
}