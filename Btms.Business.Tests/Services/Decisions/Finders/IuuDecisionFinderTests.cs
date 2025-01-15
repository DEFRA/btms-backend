using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class IuuDecisionFinderTests
{
    [Theory]
    [InlineData(true, null, DecisionCode.X00, "TBC")]
    [InlineData(true, ControlAuthorityIuuOptionEnum.Iuuok, DecisionCode.C07, "TBC")]
    [InlineData(true, ControlAuthorityIuuOptionEnum.IUUNotCompliant, DecisionCode.X00, "TBC")]
    [InlineData(true, ControlAuthorityIuuOptionEnum.Iuuna, DecisionCode.C08, "TBC")]
    [InlineData(false, null, DecisionCode.X00, null)]
    public void DecisionFinderTest(bool iuuCheckRequired, ControlAuthorityIuuOptionEnum? iuuOption, DecisionCode expectedDecisionCode, string? expectedDecisionReason)
    {
        // Arrange
        var notification = new ImportNotification
        {
            PartTwo = new PartTwo
            {
                ControlAuthority = new ControlAuthority
                {
                    IuuCheckRequired = iuuCheckRequired,
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