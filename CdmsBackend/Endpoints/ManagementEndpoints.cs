using System.Collections;
using Cdms.Backend.Data;
using CdmsBackend.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CdmsBackend.Endpoints;

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
        }
    }

    private static string[] KeysToRedact = [
        "Mongo__DatabaseUri",
        "MONGO_URI"
    ];
    
    private static bool RedactKeys(string key)
    {   
        return key.StartsWith("AZURE") ||
               key.StartsWith("BlobServiceOptions__Azure") ||
               key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
               KeysToRedact.Contains(key);
    }

    private const string Redacted = "--redacted--";
    
    private static DictionaryEntry Redact(DictionaryEntry d)
    {
        
        object? value = d.Value;
        
        try
        {
            switch(d.Key) 
            {
                case "HTTP_PROXY" or "HTTPS_PROXY":
                    // redact the password - doesn't have protocol, ie.
                    // cdms-backend::passC@proxy.perf-test.cdp-int.defra.cloud
                    value = Redacted;
                    break;
                case "CDP_HTTP_PROXY" or "CDP_HTTPS_PROXY":
                    //  redact the password
                    // https://cdms-backend::passC@proxy.perf-test.cdp-int.defra.cloud
                    value = Redacted;
                    break;
                case string s when RedactKeys(s):
                    value = Redacted;
                    break;
                default:
                    break;
            }
        }
        catch (Exception)
        {
            value = Redacted;
        }
        
        return new DictionaryEntry() { Key = d.Key, Value = value };
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
                    size = db.GetCollection<object>(c["name"].ToString()!).CountDocuments(Builders<object>.Filter.Empty)
                });
        
        return Results.Ok(new { collections = collections });
    }

    private static async Task<IResult> DropCollectionsAsync(IMongoDbContext context)
    {
        try
        {
            await context.DropCollections();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Results.Ok("Dropped");
    }
}