using Btms.Backend.Data;
using Btms.Backend.Data.InMemory;

namespace Btms.Consumers.Tests;

public abstract class ConsumerTests
{
    protected static IMongoDbContext CreateDbContext()
    {
        return new MemoryMongoDbContext();
    }
}