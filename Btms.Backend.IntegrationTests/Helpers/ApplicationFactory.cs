using Btms.Backend.Data;
using Btms.Backend.IntegrationTests.Consumers.AmazonQueues;
using Btms.BlobService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Helpers;

public interface IIntegrationTestsApplicationFactory
{
    ITestOutputHelper TestOutputHelper { get; set; }
    string DatabaseName { get; set; }
    BtmsClient CreateBtmsClient(WebApplicationFactoryClientOptions? options = null);
    IMongoDbContext GetDbContext();
    IServiceProvider Services { get; }
}

public class ApplicationFactory : WebApplicationFactory<Program>, IIntegrationTestsApplicationFactory
{
    public Action<IConfigurationBuilder> ConfigureHostConfiguration { get; set; } = _ => { };
    public bool InternalQueuePublishWillBlock { get; set; }
    public bool EnableAzureServiceBusConsumers { get; set; }
    public bool EnableAmazonSnsSqsConsumers { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Any integration test overrides could be added here
        // And we don't want to load the backend ini file 

        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(AwsConfig.DefaultLocalConfig)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DisableLoadIniFile", "true" },
                { "BlobServiceOptions:CachePath", "../../../../Btms.Test.Data/Samples" },
                { "BlobServiceOptions:CacheReadEnabled", "true" },
                { "AuthKeyStore:Credentials:IntTest", "Password" }
            });

        if (InternalQueuePublishWillBlock)
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConsumerOptions:EnableBlockingPublish", "true" }
            });

        if (EnableAzureServiceBusConsumers)
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConsumerOptions:EnableAsbConsumers", "true" }
            });

        if (EnableAmazonSnsSqsConsumers)
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConsumerOptions:EnableAmazonConsumers", "true" }
            });

        ConfigureHostConfiguration(configurationBuilder);

        builder
            .UseConfiguration(configurationBuilder.Build())
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

                    var camelCaseConvention = new ConventionPack
                    {
                        new CamelCaseElementNameConvention()
                    };
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

    public BtmsClient CreateBtmsClient(WebApplicationFactoryClientOptions? options = null)
    {
        return new BtmsClient(CreateClient(options ?? new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }));
    }

    public IMongoDbContext GetDbContext()
    {
        return Services.CreateScope().ServiceProvider.GetRequiredService<IMongoDbContext>();
    }
}