using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class ChedDDecisionFinderTests
{
    [Theory]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Submitted, true)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Amend, true)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.InProgress, true)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Modify, true)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.PartiallyRejected, true)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Rejected, true)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.SplitConsignment, true)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Validated, true)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Replaced, false)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Cancelled, false)]
    [InlineData(null, ImportNotificationTypeEnum.Cveda, ImportNotificationStatusEnum.Submitted, false)]
    [InlineData(null, ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Submitted, false)]
    [InlineData(null, ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Submitted, false)]
    [InlineData(false, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Submitted, true)]
    [InlineData(true, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Submitted, false)]
    public void CanFindDecisionTest(bool? iuuCheckRequired, ImportNotificationTypeEnum? importNotificationType, ImportNotificationStatusEnum notificationStatus, bool expectedResult)
    {
        var notification = new ImportNotification
        {
            Status = notificationStatus,
            ImportNotificationType = importNotificationType,
            PartTwo = new PartTwo
            {
                ControlAuthority = new ControlAuthority
                {
                    IuuCheckRequired = iuuCheckRequired
                }
            }
        };
        var sut = new ChedDDecisionFinder();

        var result = sut.CanFindDecision(notification, null);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(true, DecisionDecisionEnum.AcceptableForInternalMarket, null, DecisionCode.C03)]

    [InlineData(true, DecisionDecisionEnum.AcceptableForTranshipment, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransit, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTemporaryImport, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.HorseReEntry, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.NonAcceptable, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableIfChanneled, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForSpecificWarehouse, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForPrivateImport, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransfer, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]

    [InlineData(null, null, null, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]

    [InlineData(false, null, DecisionNotAcceptableActionEnum.Redispatching, DecisionCode.N04)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Destruction, DecisionCode.N02)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Transformation, DecisionCode.N03)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Other, DecisionCode.N07)]


    [InlineData(false, null, DecisionNotAcceptableActionEnum.Euthanasia, DecisionCode.X00, DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Reexport, DecisionCode.X00, DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Slaughter, DecisionCode.X00, DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.EntryRefusal, DecisionCode.X00, DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.QuarantineImposed, DecisionCode.X00, DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.SpecialTreatment, DecisionCode.X00, DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.IndustrialProcessing, DecisionCode.X00, DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.ReDispatch, DecisionCode.X00, DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.UseForOtherPurposes, DecisionCode.X00, DecisionInternalFurtherDetail.E97)]

    public void DecisionFinderTest(bool? consignmentAcceptable, DecisionDecisionEnum? decision, DecisionNotAcceptableActionEnum? notAcceptableAction, DecisionCode expectedCode, DecisionInternalFurtherDetail? expectedFurtherDetail = null)
    {
        var notification = new ImportNotification
        {
            PartTwo = new PartTwo
            {
                Decision = new Decision
                {
                    ConsignmentAcceptable = consignmentAcceptable,
                    DecisionEnum = decision,
                    NotAcceptableAction = notAcceptableAction
                }
            }
        };
        var sut = new ChedDDecisionFinder();

        var result = sut.FindDecision(notification, null);

        result.DecisionCode.Should().Be(expectedCode);
        result.InternalDecisionCode.Should().Be(expectedFurtherDetail);
        result.CheckCode.Should().BeNull();
    }
}