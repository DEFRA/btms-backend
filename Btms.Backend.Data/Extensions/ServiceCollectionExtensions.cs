using Btms.Backend.Data.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Btms.Backend.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddHostedService<MongoIndexService>();

        services.AddScoped<IMongoDbContext, MongoDbContext>();
        services.AddSingleton(sp =>
        {

            var options = sp.GetService<IOptions<MongoDbOptions>>();
            var settings = MongoClientSettings.FromConnectionString(options?.Value.DatabaseUri);
            settings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber(new InstrumentationOptions { CaptureCommandText = true }));

            var client = new MongoClient(settings);

            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            // convention must be registered before initialising collection
            ConventionRegistry.Register("CamelCase", camelCaseConvention, _ => true);

            return client.GetDatabase(options?.Value.DatabaseName);
        });

        return services;
    }
}