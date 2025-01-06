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

    private static Dictionary<string, List<GeneratedResult>> AllTestClassesDatasets
        = new Dictionary<string, List<GeneratedResult>>();
        
    protected ScenarioDatasetBaseTest(
        ITestOutputHelper testOutputHelper,
        string datasetName
    )
    {
        TestOutputHelper = testOutputHelper;
        
        var testGeneratorFixture = new TestGeneratorFixture(testOutputHelper);
        BackendFixture = new BackendFixture(testOutputHelper, GetType().Name, 200);
        
        Client = BackendFixture.BtmsClient;
        _mongoDbContext = BackendFixture.MongoDbContext;

        // This lock may not be needed if xunit guarantees tests in the same test class
        // are not parrallelised
        lock (datasetName)
        {
            if (AllTestClassesDatasets.TryGetValue(datasetName, out var loadedData))
            {
                testOutputHelper.WriteLine("Dataset is cached. Using cached data");
                LoadedData = loadedData;
            }
            else
            {
                testOutputHelper.WriteLine("Dataset is not cached, loading via test generator");
                var data = testGeneratorFixture
                    .GenerateTestDataset(datasetName)
                    .GetAwaiter()
                    .GetResult();
                
                LoadedData = BackendFixture
                    .LoadTestData(data)
                    .GetAwaiter()
                    .GetResult();
                
                AllTestClassesDatasets.Add(datasetName, LoadedData);
            }
        }
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