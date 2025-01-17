using Microsoft.Extensions.DependencyInjection;
using TestDataGenerator;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace TestGenerator.IntegrationTesting.Backend;

public abstract class MultipleScenarioGeneratorBaseTest
{
    protected BtmsClient Client = null;
    protected readonly ITestOutputHelper TestOutputHelper;
    
    protected List<GeneratedResult> LoadedData;

    public required BackendFixture BackendFixture;
    
    private static Dictionary<Type, List<GeneratedResult>> AllScenarioDatasets
        = new Dictionary<Type, List<GeneratedResult>>();

    private bool ReloadData;
    
    protected MultipleScenarioGeneratorBaseTest(
        ITestOutputHelper testOutputHelper,
        bool reloadData = true
    )
    {
        TestOutputHelper = testOutputHelper;
        ReloadData = reloadData;
    }

    public void EnsureEnvironmentInitialised(Type generatorType)
    {
        // TODO :
        // Setup & cache Web app fixture
        //  -- Generate scenario data
        //  -- Load generated data into web app
        // Expose BTMS client & other helper methods
        // Use BTMS client to get movement & assert on decision code

        lock (generatorType)
        {
            var testGeneratorFixture = new TestGeneratorFixture(TestOutputHelper);
            BackendFixture = new BackendFixture(TestOutputHelper, generatorType.Name);
            Client = BackendFixture.BtmsClient;
            
            if (AllScenarioDatasets.TryGetValue(generatorType, out var loadedData))
            {
                TestOutputHelper.WriteLine("Scenario is cached. Using cached data");
                LoadedData = loadedData;
            }
            else
            {
                TestOutputHelper.WriteLine("Scenario is not cached, loading via test generator");
                
                // Generic version:
                // var data = testGeneratorFixture
                //     .GenerateTestData<T>();
                
                // Un-generic it : a bit ugly... :fingers-crossed
                var generateTestDataMethod = typeof(TestGeneratorFixture).GetMethod("GenerateTestData");
                var genericGenerateTestDataMethod = generateTestDataMethod!.MakeGenericMethod(generatorType);
                var data = (List<GeneratedResult>)genericGenerateTestDataMethod.Invoke(testGeneratorFixture, null)!;
                
                if (ReloadData)
                {
                    LoadedData = BackendFixture
                        .LoadTestData(data)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    TestOutputHelper.WriteLine("Warn : data in DB has not been replaced!");
                    LoadedData = data;
                }
                
                AllScenarioDatasets.Add(generatorType, LoadedData);
            }
        }
    }
    
    protected void AddAdditionalContextToAssertFailures(Action assert)
    {
        try
        {
            assert.Invoke();
        }
        catch(Exception)
        {
            TestOutputHelper.WriteLine("Additional context for assertion failure(s)");
            TestOutputHelper.WriteLine("{0} Notifications are in the database.", BackendFixture.MongoDbContext.Notifications.Count());
            TestOutputHelper.WriteLine("{0} Movements are in the database.", BackendFixture.MongoDbContext.Movements.Count());
            TestOutputHelper.WriteLine("Notification IDs are {0}.", BackendFixture.MongoDbContext.Notifications.Select(m => m.Id).ToArray());
            TestOutputHelper.WriteLine("Movement IDs are {0}.", BackendFixture.MongoDbContext.Movements.Select(m => m.Id).ToArray()!);
            
            //just throw to preserve stacktrace
            throw;
        }
    }

    protected async Task ClearDb()
    {
        await Client.ClearDb();
    }
}