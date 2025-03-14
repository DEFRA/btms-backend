using Btms.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Btms.Backend.Config;
using Btms.Business.Commands;
using Btms.Consumers;
using Microsoft.AspNetCore.Mvc;
using SlimMessageBus.Host;

namespace Btms.Backend.Endpoints;

public static class ManagementEndpoints
{
    private const string BaseRoute = "mgmt";

    public static void UseManagementEndpoints(this IEndpointRouteBuilder app, IOptions<ApiOptions> options)
    {
        if (options.Value.EnableManagement)
        {
            app.MapGet(BaseRoute + "/collections", GetCollectionsAsync).AllowAnonymous();
            app.MapGet(BaseRoute + "/collections/drop", DropCollectionsAsync).AllowAnonymous();
            app.MapGet(BaseRoute + "/environment", GetEnvironment).AllowAnonymous();
            app.MapGet(BaseRoute + "/status", GetStatus).AllowAnonymous();
            app.MapGet(BaseRoute + "/initialise", Initialise).AllowAnonymous();
            app.MapGet(BaseRoute + "/asb/start", StartAsb).AllowAnonymous();
            app.MapGet(BaseRoute + "/asb/stop", StopAsb).AllowAnonymous();
            app.MapGet(BaseRoute + "/server/forcegc", ForceGC).AllowAnonymous();
        }
    }

    private static string[] _keysToRedact = [
        "Mongo__DatabaseUri",
        "MONGO_URI"
    ];

    private static bool RedactKeys(string key)
    {
        return key.StartsWith("AZURE") ||
               key.StartsWith("BlobServiceOptions__Azure") ||
               key.StartsWith("ReplicationOptions__Azure") ||
               key.Contains("ConnectionString") ||
               key.StartsWith("AuthKeyStore__Credentials") ||
               key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
               _keysToRedact.Contains(key);
    }

    private const string Redacted = "--redacted--";

    private static DictionaryEntry Redact(DictionaryEntry d)
    {

        var value = d.Value;

        try
        {
            switch (d.Key)
            {
                case "HTTP_PROXY" or "HTTPS_PROXY":
                    // redact the password - doesn't have protocol, ie.
                    // btms-backend::passC@proxy.perf-test.cdp-int.defra.cloud
                    value = Redacted;
                    break;
                case "CDP_HTTP_PROXY" or "CDP_HTTPS_PROXY":
                    //  redact the password
                    // https://btms-backend::passC@proxy.perf-test.cdp-int.defra.cloud
                    value = Redacted;
                    break;
                case string s when RedactKeys(s):
                    value = Redacted;
                    break;
            }
        }
        catch (Exception)
        {
            value = Redacted;
        }

        return new DictionaryEntry { Key = d.Key, Value = value };
    }

    private static async Task<IResult> Initialise(
        [FromServices] IHost app,
        SyncPeriod syncPeriod,
        string? rootFolder,
        InitialisationStrategy? strategy,
        bool dropCollections = true)
    {
        await SyncEndpoints.InitialiseEnvironment(app, syncPeriod, rootFolder, strategy, dropCollections);

        return Results.Ok();
    }

    private static async Task<IResult> StartAsb([FromServices] ICompositeMessageBus messageBus, [FromServices] IOptions<ConsumerOptions> options)
    {
        if (options.Value.EnableAsbConsumers && messageBus is ICompositeMessageBus compositeMessageBus)
        {
            var asbMessageBuses = compositeMessageBus.GetChildBuses().Where(x => x.Name.StartsWith("ASB"));
            foreach (var asbMessageBus in asbMessageBuses)
            {
                if (!asbMessageBus.IsStarted)
                {
                    await asbMessageBus.Start();
                }
            }
        }

        return Results.Ok();
    }


    private static async Task<IResult> StopAsb([FromServices] IMasterMessageBus messageBus, [FromServices] IOptions<ConsumerOptions> options)
    {
        if (options.Value.EnableAsbConsumers && messageBus is ICompositeMessageBus compositeMessageBus)
        {
            var asbMessageBuses = compositeMessageBus.GetChildBuses().Where(x => x.Name.StartsWith("ASB"));
            foreach (var asbMessageBus in asbMessageBuses)
            {
                if (asbMessageBus.IsStarted)
                {
                    await asbMessageBus.Stop();
                }
            }

        }

        return Results.Ok();
    }

    [SuppressMessage("SonarLint", "S1215",
        Justification =
            "Ignored this is as its a management function that will be explicity invoked")]
    private static Task<IResult> ForceGC()
    {
        GC.Collect();

        return Task.FromResult(Results.Ok());
    }

    private static IResult GetEnvironment()
    {
        var dict = System.Environment.GetEnvironmentVariables();
        var filtered = dict.Cast<DictionaryEntry>().Select(Redact).ToArray();
        return Results.Ok(filtered);
    }

    private static IResult GetStatus()
    {
        var dict = new Dictionary<string, object>
        {
            { "version", System.Environment.GetEnvironmentVariable("CONTAINER_VERSION")! }
        };
        return Results.Ok(dict);
    }

    [AllowAnonymous]
    private static async Task<IResult> GetCollectionsAsync(IMongoDatabase db)
    {
        var collections =
            (await (await db.ListCollectionsAsync()).ToListAsync()).ConvertAll(c
                => new
                {
                    name = c["name"].ToString()!,
                    size = db.GetCollection<object>(c["name"].ToString()!).CountDocuments(Builders<object>.Filter.Empty),
                    indexes = GetIndexes(db, c["name"].ToString()!)
                });

        return Results.Ok(new { collections });
    }

    private static async Task<IResult> DropCollectionsAsync(IMongoDbContext context)
    {
        try
        {
            await context.ResetCollections();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Results.Ok("Reset");
    }

    private static List<string?> GetIndexes(IMongoDatabase db, string collectionName)
    {
        var indexes = db.GetCollection<BsonDocument>(collectionName).Indexes.List().ToList();
        return indexes.Select(x => x["name"].ToString()).ToList();
    }
}