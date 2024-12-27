using Btms.Backend.Data;
using Btms.Backend.Data.Mongo;
using Btms.BlobService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog.Extensions.Logging;
using TestDataGenerator.Scenarios;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Helpers;

/// <summary>
/// This isn't currently used but the original plan was to feed things into the consumers direcly, in the desired sequence, rather than rely on the
/// data lake sync. Needs further work as couldn't get hold of consumers...
/// </summary>
public class InMemoryScenarioApplicationFixture
    : WebApplicationFactory<Program>, IIntegrationTestsFixture
{
    public BtmsClient? BtmsClient { get; private set; }
    // private readonly ILogger<ScenarioApplicationFactory> _logger = messageSink.ToLogger<ScenarioApplicationFactory>();
    // internal IWebHost? _app;
    private IMongoDbContext? _mongoDbContext;

    // internal async Task InsertScenario()
    // {
    //     var logger = new Logger<ChedPSimpleMatchScenarioGenerator>(new SerilogLoggerFactory());
    //     var scenario = new ChedPSimpleMatchScenarioGenerator(logger);
    //
    //     return await Task.FromResult();
    // }
    // TODO : Make generic version work
    // internal async Task InsertScenario<T>() where T : notnull, new
    // {
    //     var scenario = new T();
    //     // var scenario = _app!.Services.GetRequiredService<T>();
    //     await Task.FromResult("");
    // }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Any integration test overrides could be added here
        // And we don't want to load the backend ini file 
        var configurationValues = new Dictionary<string, string>
        {
            { "DisableLoadIniFile", "true" },
            { "BlobServiceOptions:CachePath", "../../../Fixtures" },
            { "BlobServiceOptions:CacheReadEnabled", "true" },
            { "AuthKeyStore:Credentials:IntTest", "Password" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues!)
            .Build();

        builder
            .UseConfiguration(configuration)
            .ConfigureServices(services =>
            {
                var mongoDatabaseDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoDatabase))!;
                services.Remove(mongoDatabaseDescriptor);

                var blobOptionsValidatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IValidateOptions<BlobServiceOptions>))!;
                services.Remove(blobOptionsValidatorDescriptor);

                services.AddSingleton(sp =>
                {
                    var options = sp.GetService<IOptions<MongoDbOptions>>()!;
                    var settings = MongoClientSettings.FromConnectionString(options.Value.DatabaseUri);
                    var client = new MongoClient(settings);

                    // _mongoDbContext = client
                    var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
                    // convention must be registered before initialising collection
                    ConventionRegistry.Register("CamelCase", camelCaseConvention, _ => true);

                    var dbName = string.IsNullOrEmpty(DatabaseName) ? Random.Shared.Next().ToString() : DatabaseName;
                    var db = client.GetDatabase($"Btms_MongoDb_{dbName}_Test");
                   
                    // TODO : Use our ILoggerFactory
                    _mongoDbContext = new MongoDbContext(db, new SerilogLoggerFactory());
                    return db;
                });

                services.AddLogging(lb => lb.AddXUnit(TestOutputHelper));
            });

        builder.UseEnvironment("Development");
        
        var options = new WebApplicationFactoryClientOptions { AllowAutoRedirect = false };
        var httpClient = base.CreateClient(options);
        
        BtmsClient = new BtmsClient(httpClient);
    }

    public ITestOutputHelper TestOutputHelper { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public IMongoDbContext GetDbContext()
    {
        return Services.CreateScope().ServiceProvider.GetRequiredService<IMongoDbContext>();
    }
    
    public BtmsClient CreateBtmsClient(WebApplicationFactoryClientOptions options)
    {
        return new BtmsClient(base.CreateClient(options));
    }

    // public async Task ClearDb(HttpClient client)
    // {
    //     await client.GetAsync("mgmt/collections/drop");
    // }
}