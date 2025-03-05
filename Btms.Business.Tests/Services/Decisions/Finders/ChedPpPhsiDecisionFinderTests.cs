using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

// ReSharper disable once InconsistentNaming
public class ChedPpPhsiDecisionFinderTests
{
    [Theory]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Submitted, true, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Amend, true, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.InProgress, true, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Modify, true, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.PartiallyRejected, true, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Rejected, true, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.SplitConsignment, true, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Validated, true, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Replaced, false, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Cancelled, false, "H220")]
    [InlineData(ImportNotificationTypeEnum.Cveda, ImportNotificationStatusEnum.Submitted, false, "H219")]
    [InlineData(ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Submitted, false, "H219")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Submitted, false, "H219")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Submitted, true, "H219")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Submitted, true, "H218")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Submitted, false, null)]
    public void CanFindDecisionTest(ImportNotificationTypeEnum? importNotificationType,
        ImportNotificationStatusEnum notificationStatus, bool expectedResult, string? checkCode)
    {
        var notification = new ImportNotification
        {
            Status = notificationStatus, ImportNotificationType = importNotificationType,
        };
        var sut = new ChedPPDecisionFinder();

        var result = sut.CanFindDecision(notification, checkCode);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(ImportNotificationStatusEnum.Amend, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]
    [InlineData(ImportNotificationStatusEnum.Cancelled, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]
    [InlineData(ImportNotificationStatusEnum.Deleted, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]
    [InlineData(ImportNotificationStatusEnum.Draft, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]
    [InlineData(ImportNotificationStatusEnum.InProgress, DecisionCode.H02)]
    [InlineData(ImportNotificationStatusEnum.Submitted, DecisionCode.H02)]
    [InlineData(ImportNotificationStatusEnum.Modify, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]
    [InlineData(ImportNotificationStatusEnum.PartiallyRejected, DecisionCode.H01)]
    [InlineData(ImportNotificationStatusEnum.Rejected, DecisionCode.N02)]
    [InlineData(ImportNotificationStatusEnum.Replaced, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]
    [InlineData(ImportNotificationStatusEnum.SplitConsignment, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]
    public void DecisionFinderTest(ImportNotificationStatusEnum status, DecisionCode expectedCode,
        DecisionInternalFurtherDetail? expectedFurtherDetail = null)
    {
        var notification = new ImportNotification { Status = status };
        var sut = new ChedPPDecisionFinder();

        var result = sut.FindDecision(notification, null);

        result.DecisionCode.Should().Be(expectedCode);
        result.InternalDecisionCode.Should().Be(expectedFurtherDetail);
        result.CheckCode.Should().BeNull();
    }
}