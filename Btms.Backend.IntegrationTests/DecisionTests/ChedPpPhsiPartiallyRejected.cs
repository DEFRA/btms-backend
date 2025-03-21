using System.Net;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Model.Cds;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;
using ImportNotificationTypeEnum = Btms.Model.Ipaffs.ImportNotificationTypeEnum;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration"), Trait("Segment", "CDMS-205-Ac3")]
public class ChedPpPhsiPartiallyRejected(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<ChedPPLinkedMrnNoDecisionsScenarioGenerator>(output)
{

    [Fact]
    public void ShouldHaveNoAlvsDecision()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .Count
            .Should().Be(0);
    }

    [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeFalse();
    }

    [Fact]
    public void ShouldHave4BtmsDecisions()
    {
        // Act
        var decisions = Client
            .GetSingleMovement()
            .Decisions;

        // Assert
        decisions.Count
            .Should().Be(4);
    }

    [Fact]
    public void ShouldHaveCorrectDecisionAuditEntries()
    {
        var notification = (ImportNotification)LoadedData
            .First(d =>
                d is { Message: ImportNotification }
            )
            .Message;

        // Assert
        var movement = Client
            .GetSingleMovement();

        var decisionWithLinkAndContext = movement.AuditEntries
            .Where(a => a is { CreatedBy: CreatedBySystem.Btms, Status: "Decision" })
            .MaxBy(a => a.Version)!;

        decisionWithLinkAndContext.Context.As<DecisionContext>()!.ImportNotifications
            .Should().NotBeNull();

        decisionWithLinkAndContext.Context.As<DecisionContext>()!.ImportNotifications!
            .Select(n => (n.Id, n.Version))
            .Should()
            .Equal(
                (notification.ReferenceNumber!, 1)
            );
    }

    [Fact]
    public void ShouldHaveNoAlvsDecisions()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.NoAlvsDecisions);
    }

    [Fact]
    public void ShouldHaveChedType()
    {
        Client
            .GetSingleMovement()
            .Status.ChedTypes
            .Should().Equal(ImportNotificationTypeEnum.Chedpp);
    }

    [Fact]
    public void ShouldHaveCorrectBtmsStatus()
    {
        var movement =
            Client
                .GetSingleMovement();

        movement
            .Status
            .Should().BeEquivalentTo(
                new
                {
                    LinkStatus = LinkStatus.AllLinked,
                    Segment = MovementSegmentEnum.Cdms205Ac3
                }
            );
    }

    [Fact]
    public async Task ShouldNotHaveExceptions()
    {
        var result = await Client
            .GetExceptions();

        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());

        (await result.GetString())
            .Should()
            .Be("[]");
    }
}