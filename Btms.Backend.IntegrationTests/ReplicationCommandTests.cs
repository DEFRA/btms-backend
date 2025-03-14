using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using FluentAssertions;
using System.IO.Compression;
using Btms.Replication.Commands;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class ReplicationCommandTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "CommandTests", "CDMS-408-DMP-Datalake-Issue", replicationConfig),
        IClassFixture<ApplicationFactory>
{
    /// <summary>
    /// This isn't the best test, but it at least exercises the code. Would be a good idea to setup some mocking around ReplicationTargetBlobService
    /// but will pick that up after the refactoring.
    /// </summary>
    [Fact]
    public async Task Replicate()
    {
        await ClearDb();

        var command = new ReplicateCommand() { SyncPeriod = SyncPeriod.All };

        var res = await Client.MakeReplicateRequest(command);
        var response = await res.Content.ReadAsStringAsync();

        //Remove quotes at start and end
        response = response.Replace("\"", "");

        response.Should().NotBeNull();
    }

    private static readonly Dictionary<string, string?> replicationConfig = new Dictionary<string, string?>() {
        {
            "ReplicationOptions:Enabled", "true"
        },
        {
            "ReplicationOptions:AzureClientId", "XXX"
        },
        {
            "ReplicationOptions:AzureTenantId", "XXX"
        },
        {
            "ReplicationOptions:AzureClientSecret", "XXX"
        },
        {
            "ReplicationOptions:DmpBlobContainer", "XXX"
        },
        {
            "ReplicationOptions:DmpBlobRootFolder", "XXX"
        },
        {
            "ReplicationOptions:DmpBlobUri", "XXX"
        }
    };
}