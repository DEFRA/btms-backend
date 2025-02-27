using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class IuuDecisionFinderTests
{
    [Theory]
    [InlineData(ImportNotificationTypeEnum.Cvedp, true, "H224")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, false, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, false, null)]

    [InlineData(ImportNotificationTypeEnum.Cveda, false, "H224")]
    [InlineData(ImportNotificationTypeEnum.Ced, false, "H224")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, false, "H224")]
    [InlineData(null, false, "H224")]
    public void CanFindDecisionTest(ImportNotificationTypeEnum? importNotificationType, bool expectedResult, string? checkCode)
    {
        var notification = new ImportNotification
        {
            ImportNotificationType = importNotificationType
        };
        var sut = new IuuDecisionFinder();

        var result = sut.CanFindDecision(notification, checkCode);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(true, ControlAuthorityIuuOptionEnum.Iuuok, DecisionCode.C07, "IUU Compliant")]
    [InlineData(true, ControlAuthorityIuuOptionEnum.IUUNotCompliant, DecisionCode.X00, "IUU Not compliant")]
    [InlineData(true, ControlAuthorityIuuOptionEnum.Iuuna, DecisionCode.C08, "IUU Not applicable")]
    [InlineData(true, null, DecisionCode.X00, "IUU Awaiting decision")]
    [InlineData(true, (ControlAuthorityIuuOptionEnum)999, DecisionCode.E95, "IUU Data error")]
    [InlineData(false, ControlAuthorityIuuOptionEnum.Iuuok, DecisionCode.E94, "IUU Data error")]
    public void FindDecisionTest(bool iuuCheckRequired, ControlAuthorityIuuOptionEnum? iuuOption, DecisionCode expectedDecisionCode, string? expectedDecisionReason)
    {
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

        var result = sut.FindDecision(notification, IuuDecisionFinder.IuuCheckCode);

        result.DecisionCode.Should().Be(expectedDecisionCode);
        result.DecisionReason.Should().StartWith(expectedDecisionReason);
        result.CheckCode.Should().Be(IuuDecisionFinder.IuuCheckCode);
    }
}