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
                Builders<ImportNotification>.IndexKeys.Ascending(n => n._MatchReference), cancellationToken),
            CreateIndex("Created",
                Builders<ImportNotification>.IndexKeys.Ascending(n => n.Created), cancellationToken),
            CreateIndex("CreatedSource",
                Builders<ImportNotification>.IndexKeys.Ascending(n => n.CreatedSource), cancellationToken),
            CreateIndex("ImportNotificationGmrLinker",
                Builders<ImportNotification>.IndexKeys
                    .Ascending(new StringFieldDefinition<ImportNotification>("externalReferences.system"))
                    .Ascending(new StringFieldDefinition<ImportNotification>("externalReferences.reference")), cancellationToken),

            CreateIndex("MatchReferenceIdx",
                Builders<Movement>.IndexKeys.Ascending(m => m._MatchReferences), cancellationToken),
            CreateIndex("Created",
                Builders<Movement>.IndexKeys.Ascending(m => m.Created), cancellationToken),
            CreateIndex("CreatedSource",
                Builders<Movement>.IndexKeys.Ascending(m => m.CreatedSource), cancellationToken)

        );

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