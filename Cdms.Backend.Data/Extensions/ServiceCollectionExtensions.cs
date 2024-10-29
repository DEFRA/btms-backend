using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Cdms.Backend.Data.Extensions
{
    public class MongoDbOptions
    {
        public const string SectionName = nameof(MongoDbOptions);

        [Required] public string DatabaseUri { get; set; }

        [Required] public string DatabaseName { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbContext(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<MongoDbOptions>()
                .Bind(configuration.GetSection(MongoDbOptions.SectionName))
                .ValidateDataAnnotations();


            services.AddScoped<IMongoDbContext, MongoDbContext>();
            services.AddSingleton(sp =>
            {
                var options = sp.GetService<IOptions<MongoDbOptions>>();
                var connectionString = configuration.GetValue<string>("Mongo:DatabaseUri")!;
                var dbName = configuration.GetValue<string>("Mongo:DatabaseName")!;
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                var client = new MongoClient(settings);

                var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
                // convention must be registered before initialising collection
                ConventionRegistry.Register("CamelCase", camelCaseConvention, _ => true);

                return client.GetDatabase(options.Value.DatabaseName);
            });

            return services;
        }
    }
}