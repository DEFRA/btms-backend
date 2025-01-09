using Btms.Backend.IntegrationTests.Helpers;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;
using ImportNotificationTypeEnum = Btms.Model.Ipaffs.ImportNotificationTypeEnum;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class ChedPSimpleTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<SimpleMatchScenarioGenerator>(output)
{

    [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusOnDecison()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .Single()
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }
    
    [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }

    [Fact]
    public void ShouldHave2BtmsDecisions()
    {
        Client
            .GetSingleMovement()
            .Decisions.Count
            .Should().Be(2);
    }

    [Fact]
    public void ShouldHaveCorrectDecisionAuditEntries()
    {
        var chedPNotification = (ImportNotification)LoadedData
            .Single(d =>
                d is { Message: ImportNotification }
            )
            .Message;
        
        // Assert
        var movement = Client
            .GetSingleMovement();
        
        var decisionWithLinkAndContext = movement.AuditEntries
            .Where(a => a is { CreatedBy: "Btms", Status: "Decision" })
            .MaxBy(a => a.Version)!;
        
        decisionWithLinkAndContext.Context!.ImportNotifications
            .Should().NotBeNull();
        
        decisionWithLinkAndContext.Context!.ImportNotifications!
            .Select(n => (n.Id, n.Version))
            .Should()
            .Equal([
                ( chedPNotification.ReferenceNumber!, 1 )
            ]);
    }
    
    [Fact]
    public void ShouldHave1AlvsDecision()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Decisions
            .Count
            .Should()
            .Be(1);
    }

    [Fact]
    public void ShouldHaveCorrectAuditTrail()
    {
        Client
            .GetSingleMovement()
            .AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version))
            .Should()
            .Equal([
                ("Cds", "Created", 1),
                ("Btms", "Decision", 1),
                ("Btms", "Linked", null),
                ("Btms", "Decision", 2),
                ("Alvs", "Decision", 1)
            ]);
    }

    [Fact]
    public void ShouldHaveDecisionMatched()
    {
        var movement = Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context!.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }
    
    [Fact]
    public void ShouldHaveDecisionStatus()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs);
    }
    
    [Fact]
    public void ShouldHaveChedType()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.ChedTypes
            .Should().Equal(ImportNotificationTypeEnum.Cvedp);
    }
    
    [Fact]
    public void ShouldBeLinked()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.LinkStatus
            .Should().Be("Linked");
    }
    
    // [Fact]
    [Fact(Skip = "Relationships aren't being deserialised correctly")]
    // TODO : for some reason whilst jsonClientResponse contains the notification relationship,
    // but movement from .GetResourceObject(s)<Movement>();  doesn't!
    public void ShouldHaveNotificationRelationships()
    {
        Client
            .GetSingleMovement()
            .Relationships.Notifications.Data
            .Should().NotBeEmpty();
    }

    
    [Fact]
    public async Task ShouldNotHaveExceptions()
    {
        // TestOutputHelper.WriteLine("Querying for aggregated data");

        var result = await Client
            .GetExceptions();
        
        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());
        
        (await result.GetString())
            .Should()
            .Be("[]");
    }
    
    [Fact]
    public void AlvsDecisionShouldBePaired()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .Single()
            .Context
            .DecisionComparison!
            .Should().Match<DecisionComparison>(c => c.BtmsDecisionNumber == 2 && c.Paired == true);
    }
}