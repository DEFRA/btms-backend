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
    private readonly string _datasetName;
    private readonly bool _reloadData; 
    protected readonly IMongoDbContext MongoDbContext;
    protected readonly BtmsClient Client;
    protected readonly BackendFixture BackendFixture;
    protected readonly ITestOutputHelper TestOutputHelper;
    
    private static Dictionary<string, List<GeneratedResult>> AllDatasets
        = new Dictionary<string, List<GeneratedResult>>();

    protected readonly IImportNotificationsAggregationService ImportNotificationsAggregationService;
    protected readonly IMovementsAggregationService MovementsAggregationService;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="testOutputHelper"></param>
    /// <param name="datasetName"></param>
    /// <param name="reloadData">Can be set to false locally in an individual test when iterating, to not reload the database every time the test is run]</param>
    protected ScenarioDatasetBaseTest(
        ITestOutputHelper testOutputHelper,
        string datasetName,
        bool reloadData = true
    )
    {
        _datasetName = datasetName;
        _reloadData = reloadData;
        TestOutputHelper = testOutputHelper;
        
        // At the moment the datasets don't care about alvs decisions,
        // So rather than slow down the load, we can just run it concurrently
        var backendConfigurationOverrides = new Dictionary<string, string>
        {
            { "ConsumerOptions:EnableBlockingPublish", "true" }
        };
        
        var testGeneratorFixture = new TestGeneratorFixture(testOutputHelper);
        BackendFixture = new BackendFixture(testOutputHelper, datasetName, 200, backendConfigurationOverrides);
        
        Client = BackendFixture.BtmsClient;
        MongoDbContext = BackendFixture.MongoDbContext;
        
        ImportNotificationsAggregationService = new ImportNotificationsAggregationService(MongoDbContext,
            TestOutputHelper.GetLogger<ImportNotificationsAggregationService>());

        MovementsAggregationService = new MovementsAggregationService(MongoDbContext,
            TestOutputHelper.GetLogger<MovementsAggregationService>());

        if (reloadData)
        {
            lock (datasetName)
            {
                if (AllDatasets.ContainsKey(datasetName))
                {
                    testOutputHelper.WriteLine("Dataset is cached. Using cached data");
                }
                else
                {
                    testOutputHelper.WriteLine("Dataset is not cached, loading via test generator");
                    var data = testGeneratorFixture
                        .GenerateTestDataset(datasetName)
                        .GetAwaiter()
                        .GetResult();

                    var loadedData = BackendFixture
                        .LoadTestData(data)
                        .GetAwaiter()
                        .GetResult();

                    AllDatasets.Add(datasetName, loadedData);
                }
            }
        }
    }

    protected List<GeneratedResult> GetLoadedData()
    {
        if (!_reloadData)
        {
            throw new Exception("You can't set reloadData to false if you rely on the LoadedData field in a test.");
        }
        return AllDatasets.GetValueOrDefault(_datasetName)!;
    } 
    
    /// <summary>
    /// TODO : would this be better moved into an Analytics specific base class?
    /// </summary>
    /// <param name="testOutputHelper"></param>
    /// <returns></returns>
    // [Obsolete("Use the ImportNotificationsAggregationService property instead")]
    protected IImportNotificationsAggregationService GetImportNotificationsAggregationService()
    {
        var logger = TestOutputHelper.GetLogger<ImportNotificationsAggregationService>();
        return new ImportNotificationsAggregationService(MongoDbContext, logger);   
    }
    
    /// <summary>
    /// TODO : would this be better moved into an Analytics specific base class?
    /// </summary>
    /// <param name="testOutputHelper"></param>
    /// <returns></returns>
    // [Obsolete("Use the MovementsAggregationService property instead")]
    protected IMovementsAggregationService GetMovementsAggregationService()
    {
        var logger = TestOutputHelper.GetLogger<MovementsAggregationService>();
        return new MovementsAggregationService(MongoDbContext, logger);
    }

    protected async Task ClearDb()
    {
        await Client.ClearDb();
    }
}