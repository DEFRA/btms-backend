using Btms.Analytics;
using Btms.Backend.Data;
using Btms.Backend.Data.Mongo;
using Btms.BlobService;
using Btms.Business.Commands;
using Btms.Common.Extensions;
using Btms.Consumers.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
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
using TestDataGenerator.Config;
using TestDataGenerator.Extensions;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Helpers;


public class InMemoryApplicationFactory(string databaseName, ITestOutputHelper testOutputHelper ) : WebApplicationFactory<Program> 
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

                    var dbName = string.IsNullOrEmpty(databaseName) ? Random.Shared.Next().ToString() : databaseName;
                    var db = client.GetDatabase($"Btms_{dbName}");
                   
                    // TODO : Use our ILoggerFactory
                    mongoDbContext = new MongoDbContext(db, new SerilogLoggerFactory());
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

public class InMemoryScenarioApplicationFactory<T>
    : IIntegrationTestsApplicationFactory
    where T : ScenarioGenerator
{
    private readonly IMongoDbContext _mongoDbContext;
    private readonly BtmsClient _btmsClient;
    
    public List<(
        ScenarioGenerator generator, int scenario, int dateOffset, int count, object message
        )> LoadedData;
    
    private IHost TestGeneratorApp { get; set; }
    private InMemoryApplicationFactory WebApp { get; set; }
    
    public InMemoryScenarioApplicationFactory()
    {   
        // Generate test data
        
        var generatorBuilder = new HostBuilder();
        generatorBuilder.ConfigureTestDataGenerator("Scenarios/Samples");
        
        TestGeneratorApp = generatorBuilder.Build();

        var dbName = string.IsNullOrEmpty(DatabaseName) ? typeof(T).Name : DatabaseName;
        WebApp = new InMemoryApplicationFactory(dbName, TestOutputHelper);
        (_mongoDbContext, _btmsClient) = WebApp.Start();

        LoadedData = GenerateAndLoadTestData().GetAwaiter().GetResult();;
    }

    public ITestOutputHelper TestOutputHelper { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public IMongoDbContext GetDbContext()
    {
        return _mongoDbContext;
    }
    
    public BtmsClient CreateBtmsClient(WebApplicationFactoryClientOptions options)
    {
        return _btmsClient;
    }

    private async Task<List<(
        ScenarioGenerator generator, int scenario, int dateOffset, int count, object message
        )>> GenerateAndLoadTestData()
    {

        // TODO : Naive caching implementation, improve
        // if (LoadedData.HasValue())
        // {
        //     return LoadedData;
        // }

        await _btmsClient.ClearDb();
        
        LoadedData = new List<(ScenarioGenerator generator, int scenario, int dateOffset, int count, object message)>();
        
        // TODO: Need a logger
        var logger = NullLogger.Instance;
        
        var scenarioConfig =
            TestGeneratorApp.Services.CreateScenarioConfig<T>(1, 1, arrivalDateRange: 0);

        var generatorResults = scenarioConfig.Generate(logger, 0);
        foreach (var generatorResult in generatorResults)
        {
            await WebApp.Services.PushToConsumers(logger, generatorResult);
            var output = generatorResult
                // ScenarioGenerator generator, int scenario, int dateOffset, int count, object message
                .Select(r => (scenarioConfig.Generator, 0, 1, 1, r))
                .ToList();
            
            LoadedData.AddRange(output);
        }


        return LoadedData;
    }
}