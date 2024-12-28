using Btms.Analytics;
using Btms.Backend.Data;
using Btms.Backend.Data.Mongo;
using Btms.BlobService;
using Btms.Business.Commands;
using Btms.Common.Extensions;
using Btms.Consumers.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Helpers;

public class InMemoryScenarioApplicationFactory
    : WebApplicationFactory<Program>, IIntegrationTestsApplicationFactory
{
    // private readonly ILogger<ScenarioApplicationFactory> _logger = messageSink.ToLogger<ScenarioApplicationFactory>();
    // internal IWebHost? _app;
    private IMongoDbContext? _mongoDbContext;

    public List<(
        ScenarioGenerator generator, int scenario, int dateOffset, int count, object message
        )>? LoadedData;
    
    private IHost TestGeneratorApp { get; set; }
    
    public InMemoryScenarioApplicationFactory()
    {   
        // Generate test data
        // TODO : Unsure if we should use a new app here or use the same one?...
        // But when we use the same one the config conflicts...       

        var generatorBuilder = new HostBuilder();
        generatorBuilder.ConfigureTestDataGenerator();
        
        TestGeneratorApp = generatorBuilder.Build();
        // Dataset = Datasets.GetDatasets(TestGeneratorApp);
    }
    
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
                services.ConfigureTestGenerationServices();
            });

        builder.UseEnvironment("Development");
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

    public async Task<List<(
        ScenarioGenerator generator, int scenario, int dateOffset, int count, object message
        )>> GenerateAndLoadTestData(BtmsClient client, string datasetName = "One", SyncPeriod period = SyncPeriod.All)
    {

        // TODO : Naive caching implementation, improve
        if (LoadedData.HasValue())
        {
            return LoadedData;
        }

        LoadedData = new List<(ScenarioGenerator generator, int scenario, int dateOffset, int count, object message)>();
        
        // var consumer = (NotificationConsumer)this.Services
        //     .GetRequiredService<IConsumer<ImportNotification>>();
        
        // var scope = this.Services.CreateScope();
        //
        // var notificationConsumer = (NotificationConsumer)scope
        //     .ServiceProvider
        //     .GetRequiredService<IConsumer<ImportNotification>>();
        //
        // notificationConsumer.Context = new ConsumerContext
        // {
        //     Headers = new Dictionary<string, object> { { "messageId", n.ReferenceNumber! } }
        // };
        //                 
        // await notificationConsumer.OnHandle(n);
        // logger.LogInformation("Sent notification {0} to consumer", n.ReferenceNumber!);
        
        // var generator = this.Services.GetRequiredService<Generator>();
        
        // TODO: Need a logger
        var logger = NullLogger.Instance;
        
        var scenarioConfig =
            this.Services.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 1, arrivalDateRange: 0);

        var generatorResults = scenarioConfig.Generate(logger, 0);
        foreach (var generatorResult in generatorResults)
        {
            await this.Services.PushToConsumers(logger, generatorResult);
            var output = generatorResult
                // ScenarioGenerator generator, int scenario, int dateOffset, int count, object message
                .Select(r => (scenarioConfig.Generator, 0, 1, 1, r))
                .ToList();
            
            LoadedData.AddRange(output);
        }
        // var messages = scenario.Generator.Generate(0, 1, DateTime.Today, new ScenarioConfig()
        // {
        //     ArrivalDateRange = 1, Count = 1, CreationDateRange = 0, 
        // })
        // var generatorResult = await generator.Generate(scenario, s, dataset.RootPath);


        return LoadedData;
    }

    
    // public async Task ClearDb(HttpClient client)
    // {
    //     await client.GetAsync("mgmt/collections/drop");
    // }
}