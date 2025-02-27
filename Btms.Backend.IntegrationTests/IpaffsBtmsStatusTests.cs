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
    [InlineData(typeof(SimpleMatchCrFirstScenarioGenerator), LinkStatus.Linked, TypeAndLinkStatus.ChedPLinked)]
    [InlineData(typeof(SimpleMatchNotificationFirstScenarioGenerator), LinkStatus.Linked, TypeAndLinkStatus.ChedPLinked)]
    [InlineData(typeof(ChedANoMatchScenarioGenerator), LinkStatus.NotLinked, TypeAndLinkStatus.ChedANotLinked)]

    public void ShouldHaveCorrectStatus(Type generatorType, LinkStatus linkStatus, TypeAndLinkStatus typeAndLinkStatus)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Status : {1}", generatorType!.FullName, linkStatus);
        EnsureEnvironmentInitialised(generatorType);
        CheckLinkStatus(linkStatus, typeAndLinkStatus);
    }

    private void CheckLinkStatus(LinkStatus linkStatus, TypeAndLinkStatus typeAndLinkStatus)
    {
        var notification = Client.GetSingleImportNotification();

        TestOutputHelper.WriteLine("Notification {0}, expectedLinkStatus {1}", notification.Id, linkStatus);

        notification
            .BtmsStatus.LinkStatus
            .Should().Be(linkStatus);

        notification
            .BtmsStatus.TypeAndLinkStatus
            .Should().Be(typeAndLinkStatus);
    }
}