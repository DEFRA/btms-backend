using Btms.Backend.IntegrationTests.Helpers;
using Btms.Consumers.Extensions;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using TestDataGenerator;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;
using Decision = Btms.Types.Alvs.Decision;

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

    protected async Task PublishMessagesToInMemoryTopics<T>(Func<object, bool>? filter = null) where T : ScenarioGenerator
    {
        var messages = GenerateMessages<T>(filter);

        await Factory.Services.PushToConsumers(Factory.TestOutputHelper.GetLogger<BaseApiTests>(), messages);
    }

    protected async Task PublishMessagesToAzureServiceBus<T>(Func<object, bool>? filter = null) where T : ScenarioGenerator
    {
        var messages = GenerateMessages<T>(filter);

        foreach (var message in messages)
        {
            switch (message)
            {
                case AlvsClearanceRequest cr:
                    await AzureServiceBusHelper.PublishClearanceRequest(cr);
                    break;
                case Decision d:
                    await AzureServiceBusHelper.PublishDecision(d);
                    break;
                case ImportNotification n:
                    await AzureServiceBusHelper.PublishNotification(n);
                    break;
                case Gmr n:
                    await AzureServiceBusHelper.PublishGmr(n);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown message type {message.GetType()}");
            }
        }
    }

    private IEnumerable<object> GenerateMessages<T>(Func<object, bool>? filter) where T : ScenarioGenerator
    {
        var testGeneratorFixture = new TestGeneratorFixture(Factory.TestOutputHelper);
        var generatorResult = testGeneratorFixture.GenerateTestData<T>();
        var messages = generatorResult.Select(x => x.Message);
        messages = filter != null ? messages.Where(filter) : messages;

        return messages;
    }
}