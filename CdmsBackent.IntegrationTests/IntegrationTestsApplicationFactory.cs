using Azure.Storage.Blobs;
using Cdms.Backend.Data.Extensions;
using Cdms.BlobService;
using Cdms.Consumers.Tests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.IO;
using Cdms.Backend.Data;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace CdmsBackend.IntegrationTests;

public class LocalBlobItem(string name) : IBlobItem
{
    public string Name { get; set; } = name;

    public string NormalisedName { get; set; } = default;
    public string Content { get; set; } = default!;

    public async Task<string> Download(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            var content = await File.ReadAllTextAsync(Name, cancellationToken);
            Content = content;
        }

        return Content;
    }
}

public class LocalBlobService(string root) : IBlobService
{
    public IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, CancellationToken cancellationToken)
    {
        return ScanFiles(Path.Combine(root, prefix), cancellationToken);
    }

    public async IAsyncEnumerable<IBlobItem> ScanFiles(string prefix, CancellationToken cancellationToken)
    {
        foreach (string f in Directory.GetFiles(prefix))
        {
            yield return new LocalBlobItem(f);
        }

        foreach (string d in Directory.GetDirectories(prefix))
        {
            await foreach (var item in GetResourcesAsync(d, cancellationToken))
            {
                yield return item;
            }
        }
    }
}

public class IntegrationTestsApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var mongoDatabaseDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoDatabase));
            services.Remove(mongoDatabaseDescriptor);

            services.AddSingleton(sp =>
            {
                var options = sp.GetService<IOptions<MongoDbOptions>>();
                var settings = MongoClientSettings.FromConnectionString(options.Value.DatabaseUri);
                var client = new MongoClient(MongoRunnerProvider.Instance.Get().ConnectionString);

                var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
                // convention must be registered before initialising collection
                ConventionRegistry.Register("CamelCase", camelCaseConvention, _ => true);

                return client.GetDatabase($"Cdms_MongoDb_{Random.Shared.Next()}_Test");
            });

            var blobServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBlobService));
            services.Remove(blobServiceDescriptor);

            services.AddSingleton<IBlobService>(new LocalBlobService("../../../Fixtures"));

            services.AddLogging(lb => lb.AddXUnit(testOutputHelper));
        });

        builder.UseEnvironment("Development");
    }

    internal ITestOutputHelper testOutputHelper { get; set; }

    public IMongoDbContext GetDbContext()
    {
        return Services.CreateScope().ServiceProvider.GetRequiredService<IMongoDbContext>();
    }
}