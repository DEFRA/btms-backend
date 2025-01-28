using Btms.Backend.Data;
using Btms.BlobService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    BtmsClient CreateBtmsClient(WebApplicationFactoryClientOptions options);
    IMongoDbContext GetDbContext();
    IServiceProvider Services { get; }
}

public class ApplicationFactory : WebApplicationFactory<Program>, IIntegrationTestsApplicationFactory
{
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
                // To allow tests to freeze time as application should not be using things like DateTime.UtcNow everywhere
                // because it's not testable
                services.Replace(
                    ServiceDescriptor.Singleton<TimeProvider>(new FrozenTimeProvider(TimeProviderStartDateTime)));
                
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
    
    public DateTimeOffset TimeProviderStartDateTime { get; set; } = new(2025, 1, 28, 11, 30, 00, TimeSpan.Zero);

    public BtmsClient CreateBtmsClient(WebApplicationFactoryClientOptions options)
    {
        return new BtmsClient(base.CreateClient(options));
    }

    public IMongoDbContext GetDbContext()
    {
        return Services.CreateScope().ServiceProvider.GetRequiredService<IMongoDbContext>();
    }
}