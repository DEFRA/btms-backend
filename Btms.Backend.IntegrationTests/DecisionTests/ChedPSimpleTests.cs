using Btms.Model;
using FluentAssertions;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model.Ipaffs;
using Btms.Types.Alvs;
using TestDataGenerator.Scenarios.ChedP;
using Xunit;
using Xunit.Abstractions;
using ImportNotification = Btms.Types.Ipaffs.ImportNotification;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class ChedPSimpleTests(InMemoryScenarioApplicationFactory<SimpleMatchScenarioGenerator> factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "DecisionTests"), IClassFixture<InMemoryScenarioApplicationFactory<SimpleMatchScenarioGenerator>>
{
    
    [Fact]
    public void ShouldBeLinked()
    {
        // Act
        var movementResource = Client.AsJsonApiClient()
            .Get("api/movements")
            .Data
            .Single()
            .Relationships!.Count.Should().Be(1);
    }
    
    [Fact]
    public void ShouldHaveCorrectDecisions()
    {
        var chedPNotification = (Types.Ipaffs.ImportNotification)factory
            .LoadedData
            .Single(d =>
                d is { message: Types.Ipaffs.ImportNotification }
            )
            .message;
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();
        
        movement.Decisions.Count.Should().Be(2);
        movement.AlvsDecisionStatus.Decisions.Count.Should().Be(1);
        
        movement.AlvsDecisionStatus.Decisions
            .First()
            .Context.DecisionMatched
            .Should().BeTrue();
        
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
    public void ShouldHaveChedType()
    {
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();
       
        
        movement
            .Status
            .ChedTypes
            .Should()
            .Equal(ImportNotificationTypeEnum.Cvedp);
        
        //
        // movement
        //     .AlvsDecisionStatus
        //     .Context!
        //     .ChedTypes
        //     .Should()
        //     .Equal(ImportNotificationTypeEnum.Cvedp);

    }
    
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
}