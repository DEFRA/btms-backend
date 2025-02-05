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
public class IpaffsCancelledTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CancelledNotificationTestsScenarioGenerator>(output)
{
    [Fact]
    public void NotificationShouldBeDeleted_AndRelationshipsRemoved_AndHaveAuditEntries()
    {
        // Assert
        var resource = Client
            .GetNotificationById("CHEDD.GB.2024.5246106");

        var notification = resource.GetResourceObject<ImportNotification>();

        resource.Data.Relationships!.First().Value!.Data.ManyValue!.Count.Should().Be(1);

        notification.Status.Should().Be(ImportNotificationStatusEnum.Cancelled);
        notification.AuditEntries.Count(x => x.Status == "Unlinked").Should().Be(0);
        notification.AuditEntries.Count(x => x.Status == "Cancelled").Should().Be(1);
    }
}