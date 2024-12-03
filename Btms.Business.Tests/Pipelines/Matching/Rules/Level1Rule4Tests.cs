using Btms.Business.Pipelines.Matching;
using Btms.Business.Pipelines.Matching.Rules;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Pipelines.Matching.Rules;

public class Level1Rule4Tests
{
    [Fact]
    public async Task ProcessFilter_ValidContext_ReturnsNoMatch()
    {
        // Arrange
        var sut = new Level1Rule4();
        var context = new MatchContext();
        
        // Act
        var result = await sut.ProcessFilter(context);
        
        // Assert
        result.Should().NotBeNull();
        result.ExitPipeline.Should().BeFalse();
        context.Record.Should().StartWith("Did rule four");
    }
}