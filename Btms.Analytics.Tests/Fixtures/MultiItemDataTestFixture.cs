using Bogus;
using Btms.Analytics.Tests.Helpers;
using Btms.Backend.Data;
using Btms.SyncJob.Extensions;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Scenarios;
using Xunit.Abstractions;

namespace Btms.Analytics.Tests.Fixtures;

#pragma warning disable S3881
public class MultiItemDataTestFixture : IDisposable
#pragma warning restore S3881
{
    public readonly IMongoDbContext MongoDbContext;
    private readonly IServiceScope _rootScope;
    private readonly ILogger<MultiItemDataTestFixture> _logger;

    public MultiItemDataTestFixture(IMessageSink messageSink)
    {
        _logger = messageSink.ToLogger<MultiItemDataTestFixture>();
        
        var builder = TestContextHelper.CreateBuilder<MultiItemDataTestFixture>();

        var app = builder.Build();
        _rootScope = app.Services.CreateScope();

        MongoDbContext = _rootScope.ServiceProvider.GetRequiredService<IMongoDbContext>();
        
        // Would like to pick this up from env/config/DB state
        var insertToMongo = true;
        
        if (insertToMongo)
        {
            MongoDbContext.ResetCollections().GetAwaiter().GetResult();
        
            app.PushToConsumers(_logger, app.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
            
            app.PushToConsumers(_logger, app.CreateScenarioConfig<CrNoMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
            
            app.PushToConsumers(_logger, app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
            
            app.PushToConsumers(_logger, app.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 1, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
            
            

            // var result = app.Services.WaitOnAllJobs(_logger).GetAwaiter().GetResult();
        }
    }
    
    public IImportNotificationsAggregationService GetImportNotificationsAggregationService(ITestOutputHelper testOutputHelper)
    {
        var logger = testOutputHelper.GetLogger<ImportNotificationsAggregationService>();
        return new ImportNotificationsAggregationService(MongoDbContext, logger);   
    }
    
    public IMovementsAggregationService GetMovementsAggregationService(ITestOutputHelper testOutputHelper)
    {
        var logger = testOutputHelper.GetLogger<MovementsAggregationService>();
        // return _rootScope.ServiceProvider.GetRequiredService<IMovementsAggregationService>(); 
        return new MovementsAggregationService(MongoDbContext, logger);
    }
    

    public void Dispose()
    {
        // ... clean up test data from the database ...
    }
}