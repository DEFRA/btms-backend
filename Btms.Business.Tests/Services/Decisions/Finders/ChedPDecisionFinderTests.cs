using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class ChedPDecisionFinderTests
{
    [Theory]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Submitted, true, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Amend, true, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.InProgress, true, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Modify, true, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.PartiallyRejected, true, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Rejected, true, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.SplitConsignment, true, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Validated, true, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Replaced, false, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Cancelled, false, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cveda, ImportNotificationStatusEnum.Submitted, false, "H222")]
    [InlineData(ImportNotificationTypeEnum.Ced, ImportNotificationStatusEnum.Submitted, false, "H222")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ImportNotificationStatusEnum.Submitted, false, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Submitted, true, null)]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ImportNotificationStatusEnum.Submitted, false, "H224")]
    public void CanFindDecisionTest(ImportNotificationTypeEnum? importNotificationType,
        ImportNotificationStatusEnum notificationStatus, bool expectedResult, string? checkCode)
    {
        var notification = new ImportNotification
        {
            Status = notificationStatus, ImportNotificationType = importNotificationType,
        };
        var sut = new ChedPDecisionFinder();

        var result = sut.CanFindDecision(notification, checkCode);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(true, DecisionDecisionEnum.AcceptableForInternalMarket, null, DecisionCode.C03)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransit, null, DecisionCode.E03)]
    [InlineData(true, DecisionDecisionEnum.AcceptableIfChanneled, null, DecisionCode.C06)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTranshipment, null, DecisionCode.E03)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForSpecificWarehouse, null, DecisionCode.E03)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTemporaryImport, null, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.HorseReEntry, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.NonAcceptable, null, DecisionCode.X00, DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForPrivateImport, null, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransfer, null, DecisionCode.X00,
        DecisionInternalFurtherDetail.E96)]
    [InlineData(null, null, null, DecisionCode.X00, DecisionInternalFurtherDetail.E99)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Reexport, DecisionCode.N04)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Destruction, DecisionCode.N02)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Transformation, DecisionCode.N03)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Other, DecisionCode.N07)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Euthanasia, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Slaughter, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Redispatching, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.EntryRefusal, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.QuarantineImposed, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.SpecialTreatment, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.IndustrialProcessing, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.ReDispatch, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.UseForOtherPurposes, DecisionCode.X00,
        DecisionInternalFurtherDetail.E97)]
    public void DecisionFinderTest(bool? consignmentAcceptable, DecisionDecisionEnum? decision,
        DecisionNotAcceptableActionEnum? notAcceptableAction, DecisionCode expectedCode,
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
                    NotAcceptableAction = notAcceptableAction
                }
            }
        };
        var sut = new ChedPDecisionFinder();

        var result = sut.FindDecision(notification, null);

        result.DecisionCode.Should().Be(expectedCode);
        result.InternalDecisionCode.Should().Be(expectedFurtherDetail);
        result.CheckCode.Should().BeNull();
    }
}