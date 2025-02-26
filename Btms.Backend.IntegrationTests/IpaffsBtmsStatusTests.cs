using Btms.Common.Extensions;
using Btms.Model.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.ChedP;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class IpaffsBtmsStatusTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(SimpleMatchCrFirstScenarioGenerator), NotificationLinkStatus.Linked, TypeAndLinkStatus.ChedPLinked)]
    [InlineData(typeof(SimpleMatchNotificationFirstScenarioGenerator), NotificationLinkStatus.Linked, TypeAndLinkStatus.ChedPLinked)]
    [InlineData(typeof(ChedANoMatchScenarioGenerator), NotificationLinkStatus.NotLinked, TypeAndLinkStatus.ChedANotLinked)]

    public void ShouldHaveCorrectStatus(Type generatorType, NotificationLinkStatus notificationLinkStatus, TypeAndLinkStatus typeAndLinkStatus)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Status : {1}", generatorType!.FullName, notificationLinkStatus);
        EnsureEnvironmentInitialised(generatorType);
        CheckLinkStatus(notificationLinkStatus, typeAndLinkStatus);
    }

    private void CheckLinkStatus(NotificationLinkStatus notificationLinkStatus, TypeAndLinkStatus typeAndLinkStatus)
    {
        var notification = Client.GetSingleImportNotification();

        TestOutputHelper.WriteLine("Notification {0}, expectedLinkStatus {1}", notification.Id, notificationLinkStatus);

        notification
            .BtmsStatus.LinkStatus
            .Should().Be(notificationLinkStatus);

        notification
            .BtmsStatus.TypeAndLinkStatus
            .Should().Be(typeAndLinkStatus);
    }
}