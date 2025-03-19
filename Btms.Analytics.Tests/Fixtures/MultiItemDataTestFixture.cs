using Btms.Analytics.Tests.Helpers;
using Btms.Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend.Extensions;
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

            app.Services.GeneratorPushToConsumers(_logger, app.Services.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();

            app.Services.GeneratorPushToConsumers(_logger, app.Services.CreateScenarioConfig<CrNoMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();

            app.Services.GeneratorPushToConsumers(_logger, app.Services.CreateScenarioConfig<CrNoMatchNoDecisionScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();

            app.Services.GeneratorPushToConsumers(_logger, app.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();

            app.Services.GeneratorPushToConsumers(_logger, app.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchCrFirstScenarioGenerator>(1, 1, arrivalDateRange: 0))
                .GetAwaiter().GetResult();
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