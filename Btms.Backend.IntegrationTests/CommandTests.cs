using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.SyncJob;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class CommandTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "CommandTests"), IClassFixture<ApplicationFactory>
{
    [Fact]
    public async Task SyncNotifications()
    {
        await ClearDb();

        var command = new SyncNotificationsCommand() { SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest" };

        await Client.MakeSyncNotificationsRequest(command);

        Factory.GetDbContext().Notifications.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SyncClearanceRequests()
    {
        await ClearDb();

        var command = new SyncClearanceRequestsCommand()
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        };

        await Client.MakeSyncClearanceRequest(command);

        Factory.GetDbContext().Movements.Count().Should().BeGreaterThan(0);
    }


    [Fact]
    public async Task Download()
    {
        await ClearDb();

        var command = new SyncNotificationsCommand() { SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest" };

        await Client.MakeSyncNotificationsRequest(command);

        var crCommand = new SyncClearanceRequestsCommand()
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        };

        await Client.MakeSyncClearanceRequest(crCommand);

        const string smokeTestChed = "CHEDA.GB.2024.1041389";
        const string smokeTestMrn = "CHEDAGB20241041389";

        var smokeTestDownloadFilter = new DownloadCommand.DownloadFilter(Mrns: [smokeTestMrn],
            Cheds: [DownloadCommand.Ched.FromReference(smokeTestChed)]);

        var downloadCommand = new DownloadCommand() { SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest", Filter = smokeTestDownloadFilter };

        var res = await Client.MakeDownloadRequest(downloadCommand);
        var response = await res.Content.ReadAsStringAsync();

        //Remove quotes at start and end
        response = response.Replace("\"", "");

        File.Exists($"../../../../Btms.Backend/{response}.zip").Should().BeTrue();
    }
}