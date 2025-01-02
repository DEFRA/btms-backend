using Btms.Analytics.Extensions;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Relationships;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Analytics.Tests.Unit;

public static class MovementExtensions
{
    public static string GetDescription(this Movement m)
    {
        return new List<Movement>() { m }
            .AsQueryable()
            .SelectLinkStatus()
            .First()
            .Description;
    }
    
}
public class MovementLinkStatus
{
    [Fact]
    public void WhenRelationshipPresent_ShouldBeLinked()
    {
        new Movement()
            {
                CreatedSource = DateTime.Now,
                Relationships = new MovementTdmRelationships()
                {
                    Notifications = new TdmRelationshipObject()
                    {
                        Data = [new RelationshipDataItem()]
                    }
                },
                Status = MovementStatus.Default()
            }
            .GetDescription()
            .Should()
            .Be("Linked");
    }
    
    [Fact]
    public void ShouldBeNotLinked()
    {
        new Movement()
        {
            CreatedSource = DateTime.Now,
            Status = new MovementStatus()
            {
                ChedTypes = []
            }
        }
        .GetDescription()
        .Should()
        .Be("Not Linked");
    }
    
    [Fact]
    public void WhenNoRelationshipPresentAnd_ShouldBeLinked()
    {
        var m = new Movement()
        {
            CreatedSource = DateTime.Now,
            AlvsDecisionStatus = new AlvsDecisionStatus()
            {
                Context = new DecisionContext()
                {
                    AlvsCheckStatus = new StatusChecker()
                    {
                        AnyMatch = true
                    }
                }
            },
            Status = MovementStatus.Default()
        };
        
        m
            .GetDescription()
            .Should()
            .Be("Investigate");
    }
}