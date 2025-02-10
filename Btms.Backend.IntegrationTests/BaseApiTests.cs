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

    protected BaseApiTests(IIntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper, string databaseName = "SmokeTests")
    {
        Factory = factory;
        Factory.TestOutputHelper = testOutputHelper;
        Factory.DatabaseName = databaseName;
    }
}