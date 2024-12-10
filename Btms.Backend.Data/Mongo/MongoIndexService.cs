using Btms.Model;
using Btms.Model.Ipaffs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Btms.Backend.Data.Mongo;

public class MongoIndexService(IMongoDatabase database, ILogger<MongoIndexService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(
            CreateIndex("MatchReferenceIdx",
                Builders<ImportNotification>.IndexKeys.Ascending(m => m._MatchReference), cancellationToken),

            CreateIndex("MatchReferenceIdx",
                Builders<Movement>.IndexKeys.Ascending(m => m._MatchReferences), cancellationToken));

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task CreateIndex<T>(string name, IndexKeysDefinition<T> keys, CancellationToken cancellationToken)
    {
        try
        {
            var indexModel = new CreateIndexModel<T>(keys,
                new CreateIndexOptions
                {
                    Name = name,
                    Background = true,
                });
            await database.GetCollection<T>(typeof(T).Name).Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to Create index {Name} on {Collection}", name, typeof(T).Name);
        }
       
    }
}