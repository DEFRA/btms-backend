using Btms.Business.Builders;
using Btms.Business.Commands;
using Btms.Model;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests;

public class BusinessDecisionStatusFinderTests
{
    [Fact]
    public void EnsureWeHaveAllStatusFinders()
    {
        //An exception would be thrown if it doesn't
        var decisionFinder = new BusinessDecisionStatusFinder();
        decisionFinder.Should().NotBeNull();
    }
}