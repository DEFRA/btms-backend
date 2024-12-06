using Btms.Analytics.Tests.Helpers;
using Btms.Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestDataGenerator.Scenarios;

namespace Btms.Analytics.Tests.Fixtures;

#pragma warning disable S3881
public class BasicSampleDataTestFixture : IDisposable
#pragma warning restore S3881
{
    public IHost App;
    public IImportNotificationsAggregationService ImportNotificationsAggregationService;
    public IMovementsAggregationService MovementsAggregationService;

    public IMongoDbContext MongoDbContext;
    public BasicSampleDataTestFixture()
    {
        var builder = TestContextHelper.CreateBuilder<BasicSampleDataTestFixture>();

        App = builder.Build();
        var rootScope = App.Services.CreateScope();

        MongoDbContext = rootScope.ServiceProvider.GetRequiredService<IMongoDbContext>();
        ImportNotificationsAggregationService = rootScope.ServiceProvider.GetRequiredService<IImportNotificationsAggregationService>();
        MovementsAggregationService = rootScope.ServiceProvider.GetRequiredService<IMovementsAggregationService>();
        
        // Would like to pick this up from env/config/DB state
        var insertToMongo = true;

        if (insertToMongo)
        {
            MongoDbContext.ResetCollections().GetAwaiter().GetResult();

            // Ensure we have some data scenarios around 24/48 hour tests
            App.PushToConsumers(App.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();

            App.PushToConsumers(App.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 2))
                .GetAwaiter().GetResult();

            App.PushToConsumers(App.CreateScenarioConfig<CrNoMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();

            // Create some more variable data over the rest of time
            App.PushToConsumers(
                    App.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 7, arrivalDateRange: 10))
                .GetAwaiter().GetResult();

            App.PushToConsumers(App.CreateScenarioConfig<ChedANoMatchScenarioGenerator>(5, 3, arrivalDateRange: 10))
                .GetAwaiter().GetResult();

            App.PushToConsumers(App.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 3, arrivalDateRange: 10))
                .GetAwaiter().GetResult();

            App.PushToConsumers(App.CreateScenarioConfig<CrNoMatchScenarioGenerator>(1, 3, arrivalDateRange: 10))
                .GetAwaiter().GetResult();
        }
    }

    public void Dispose()
    {
        // ... clean up test data from the database ...
    }
}