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
    [InlineData(null, ImportNotificationTypeEnum.Cveda, ImportNotificationStatusEnum.Submitted, false)]
    [InlineData(null, ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Submitted, false)]
    [InlineData(null, ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Submitted, false)]
    [InlineData(false, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Submitted, true)]
    [InlineData(true, ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Submitted, false)]
    public void CanFindDecisionTest(bool? iuuCheckRequired, ImportNotificationTypeEnum? importNotificationType,
        ImportNotificationStatusEnum notificationStatus, bool expectedResult)
    {
        var notification = new ImportNotification
        {
            Status = notificationStatus,
            ImportNotificationType = importNotificationType,
            PartTwo = new PartTwo
            {
                ControlAuthority = new ControlAuthority { IuuCheckRequired = iuuCheckRequired }
            }
        };
        var sut = new ChedDDecisionFinder();

        var result = sut.CanFindDecision(notification, "H223");

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(true, DecisionDecisionEnum.AcceptableForInternalMarket, null, new[] { "Other" }, DecisionCode.C03)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTranshipment, null, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransit, null, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTemporaryImport, null, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.HorseReEntry, null, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.NonAcceptable, null, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableIfChanneled, null, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForSpecificWarehouse, null, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForPrivateImport, null, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransfer, null, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(null, null, null, new[] { "Other" }, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]
    [InlineData(false, null, null, new[] { "Other" }, DecisionCode.N04)]
    [InlineData(false, null, null, new[] { "PhysicalHygieneFailure", "ChemicalContamination" }, DecisionCode.N04)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Redispatching, new[] { "Other" }, DecisionCode.N04)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Destruction, null, DecisionCode.N02)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Transformation, new[] { "Other" }, DecisionCode.N03)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Other, new[] { "Other" }, DecisionCode.N07)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Euthanasia, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Reexport, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Slaughter, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.EntryRefusal, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.QuarantineImposed, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.SpecialTreatment, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.IndustrialProcessing, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.ReDispatch, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.UseForOtherPurposes, new[] { "Other" }, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    public void DecisionFinderTest(bool? consignmentAcceptable, DecisionDecisionEnum? decision,
        DecisionNotAcceptableActionEnum? notAcceptableAction, String[]? notAcceptableReasons, DecisionCode expectedCode,
        DecisionInternalFurtherDetail? expectedFurtherDetail = null)
    {
        var notification = new ImportNotification
        {
            PartTwo = new PartTwo
            {
                Decision = new Decision
                {
                    ConsignmentAcceptable = consignmentAcceptable,
                    DecisionEnum = decision,
                    NotAcceptableAction = notAcceptableAction,
                    NotAcceptableReasons = notAcceptableReasons
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