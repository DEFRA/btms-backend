using Btms.Backend.IntegrationTests.Helpers;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Btms.Backend.IntegrationTests;

public abstract class BaseApiTests
{
    private BtmsClient? _client;
    
    protected BtmsClient Client => _client ??= Factory.CreateBtmsClient();
    
    protected IIntegrationTestsApplicationFactory Factory { get; }
    
    protected async Task ClearDb()
    {
        await Client.ClearDb();
    }

    protected async Task WaitUntilHealthy()
    {
        var response = await Client.GetHealth();

        var task = Task.Run(async () =>
        {
            while (!response.IsSuccessStatusCode)
            {
                await Task.Delay(3000);
                response = await Client.GetHealth();
            }
        });

        var winner = await Task.WhenAny(
            task,
            Task.Delay(TimeSpan.FromMinutes(5)));

        if (winner != task)
        {
            throw new TimeoutException("Timeout waiting for service to become healthy");
        }
    }

    protected BaseApiTests(IIntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper, string databaseName = "SmokeTests")
    {
        Factory = factory;
        Factory.TestOutputHelper = testOutputHelper;
        Factory.DatabaseName = databaseName;
    }
}