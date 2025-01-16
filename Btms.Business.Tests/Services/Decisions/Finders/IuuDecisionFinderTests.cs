using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class IuuDecisionFinderTests
{
    [Theory]
    [InlineData(null, DecisionCode.X00, "TBC")]
    [InlineData(ControlAuthorityIuuOptionEnum.Iuuok, DecisionCode.C07, "TBC")]
    [InlineData(ControlAuthorityIuuOptionEnum.IUUNotCompliant, DecisionCode.X00, "TBC")]
    [InlineData(ControlAuthorityIuuOptionEnum.Iuuna, DecisionCode.C08, "TBC")]
    public void DecisionFinderTest(ControlAuthorityIuuOptionEnum? iuuOption, DecisionCode expectedDecisionCode, string? expectedDecisionReason)
    {
        // Arrange
        var notification = new ImportNotification
        {
            PartTwo = new PartTwo
            {
                ControlAuthority = new ControlAuthority
                {
                    IuuCheckRequired = true,
                    IuuOption = iuuOption
                }
            }
        };
        var sut = new IuuDecisionFinder();

        // Act
        var result = sut.FindDecision(notification);

        // Assert
        result.DecisionCode.Should().Be(expectedDecisionCode);
        result.DecisionReason.Should().Be(expectedDecisionReason);
    }
}