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
    public ILinkingAggregationService LinkingAggregationService;

    public IMongoDbContext MongoDbContext;
    public AggregationTestFixture()
    {
        var builder = TestContextHelper.CreateBuilder<AggregationTestFixture>();

        App = builder.Build();
        var rootScope = App.Services.CreateScope();

        MongoDbContext = rootScope.ServiceProvider.GetRequiredService<IMongoDbContext>();
        LinkingAggregationService = rootScope.ServiceProvider.GetRequiredService<ILinkingAggregationService>();
        
        MongoDbContext.DropCollections().GetAwaiter().GetResult();

        // Ensure we have some data scenarios around 24 hour tests
        App.PushToConsumers(App.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 1, arrivalDateRange: 2), 1)
            .GetAwaiter().GetResult();
        
        App.PushToConsumers(App.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(10, 1, arrivalDateRange: 2), 2)
            .GetAwaiter().GetResult();
        
        // Create some more variable data over the rest of time
        App.PushToConsumers(App.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 7, arrivalDateRange: 10), 3)
            .GetAwaiter().GetResult();
        
        App.PushToConsumers(App.CreateScenarioConfig<ChedANoMatchScenarioGenerator>(5, 3, arrivalDateRange: 10), 4)
            .GetAwaiter().GetResult();
        
        App.PushToConsumers(App.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 3, arrivalDateRange: 10), 5)
            .GetAwaiter().GetResult();
        
        App.PushToConsumers(App.CreateScenarioConfig<CRNoMatchScenarioGenerator>(1, 3, arrivalDateRange: 10), 6)
            .GetAwaiter().GetResult();

        // SetupData().GetAwaiter().GetResult();
    }

    // public async Task SetupData()
    // {
    //     await MongoDbContext.DropCollections();
    //
    //     var scenario = App.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 10);
    //     await App.PushToConsumers(scenario, 1);
    //
    //     var noMatchScenario = App.CreateScenarioConfig<ChedANoMatchScenarioGenerator>(5, 3, arrivalDateRange: 10);
    //     await App.PushToConsumers(noMatchScenario, 2);
    //
    //     var chedPScenario = App.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 3, arrivalDateRange: 10);
    //     await App.PushToConsumers(chedPScenario, 3);
    // }

    public void Dispose()
    {
        // ... clean up test data from the database ...
    }
}