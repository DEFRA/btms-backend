using Btms.Backend.Data;
using Btms.Backend.Data.Mongo;
using Btms.BlobService;
using Btms.Common.Extensions;
using Btms.Consumers.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog.Extensions.Logging;
using TestDataGenerator;
using Xunit.Abstractions;

using TestGenerator.IntegrationTesting.Backend.Extensions;

namespace TestGenerator.IntegrationTesting.Backend.Fixtures;

public class BackendFixture
{
    public readonly IMongoDbContext MongoDbContext;
    public readonly BtmsClient BtmsClient;
    
    private BackendFactory WebApp { get; set; }
    private readonly int _consumerPushDelayMs;
    public readonly ITestOutputHelper TestOutputHelper;

    public BackendFixture(ITestOutputHelper testOutputHelper, string databaseName, int consumerPushDelayMs = 1000)
    {
        TestOutputHelper = testOutputHelper;
        _consumerPushDelayMs = consumerPushDelayMs;
        
        WebApp = new BackendFactory(databaseName, testOutputHelper);
        (MongoDbContext, BtmsClient) = WebApp.Start();
        
    }

    public async Task<List<GeneratedResult>> LoadTestData(List<GeneratedResult> testData)
    {
        await BtmsClient.ClearDb();
        
        var logger = TestOutputHelper.GetLogger<BackendFactory>();
        
        await WebApp.Services.PushToConsumers(logger, testData.Select(d => d.Message), _consumerPushDelayMs);
        
        return testData;
    }
}
public class BackendFactory(string databaseName, ITestOutputHelper testOutputHelper ) : WebApplicationFactory<Program> 
{
    private IMongoDbContext? mongoDbContext;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Any integration test overrides could be added here
        // And we don't want to load the backend ini file 
        var configurationValues = new Dictionary<string, string>
        {
            { "DisableLoadIniFile", "true" },
            { "BlobServiceOptions:CachePath", "Scenarios/Samples" },
            { "BlobServiceOptions:CacheReadEnabled", "true" },
            { "AuthKeyStore:Credentials:IntTest", "Password" },

            { "ConsumerOptions:EnableBlockingPublish", "true" }
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

                    var dbName = string.IsNullOrEmpty(databaseName) ?
                        Random.Shared.Next().ToString() :
                        databaseName
                            .Replace("ScenarioGenerator", "");
                    
                    var db = client.GetDatabase($"btms-{dbName}");
                   
                    mongoDbContext = new MongoDbContext(db, testOutputHelper.GetLoggerFactory());
                    return db;
                });

                if (testOutputHelper.HasValue())
                {
                    services.AddLogging(lb => lb.AddXUnit(testOutputHelper));    
                }
            });

        builder.UseEnvironment("Development");
    }

    public (IMongoDbContext, BtmsClient) Start()
    {
        var builder = Host.CreateDefaultBuilder();
        
        var host = base
            .CreateHost(builder);
        
        // Creating the client causes the 
        var client = this.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var btmsClient = new BtmsClient(client);

        return (mongoDbContext!, btmsClient);
    }

}