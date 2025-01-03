using Btms.Analytics;
using Btms.Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using TestDataGenerator;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace TestGenerator.IntegrationTesting.Backend;

public abstract class ScenarioDatasetBaseTest
{
    private readonly IMongoDbContext _mongoDbContext;
    protected readonly BtmsClient Client;
    protected readonly BackendFixture BackendFixture;
    protected readonly ITestOutputHelper TestOutputHelper;

    protected readonly List<GeneratedResult> LoadedData;
    protected ScenarioDatasetBaseTest(
        ITestOutputHelper testOutputHelper
    )
    {
        TestOutputHelper = testOutputHelper;
        
        var testGeneratorFixture = new TestGeneratorFixture(testOutputHelper);
        BackendFixture = new BackendFixture(testOutputHelper, GetType().Name, 200);
        
        Client = BackendFixture.BtmsClient;
        _mongoDbContext = BackendFixture.MongoDbContext;
        
        var data = testGeneratorFixture
            .GenerateTestDataset("LoadTest-Condensed")
            .GetAwaiter()
            .GetResult();
        
        LoadedData = BackendFixture
            .LoadTestData(data)
            .GetAwaiter()
            .GetResult();
    }
    
    /// <summary>
    /// TODO : would this be better moved into an Analytics specific base class?
    /// </summary>
    /// <param name="testOutputHelper"></param>
    /// <returns></returns>
    protected IImportNotificationsAggregationService GetImportNotificationsAggregationService()
    {
        var logger = TestOutputHelper.GetLogger<ImportNotificationsAggregationService>();
        return new ImportNotificationsAggregationService(_mongoDbContext, logger);   
    }
    
    /// <summary>
    /// TODO : would this be better moved into an Analytics specific base class?
    /// </summary>
    /// <param name="testOutputHelper"></param>
    /// <returns></returns>
    protected IMovementsAggregationService GetMovementsAggregationService()
    {
        var logger = TestOutputHelper.GetLogger<MovementsAggregationService>();
        return new MovementsAggregationService(_mongoDbContext, logger);
    }

    protected async Task ClearDb()
    {
        await Client.ClearDb();
    }
}