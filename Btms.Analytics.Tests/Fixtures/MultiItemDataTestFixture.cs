using Btms.Analytics.Tests.Helpers;
using Btms.Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using TestDataGenerator.Scenarios;

namespace Btms.Analytics.Tests.Fixtures;

#pragma warning disable S3881
public class MultiItemDataTestFixture : IDisposable
#pragma warning restore S3881
{
    public readonly IImportNotificationsAggregationService ImportNotificationsAggregationService;
    public readonly IMovementsAggregationService MovementsAggregationService;

    public IMongoDbContext MongoDbContext;
    public MultiItemDataTestFixture()
    {
        var builder = TestContextHelper.CreateBuilder<MultiItemDataTestFixture>();

        var app = builder.Build();
        var rootScope = app.Services.CreateScope();

        MongoDbContext = rootScope.ServiceProvider.GetRequiredService<IMongoDbContext>();
        ImportNotificationsAggregationService = rootScope.ServiceProvider.GetRequiredService<IImportNotificationsAggregationService>();
        MovementsAggregationService = rootScope.ServiceProvider.GetRequiredService<IMovementsAggregationService>();
        
        // Would like to pick this up from env/config/DB state
        var insertToMongo = true;
        
        if (insertToMongo)
        {
            MongoDbContext.ResetCollections().GetAwaiter().GetResult();
        
            app.PushToConsumers(app.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
        
            app.PushToConsumers(app.CreateScenarioConfig<CrNoMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
        
            app.PushToConsumers(app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
        }
    }

    public void Dispose()
    {
        // ... clean up test data from the database ...
    }
}