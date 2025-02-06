using Btms.Business.Builders;
using Btms.Business.Commands;
using Btms.Model;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests;

public class DecisionStatusFinderTests
{
    [Fact]
    public void EnsureWeHaveAllStatusFinders()
    {
        //An exception would be thrown if it doesn't
        var decisionFinder = new DecisionStatusFinder();
        decisionFinder.Should().NotBeNull();
    }
}