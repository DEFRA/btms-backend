using Btms.Backend.IntegrationTests.Helpers;
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
    public void ShouldHaveCorrectAlvsDecisions()
    {
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();
        
        movement.AlvsDecisionStatus.Decisions.Count.Should().Be(1);
        
        movement.AlvsDecisionStatus.Decisions
            .First()
            .Context.DecisionMatched
            .Should().BeTrue();
    }
    
    [Fact]
    public void ShouldHaveCorrectBtmsDecisions()
    {
        var chedPNotification = (ImportNotification)LoadedData
            .Single(d =>
                d is { Message: ImportNotification }
            )
            .Message;
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();
        
        movement.Decisions.Count.Should().Be(2);
        
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
    public void ShouldHaveCorrectAuditTrail()
    {
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();
        
        movement.AuditEntries
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
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

        movement
            .AlvsDecisionStatus
            .Context!
            .DecisionMatched
            .Should()
            .BeTrue();
    }
    
    [Fact]
    public void ShouldHaveDecisionStatus()
    {
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

        movement
            .AlvsDecisionStatus
            .DecisionStatus
            .Should()
            .Be(DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs);
    }
    
    [Fact]
    public void ShouldHaveChedType()
    {
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();
        
        movement
            .BtmsStatus
            .ChedTypes
            .Should()
            .Equal(ImportNotificationTypeEnum.Cvedp);
    }
    
    [Fact]
    // [Fact(Skip = "Relationships aren't being deserialised correctly")]
    // TODO : for some reason whilst jsonClientResponse contains the notification relationship,
    // but movement from .GetResourceObject(s)<Movement>();  doesn't!
    public void ShouldBeLinked()
    {
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

        movement.BtmsStatus.LinkStatus.Should().Be("Linked");
    }
    
    // [Fact]
    [Fact(Skip = "Relationships aren't being deserialised correctly")]
    // TODO : for some reason whilst jsonClientResponse contains the notification relationship,
    // but movement from .GetResourceObject(s)<Movement>();  doesn't!
    public void ShouldHaveNotificationRelationships()
    {
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

        movement
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
}