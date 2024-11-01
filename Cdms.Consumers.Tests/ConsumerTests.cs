using Cdms.Backend.Data;
using MongoDB.Driver;

namespace Cdms.Consumers.Tests;

public abstract class ConsumerTests
{
    protected MongoDbContext CreateDbContext()
    {
        var client = new MongoClient(MongoRunnerProvider.Instance.Get().ConnectionString);
        var db = client.GetDatabase($"Cdms_MongoDb_{Random.Shared.Next()}_Test");
        return new MongoDbContext(db);
    }
}