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

namespace TestGenerator.IntegrationTesting.Backend.Fixtures;

public class BackendFixture
{
    private IMongoDbContext _mongoDbContext;
    public BtmsClient BtmsClient;
    
    // public List<GeneratedResult> LoadedData;
    
    // private IHost TestGeneratorApp { get; set; }
    private BackendFactory WebApp { get; set; }
    
    public BackendFixture()
    {   
        // Generate test data
        
        // var generatorBuilder = new HostBuilder();
        // generatorBuilder.ConfigureTestDataGenerator("Scenarios/Samples");
        
       // var dbName = typeof(T).Name;
       
    }

    public void Init(string dbName)
    {
        WebApp = new BackendFactory(DatabaseName, TestOutputHelper);
        (_mongoDbContext, BtmsClient) = WebApp.Start();
    }

    public ITestOutputHelper TestOutputHelper { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;

    // public IMongoDbContext GetDbContext()
    // {
    //     return _mongoDbContext;
    // }

    public async Task<List<GeneratedResult>> LoadTestData(List<GeneratedResult> testData)
    {
        await BtmsClient.ClearDb();
        
        // var data = new List<GeneratedResult>();
        
        // TODO: Need a logger
        var logger = NullLogger.Instance;
        
        await WebApp.Services.PushToConsumers(logger, testData.Select(d => d.Message));
        
        //
        // foreach (var t in testData)
        // {
        //     
        //     
        //     // data.AddRange(t.GeneratorResult);
        //     // var output = t.GeneratorResult
        //     //     .Select(r => new  (generatorResult.Generator, 0, 1, 1, r))
        //     //     .ToList();
        //     
        //     // LoadedData.AddRange(t.GeneratorResult);
        // }


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

                    var dbName = string.IsNullOrEmpty(databaseName) ?
                        Random.Shared.Next().ToString() :
                        databaseName
                            .Replace("ScenarioGenerator", "");
                    
                    var db = client.GetDatabase($"btms-{dbName}");
                   
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