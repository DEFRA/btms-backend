using Btms.Analytics.Tests.Helpers;
using Btms.Backend.Data;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using TestDataGenerator.Scenarios;
using Xunit.Abstractions;

namespace Btms.Analytics.Tests.Fixtures;

#pragma warning disable S3881
public class MultiItemDataTestFixture : IDisposable
#pragma warning restore S3881
{
    private readonly IMongoDbContext _mongoDbContext;
    private readonly IServiceScope _rootScope;
    public MultiItemDataTestFixture()
    {
        var builder = TestContextHelper.CreateBuilder<MultiItemDataTestFixture>();

        var app = builder.Build();
        _rootScope = app.Services.CreateScope();

        _mongoDbContext = _rootScope.ServiceProvider.GetRequiredService<IMongoDbContext>();
        
        // Would like to pick this up from env/config/DB state
        var insertToMongo = true;
        
        if (insertToMongo)
        {
            _mongoDbContext.ResetCollections().GetAwaiter().GetResult();
        
            app.PushToConsumers(app.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
        
            app.PushToConsumers(app.CreateScenarioConfig<CrNoMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
        
            app.PushToConsumers(app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
        }
    }
    
    public IImportNotificationsAggregationService GetImportNotificationsAggregationService(ITestOutputHelper testOutputHelper)
    {
        var logger = testOutputHelper.GetLogger<ImportNotificationsAggregationService>();
        return new ImportNotificationsAggregationService(_mongoDbContext, logger);   
    }
    
    public IMovementsAggregationService GetMovementsAggregationService(ITestOutputHelper testOutputHelper)
    {
        var logger = testOutputHelper.GetLogger<MovementsAggregationService>();
        // return _rootScope.ServiceProvider.GetRequiredService<IMovementsAggregationService>(); 
        return new MovementsAggregationService(_mongoDbContext, logger);
    }
    

    public void Dispose()
    {
        // ... clean up test data from the database ...
    }
}