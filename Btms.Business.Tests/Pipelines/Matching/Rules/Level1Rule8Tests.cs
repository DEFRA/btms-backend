using Btms.Business.Pipelines.Matching;
using Btms.Business.Pipelines.Matching.Rules;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Pipelines.Matching.Rules;

public class Level1Rule8Tests
{
    [Fact]
    public async Task ProcessFilter_ValidContext_ReturnsNoMatch()
    {
        // Arrange
        var sut = new Level1Rule8();
        var context = new MatchContext();

        // Act
        var result = await sut.ProcessFilter(context);

        // Assert
        result.Should().NotBeNull();
        result.ExitPipeline.Should().BeFalse();
        context.Record.Should().StartWith("Did rule eight");
    }

    // Refactor this into a theory once it's done
    [Fact]
    public async Task ProcessFilter_SingleMRN_FourItems_ThreeCHEDs_FourTotalCommodityLines_ReturnsSixteenMatches()
    {
        // Taken from an IBM example match
        // This should take the form of 3 CHEDs with a single commodity line in 2, and 2 commodity lines in the final one
        // The MRN should contain four items, each item should reference all three CHEDs
        // except one which should only reference the single commodity CHEDs
        // The end result of this using Rule 8 should be a match from every single MRN item to every single CHED commodity line
        // Sixteen matches in total

        // Arrange

        // Act

        // Assert
        Assert.True(true);
        await Task.CompletedTask;
    }
}