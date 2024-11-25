using Cdms.Model;
using Cdms.Model.Ipaffs;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Cdms.Backend.Data.Mongo;

public class MongoIndexService(IMongoDatabase database) : IHostedService
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

    private Task CreateIndex<T>(string name, IndexKeysDefinition<T> keys, CancellationToken cancellationToken)
    {
        var indexModel = new CreateIndexModel<T>(keys,
            new CreateIndexOptions()
            {
                Name = name,
                Background = true,
            });
        return database.GetCollection<T>(typeof(T).Name).Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
    }
}