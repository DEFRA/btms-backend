using Btms.Business.Pipelines.Matching;
using Btms.Business.Pipelines.Matching.Rules;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Pipelines.Matching.Rules;

public class Level1Rule1Tests
{
    [Fact]
    public async Task ProcessFilter_ValidContext_ReturnsNoMatch()
    {
        // Arrange
        var sut = new Level1Rule1();
        var context = new MatchContext();

        // Act
        var result = await sut.ProcessFilter(context);

        // Assert
        result.Should().NotBeNull();
        result.ExitPipeline.Should().BeFalse();
        context.Record.Should().StartWith("Did rule one");
    }

    // Refactor this into a theory once it's done
    [Fact]
    public async Task ProcessFilter_SingleMRNItem_SingleCHED_ThreeCommodityLines_ReturnsSingleMatch()
    {
        // Taken from an IBM example match
        // This should take the form of a single CHEDs with 3 commodity lines
        // The MRN should contain a single item referencing the only CHED

        // The end result of this using Rule 1 should be a single match from the MRN item to a single CHED commodity line
        // but no fixed determination on which CHED commodity line the MRN item is matched to

        // Arrange

        // Act

        // Assert
        Assert.True(true);
        await Task.CompletedTask;
    }

    // Refactor this into a theory once it's done
    [Fact]
    public async Task ProcessFilter_SingleMRN_TwoItems_SingleCHED_ThreeCommodityLines_ReturnsTwoMatches()
    {
        // Taken from an IBM example match
        // This should take the form of a single CHEDs with 3 commodity lines
        // The MRN should contain 2 items each referencing the only CHED

        // The end result of this using Rule 1 should be a match from each MRN item to a single CHED commodity line
        // Two matches in total but no fixed determination on which CHED commodity lines the MRN items are matched to

        // Arrange

        // Act

        // Assert
        Assert.True(true);
        await Task.CompletedTask;
    }
}