using Microsoft.Extensions.DependencyInjection;
using TestDataGenerator;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace TestGenerator.IntegrationTesting.Backend;

public abstract class ScenarioGeneratorBaseTest<T>
    where T : ScenarioGenerator
{
    protected readonly BtmsClient Client;
    protected readonly ITestOutputHelper TestOutputHelper;

    protected readonly List<GeneratedResult> LoadedData;

    public required BackendFixture BackendFixture;

    private static Dictionary<Type, List<GeneratedResult>> AllScenarioDatasets
        = new Dictionary<Type, List<GeneratedResult>>();

    protected ScenarioGeneratorBaseTest(
        ITestOutputHelper testOutputHelper,
        bool reloadData = true
    )
    {
        TestOutputHelper = testOutputHelper;

        var testGeneratorFixture = new TestGeneratorFixture(testOutputHelper);
        BackendFixture = new BackendFixture(testOutputHelper, typeof(T).Name);

        Client = BackendFixture.BtmsClient;

        lock (typeof(T))
        {
            if (AllScenarioDatasets.TryGetValue(typeof(T), out var loadedData))
            {
                testOutputHelper.WriteLine("Scenario is cached. Using cached data");
                LoadedData = loadedData;
            }
            else
            {
                testOutputHelper.WriteLine("Scenario is not cached, loading via test generator");

                var data = testGeneratorFixture
                    .GenerateTestData<T>();


                if (reloadData)
                {
                    LoadedData = BackendFixture
                        .LoadTestData(data, true)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    TestOutputHelper.WriteLine("Warn : data in DB has not been replaced!");
                    LoadedData = data;
                }

                AllScenarioDatasets.Add(typeof(T), LoadedData);
            }
        }

    }

    protected void AddAdditionalContextToAssertFailures(Action assert)
    {
        try
        {
            assert.Invoke();
        }
        catch (Exception)
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