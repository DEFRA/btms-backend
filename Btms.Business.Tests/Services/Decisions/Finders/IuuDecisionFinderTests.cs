using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class IuuDecisionFinderTests
{
    [Theory]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Submitted, true, "H224")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Amend, true, "H224")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.InProgress, true, "H224")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Modify, true, "H224")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.PartiallyRejected, true, "H224")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Rejected, true, "H224")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.SplitConsignment, true, "H224")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Validated, true, "H224")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Submitted, false, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Submitted, false, null)]
    [InlineData(ImportNotificationTypeEnum.Cveda, ImportNotificationStatusEnum.Submitted, false, "H224")]
    [InlineData(ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Submitted, false, "H224")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Submitted, false, "H224")]
    [InlineData(null, ImportNotificationStatusEnum.Submitted, false, "H224")]
    public void CanFindDecisionTest(ImportNotificationTypeEnum? importNotificationType, ImportNotificationStatusEnum notificationStatus, bool expectedResult, string? checkCode)
    {
        var notification = new ImportNotification
        {
            Status = notificationStatus,
            ImportNotificationType = importNotificationType
        };
        var sut = new IuuDecisionFinder();

        var result = sut.CanFindDecision(notification, checkCode);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(true, ControlAuthorityIuuOptionEnum.Iuuok, DecisionCode.C07, null, "IUU Compliant")]
    [InlineData(true, ControlAuthorityIuuOptionEnum.IUUNotCompliant, DecisionCode.X00, null, "IUU Not compliant")]
    [InlineData(true, ControlAuthorityIuuOptionEnum.Iuuna, DecisionCode.C08, null, "IUU Not applicable")]
    [InlineData(true, null, DecisionCode.X00, null, "IUU Awaiting decision")]
    [InlineData(true, (ControlAuthorityIuuOptionEnum)999, DecisionCode.X00, DecisionInternalFurtherDetail.E95, "IUU Data error")]
    [InlineData(false, ControlAuthorityIuuOptionEnum.Iuuok, DecisionCode.X00, DecisionInternalFurtherDetail.E94, "IUU Data error")]
    public void FindDecisionTest(bool iuuCheckRequired, ControlAuthorityIuuOptionEnum? iuuOption, DecisionCode expectedDecisionCode, DecisionInternalFurtherDetail? expectedFurtherDetail = null, string? expectedDecisionReason = null)
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
        result.InternalDecisionCode.Should().Be(expectedFurtherDetail);
        result.DecisionReason.Should().StartWith(expectedDecisionReason);
        result.CheckCode.Should().Be(IuuDecisionFinder.IuuCheckCode);
    }
}