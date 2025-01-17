using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class ChedDDecisionFinderTests
{
    [Theory]
    [InlineData(null, ImportNotificationTypeEnum.Cveda, false)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, true)]
    [InlineData(null, ImportNotificationTypeEnum.Cvedp, false)]
    [InlineData(null, ImportNotificationTypeEnum.Chedpp, false)]
    [InlineData(false, ImportNotificationTypeEnum.Ced, true)]
    [InlineData(true, ImportNotificationTypeEnum.Ced, true)]
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
        var sut = new ChedDDecisionFinder();

        var result = sut.CanFindDecision(notification);

        result.Should().Be(expectedResult);
    }
    
    [Theory]
    [InlineData(true, DecisionDecisionEnum.AcceptableForInternalMarket, null, DecisionCode.C03)]

    [InlineData(true, DecisionDecisionEnum.AcceptableForTranshipment, null, DecisionCode.X00)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransit, null, DecisionCode.X00)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTemporaryImport, null, DecisionCode.X00)]
    [InlineData(true, DecisionDecisionEnum.HorseReEntry, null, DecisionCode.X00)]
    [InlineData(true, DecisionDecisionEnum.NonAcceptable, null, DecisionCode.X00)]
    [InlineData(true, DecisionDecisionEnum.AcceptableIfChanneled, null, DecisionCode.X00)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForSpecificWarehouse, null, DecisionCode.X00)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForPrivateImport, null, DecisionCode.X00)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransfer, null, DecisionCode.X00)]

    [InlineData(null, null, null, DecisionCode.X00)]

    [InlineData(false, null, DecisionNotAcceptableActionEnum.Redispatching, DecisionCode.N04)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Destruction, DecisionCode.N02)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Transformation, DecisionCode.N03)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Other, DecisionCode.N07)]

    [InlineData(false, null, DecisionNotAcceptableActionEnum.Euthanasia, DecisionCode.X00)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Reexport, DecisionCode.X00)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Slaughter, DecisionCode.X00)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.EntryRefusal, DecisionCode.X00)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.QuarantineImposed, DecisionCode.X00)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.SpecialTreatment, DecisionCode.X00)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.IndustrialProcessing, DecisionCode.X00)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.ReDispatch, DecisionCode.X00)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.UseForOtherPurposes, DecisionCode.X00)]
    public void FindDecisionTest(bool? consignmentAcceptable, DecisionDecisionEnum? decision, DecisionNotAcceptableActionEnum? notAcceptableAction, DecisionCode expectedCode)
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

        var result = sut.FindDecision(notification);

        result.DecisionCode.Should().Be(expectedCode);
        result.DecisionType.Should().Be(DecisionType.Ched);
    }
}