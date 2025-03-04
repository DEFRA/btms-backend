using Btms.Analytics.Tests.Helpers;
using Btms.Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using NSubstitute;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit.Abstractions;

namespace Btms.Analytics.Tests.Fixtures;

#pragma warning disable S3881
public class BasicSampleDataTestFixture : IDisposable
#pragma warning restore S3881
{
    public IHost App;

    private readonly IMongoDbContext _mongoDbContext;
    private readonly ILogger<MultiItemDataTestFixture> _logger;
    public BasicSampleDataTestFixture(IMessageSink messageSink)
    {
        _logger = messageSink.ToLogger<MultiItemDataTestFixture>();

        var builder = TestContextHelper.CreateBuilder<BasicSampleDataTestFixture>();

        App = builder.Build();
        var rootScope = App.Services.CreateScope();

        _mongoDbContext = rootScope.ServiceProvider.GetRequiredService<IMongoDbContext>();

        // Would like to pick this up from env/config/DB state
        var insertToMongo = true;

        if (insertToMongo)
        {
            _mongoDbContext.ResetCollections().GetAwaiter().GetResult();

            // Ensure we have some data scenarios around 24/48 hour tests
            App.Services.GeneratorPushToConsumers(_logger, App.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();

            App.Services.GeneratorPushToConsumers(_logger, App.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchCrFirstScenarioGenerator>(10, 3, arrivalDateRange: 2))
                .GetAwaiter().GetResult();

            App.Services.GeneratorPushToConsumers(_logger, App.Services.CreateScenarioConfig<CrNoMatchScenarioGenerator>(10, 3, arrivalDateRange: 0))
                .GetAwaiter().GetResult();

            // Create some more variable data over the rest of time
            App.Services.GeneratorPushToConsumers(_logger,
                    App.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 7, arrivalDateRange: 10))
                .GetAwaiter().GetResult();

            App.Services.GeneratorPushToConsumers(_logger, App.Services.CreateScenarioConfig<ChedANoMatchScenarioGenerator>(5, 3, arrivalDateRange: 10))
                .GetAwaiter().GetResult();

            App.Services.GeneratorPushToConsumers(_logger, App.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchCrFirstScenarioGenerator>(1, 3, arrivalDateRange: 10))
                .GetAwaiter().GetResult();

            App.Services.GeneratorPushToConsumers(_logger, App.Services.CreateScenarioConfig<CrNoMatchScenarioGenerator>(1, 3, arrivalDateRange: 10))
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
        return new MovementsAggregationService(_mongoDbContext, logger);
    }

    public void Dispose()
    {
        // ... clean up test data from the database ...
    }
}