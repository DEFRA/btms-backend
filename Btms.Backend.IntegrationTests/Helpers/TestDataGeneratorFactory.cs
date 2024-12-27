using System.Reflection;
using Btms.Backend.Data;
using Btms.BlobService;
using Btms.Business.Commands;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using TestDataGenerator;
using TestDataGenerator.Config;
using Xunit.Abstractions;

using TestDataGenerator.Extensions;

namespace Btms.Backend.IntegrationTests.Helpers;

public class TestDataGeneratorFactory : WebApplicationFactory<Program>, IIntegrationTestsApplicationFactory
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        return base.CreateHost(builder);
    }

    protected override IWebHostBuilder? CreateWebHostBuilder()
    {
        return base.CreateWebHostBuilder();
    }
    
    // Create a 'Data Lake' like folder
    // Relative to the executing dll
    string CachePath = Path.GetFullPath("../.int-tests-test-data-generator", 
        Assembly.GetExecutingAssembly().Location);

    
    private Datasets Datasets { get; set; }
    private IHost TestGeneratorApp { get; set; }

    public TestDataGeneratorFactory()
    {
        
        // Generate test data
        // TODO : Unsure if we should use a new app here or use the same one?...
        // But when we use the same one the config conflicts...       

        var generatorBuilder = new HostBuilder();
        generatorBuilder.ConfigureTestDataGenerator(CachePath);
        
        TestGeneratorApp = generatorBuilder.Build();
        Datasets = new Datasets(TestGeneratorApp);
    }

    // TODO : Should we be using IHost / Builder rather than IWebHostBuilder
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {   
        // Any integration test overrides could be added here
        // And we don't want to load the backend ini file 
        var configurationValues = new Dictionary<string, string>
        {
            { "DisableLoadIniFile", "true" },
            { "BlobServiceOptions:CachePath", CachePath },
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

                    var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
                    // convention must be registered before initialising collection
                    ConventionRegistry.Register("CamelCase", camelCaseConvention, _ => true);

                    var dbName = string.IsNullOrEmpty(DatabaseName) ? Random.Shared.Next().ToString() : DatabaseName;
                    return client.GetDatabase($"Btms_MongoDb_{dbName}_Test");
                });

                services.AddLogging(lb => lb.AddXUnit(TestOutputHelper));
            });

        builder.UseEnvironment("Development");
    }

    public ITestOutputHelper TestOutputHelper { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public IMongoDbContext GetDbContext()
    {
        return Services.CreateScope().ServiceProvider.GetRequiredService<IMongoDbContext>();
    }

    // public async Task ClearDb(HttpClient client)
    // {
    //     await client.GetAsync("mgmt/collections/drop");
    // }
    
    public BtmsClient CreateBtmsClient(WebApplicationFactoryClientOptions options)
    {
        return new BtmsClient(base.CreateClient(options));
    }

    public async Task GenerateAndLoadTestData(BtmsClient client, string rootFolder = "GENERATED-ONE", SyncPeriod period = SyncPeriod.All)
    {
        var testDataset = Datasets.One;
        var generator = TestGeneratorApp.Services.GetRequiredService<Generator>();
        
        TestOutputHelper.WriteLine("{0} scenario(s) configured", testDataset.Scenarios.Count());

        var scenario = 1;
        
        await generator.Cleardown(testDataset.RootPath);

        foreach (var s in testDataset.Scenarios)
        {
            await generator.Generate(scenario, s, testDataset.RootPath);
            scenario++;
        }

        TestOutputHelper.WriteLine("{0} Done", testDataset.Name);
        
        TestOutputHelper.WriteLine("Generated test data");
        
        await client.ClearDb();
        
        await client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = period,
            RootFolder = rootFolder
        });
        
        await client.MakeSyncNotificationsRequest(new SyncNotificationsCommand()
        {
            SyncPeriod = period,
            RootFolder = rootFolder
        });
        
        await client.MakeSyncDecisionsRequest(new SyncDecisionsCommand()
        {
            SyncPeriod = period,
            RootFolder = rootFolder
        });
    }
}