using Btms.Model;
using Btms.Model.Auditing;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.PreprocessingTests;

[Trait("Category", "Integration")]
public class IpaffsDeletedTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<DeletedNotificationTestsScenarioGenerator>(output)
{
    [Fact]
    public void NotificationShouldBeDeleted_AndRelationshipsRemoved_AndHaveAuditEntries()
    {
        // Assert
        var resource = Client
            .GetNotificationById("CHEDD.GB.2024.5256764");

        var notification = resource.GetResourceObject<ImportNotification>();

        resource.Data.Relationships!.First().Value!.Data.ManyValue!.Count.Should().Be(0);

        notification.Status.Should().Be(ImportNotificationStatusEnum.Deleted);
        notification.AuditEntries.Count(x => x.Status == "Unlinked").Should().Be(1);
        notification.AuditEntries.Count(x => x.Status == "Deleted").Should().Be(1);
    }

    [Fact]
    public void MovementShouldHaveRelationshipsRemoved_AndHaveAuditEntries()
    {
        // Assert
        var movement1 = Client.GetMovementByMrn("24GBDHDJXQ5TCDBAR1");
        var movement2 = Client.GetMovementByMrn("24GBDPN9J48XRW5AR0");

        movement1.Data.Relationships!.First().Value!.Data.ManyValue!.Count.Should().Be(0);
        movement1.GetResourceObject<Movement>().AuditEntries.Count(x => x.Status == "Unlinked").Should().Be(1);

        movement2.Data.Relationships!.First().Value!.Data.ManyValue!.Count.Should().Be(0);
        movement2.GetResourceObject<Movement>().AuditEntries.Count(x => x.Status == "Unlinked").Should().Be(1);
    }
}