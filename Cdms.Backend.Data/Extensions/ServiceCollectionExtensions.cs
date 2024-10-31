using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Cdms.Backend.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbContext(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IMongoDbContext, MongoDbContext>();
            services.AddSingleton(sp =>
            {
                var connectionString = configuration.GetValue<string>("Mongo:DatabaseUri")!;
                var dbName = configuration.GetValue<string>("Mongo:DatabaseName")!;
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                var client = new MongoClient(settings);

                var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
                // convention must be registered before initialising collection
                ConventionRegistry.Register("CamelCase", camelCaseConvention, _ => true);

                return client.GetDatabase(dbName);
            });

            return services;
        }
    }
}