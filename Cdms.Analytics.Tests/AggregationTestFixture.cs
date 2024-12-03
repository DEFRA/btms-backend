using Cdms.Analytics.Tests.Helpers;
using Cdms.Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestDataGenerator.Scenarios;

namespace Cdms.Analytics.Tests;

#pragma warning disable S3881
public class AggregationTestFixture : IDisposable
#pragma warning restore S3881
{
    public IHost App;
    public IImportNotificationsAggregationService ImportNotificationsAggregationService;
    public IMovementsAggregationService MovementsAggregationService;

    public IMongoDbContext MongoDbContext;
    public AggregationTestFixture()
    {
        var builder = TestContextHelper.CreateBuilder<AggregationTestFixture>();

        App = builder.Build();
        var rootScope = App.Services.CreateScope();

        MongoDbContext = rootScope.ServiceProvider.GetRequiredService<IMongoDbContext>();
        ImportNotificationsAggregationService = rootScope.ServiceProvider.GetRequiredService<IImportNotificationsAggregationService>();
        MovementsAggregationService = rootScope.ServiceProvider.GetRequiredService<IMovementsAggregationService>();
        
        MongoDbContext.ResetCollections().GetAwaiter().GetResult();

        // Ensure we have some data scenarios around 24/48 hour tests
        App.PushToConsumers(App.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
            .GetAwaiter().GetResult();
        
        App.PushToConsumers(App.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 2))
            .GetAwaiter().GetResult();
        
        App.PushToConsumers(App.CreateScenarioConfig<CRNoMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
            .GetAwaiter().GetResult();
        
        // Create some more variable data over the rest of time
        App.PushToConsumers(App.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 7, arrivalDateRange: 10))
            .GetAwaiter().GetResult();
        
        App.PushToConsumers(App.CreateScenarioConfig<ChedANoMatchScenarioGenerator>(5, 3, arrivalDateRange: 10))
            .GetAwaiter().GetResult();
        
        App.PushToConsumers(App.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 3, arrivalDateRange: 10))
            .GetAwaiter().GetResult();
        
        App.PushToConsumers(App.CreateScenarioConfig<CRNoMatchScenarioGenerator>(1, 3, arrivalDateRange: 10))
            .GetAwaiter().GetResult();

    }

    public void Dispose()
    {
        // ... clean up test data from the database ...
    }
}