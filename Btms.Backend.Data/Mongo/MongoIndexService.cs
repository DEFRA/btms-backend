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
                Builders<ImportNotification>.IndexKeys.Ascending(n => n._MatchReference), cancellationToken: cancellationToken),
            CreateIndex("Created",
                Builders<ImportNotification>.IndexKeys.Ascending(n => n.Created), cancellationToken: cancellationToken),
            CreateIndex("CreatedSource",
                Builders<ImportNotification>.IndexKeys.Ascending(n => n.CreatedSource), cancellationToken: cancellationToken),
            CreateIndex("UpdatedEntity",
                Builders<ImportNotification>.IndexKeys.Ascending(n => n.UpdatedEntity), cancellationToken: cancellationToken),

            CreateIndex("MatchReferenceIdx",
                Builders<Movement>.IndexKeys.Ascending(m => m._MatchReferences), cancellationToken: cancellationToken),
            CreateIndex("Created",
                Builders<Movement>.IndexKeys.Ascending(m => m.Created), cancellationToken: cancellationToken),
            CreateIndex("CreatedSource",
                Builders<Movement>.IndexKeys.Ascending(m => m.CreatedSource), cancellationToken: cancellationToken),
            CreateIndex("UniqueDecisionNumber",
                Builders<Movement>.IndexKeys.Ascending(m => m.EntryReference)
                    .Ascending(new StringFieldDefinition<Movement>("decisions.header.decisionNumber")), true, cancellationToken: cancellationToken)

        );

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task CreateIndex<T>(string name, IndexKeysDefinition<T> keys, bool unique = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var indexModel = new CreateIndexModel<T>(keys,
                new CreateIndexOptions
                {
                    Name = name,
                    Background = true,
                    Unique = unique,
                });
            await database.GetCollection<T>(typeof(T).Name).Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to Create index {Name} on {Collection}", name, typeof(T).Name);
        }

    }
}