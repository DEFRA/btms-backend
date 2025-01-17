using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class IuuDecisionFinderTests
{
    [Theory]
    [InlineData(null, ImportNotificationTypeEnum.Cvedp, false)]
    [InlineData(null, null, false)]
    [InlineData(false, ImportNotificationTypeEnum.Cvedp, false)]
    [InlineData(false, null, false)]
    [InlineData(true, ImportNotificationTypeEnum.Cvedp, true)]
    [InlineData(true, null, true)]
    public void CanFindDecisionTest(bool? iuuCheckRequired, ImportNotificationTypeEnum? importNotificationType, bool expectedResult)
    {
        var notification = new ImportNotification
        {
            ImportNotificationType = importNotificationType,
            PartTwo = new PartTwo
            {
                ControlAuthority = new ControlAuthority
                {
                    IuuCheckRequired = iuuCheckRequired
                }
            }
        };
        var sut = new IuuDecisionFinder();

        var result = sut.CanFindDecision(notification);

        result.Should().Be(expectedResult);
    }
    
    [Theory]
    [InlineData(ControlAuthorityIuuOptionEnum.Iuuok, DecisionCode.C07, "IUU OK")]
    [InlineData(ControlAuthorityIuuOptionEnum.IUUNotCompliant, DecisionCode.X00, "IUU Not Compliant")]
    [InlineData(ControlAuthorityIuuOptionEnum.Iuuna, DecisionCode.C08, "IUU NA")]
    [InlineData(null, DecisionCode.X00, "IUU None")]
    [InlineData((ControlAuthorityIuuOptionEnum)999, DecisionCode.E95, "IUU Unknown")]
    public void FindDecisionTest(ControlAuthorityIuuOptionEnum? iuuOption, DecisionCode expectedDecisionCode, string? expectedDecisionReason)
    {
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

        var result = sut.FindDecision(notification);

        result.DecisionCode.Should().Be(expectedDecisionCode);
        result.DecisionReason.Should().Be(expectedDecisionReason);
        result.DecisionType.Should().Be(DecisionType.Iuu);
    }
}