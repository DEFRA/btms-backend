using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;

namespace Btms.Backend.Data.Healthcheck;

public class MongoDbHealthCheck(MongoClientSettings clientSettings, string? databaseName = default)
    : IHealthCheck
{
    private static readonly BsonDocumentCommand<BsonDocument> Command = new(BsonDocument.Parse("{ping:1}"));
    private static readonly ConcurrentDictionary<string, IMongoClient> MongoClient = new();
    private readonly MongoClientSettings _mongoClientSettings = clientSettings;
    private readonly string? _specifiedDatabase = databaseName;

    public MongoDbHealthCheck(string connectionString, string? databaseName = default)
        : this(MongoClientSettings.FromUrl(MongoUrl.Create(connectionString)), databaseName)
    {
        if (databaseName == default)
        {
            _specifiedDatabase = MongoUrl.Create(connectionString)?.DatabaseName;
        }
    }

    public MongoDbHealthCheck(IMongoClient client, string? databaseName = default)
        : this(client.Settings, databaseName)
    {
        MongoClient[_mongoClientSettings.ToString()] = client;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var mongoClient = MongoClient.GetOrAdd(_mongoClientSettings.ToString(),
                _ => new MongoClient(_mongoClientSettings));

            if (!string.IsNullOrEmpty(_specifiedDatabase))
            {
                // some users can't list all databases depending on database privileges, with
                // this you can check a specified database.
                // Related with issue #43 and #617

                await mongoClient
                    .GetDatabase(_specifiedDatabase)
                    .RunCommandAsync(Command, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                using var cursor =
                    await mongoClient.ListDatabaseNamesAsync(cancellationToken).ConfigureAwait(false);
                await cursor.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}