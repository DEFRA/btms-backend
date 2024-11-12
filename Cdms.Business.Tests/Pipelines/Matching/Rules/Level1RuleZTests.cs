using Cdms.Business.Pipelines.Matching;
using Cdms.Business.Pipelines.Matching.Rules;
using FluentAssertions;
using Xunit;

namespace Cdms.Business.Tests.Pipelines.Matching.Rules;

public class Level1RuleZTests
{
    [Fact]
    public async Task ProcessFilter_ValidContext_ReturnsNoMatch()
    {
        // Arrange
        var sut = new Level1RuleZ();
        var context = new MatchContext();
        
        // Act
        var result = await sut.ProcessFilter(context);
        
        // Assert
        result.Should().NotBeNull();
        result.ExitPipeline.Should().BeFalse();
        context.Record.Should().StartWith("Did rule Z");
    }
}