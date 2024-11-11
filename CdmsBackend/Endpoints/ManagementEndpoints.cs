using System.Collections;
using System.Linq.Expressions;
using System.Security.Claims;
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
            app.MapGet(BaseRoute + "/proxy/set", SetProxy).AllowAnonymous();
            app.MapGet(BaseRoute + "/proxy/unset", UnsetProxy).AllowAnonymous();
        }
    }

    private static bool FilterEnvKeys(DictionaryEntry d)
    {
        var key = d.Key.ToString()!;
        return key.StartsWith("DMP") || key.StartsWith("CDP")
                                     || key.StartsWith("AZURE") || key.StartsWith("TRADE")
                                     || key.StartsWith("HTTP") || key.StartsWith("TDM")
                                     || key == "CONTAINER_VERSION";
    }

    private static IResult GetEnvironment()
    {
        var dict = System.Environment.GetEnvironmentVariables();
        var filtered = dict.Cast<DictionaryEntry>().Where(FilterEnvKeys).ToArray();
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

    private static IResult SetProxy(IConfiguration configuration)
    {
        System.Environment.SetEnvironmentVariable("HTTPS_PROXY", configuration["CDP_HTTPS_PROXY"]);
        System.Environment.SetEnvironmentVariable("HTTP_PROXY", configuration["CDP_HTTP_PROXY"]);
        return Results.Ok();
    }

    private static IResult UnsetProxy()
    {
        System.Environment.SetEnvironmentVariable("HTTPS_PROXY", "");
        System.Environment.SetEnvironmentVariable("HTTP_PROXY", "");
        return Results.Ok();
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
        
        return Results.Ok(collections);
    }

    private static async Task<IResult> DropCollectionsAsync(IMongoDatabase db)
    {
        try
        {
            var collections = await (await db.ListCollectionsAsync()).ToListAsync();

            foreach (var collection in collections)
            {
                await db.DropCollectionAsync(collection["name"].ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Results.Ok("Dropped");
    }
}