using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class ChedDDecisionFinderTests
{
    [Theory]
    [InlineData(true, DecisionDecisionEnum.AcceptableForInternalMarket, null, DecisionCode.C03)]

    [InlineData(true, DecisionDecisionEnum.AcceptableForTranshipment, null, DecisionCode.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransit, null, DecisionCode.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTemporaryImport, null, DecisionCode.E96)]
    [InlineData(true, DecisionDecisionEnum.HorseReEntry, null, DecisionCode.E96)]
    [InlineData(true, DecisionDecisionEnum.NonAcceptable, null, DecisionCode.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableIfChanneled, null, DecisionCode.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForSpecificWarehouse, null, DecisionCode.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForPrivateImport, null, DecisionCode.E96)]
    [InlineData(true, DecisionDecisionEnum.AcceptableForTransfer, null, DecisionCode.E96)]

    [InlineData(null, null, null, DecisionCode.E99)]

    [InlineData(false, null, DecisionNotAcceptableActionEnum.Redispatching, DecisionCode.N04)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Destruction, DecisionCode.N02)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Transformation, DecisionCode.N03)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Other, DecisionCode.N07)]

    [InlineData(false, null, DecisionNotAcceptableActionEnum.Euthanasia, DecisionCode.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Reexport, DecisionCode.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.Slaughter, DecisionCode.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.EntryRefusal, DecisionCode.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.QuarantineImposed, DecisionCode.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.SpecialTreatment, DecisionCode.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.IndustrialProcessing, DecisionCode.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.ReDispatch, DecisionCode.E97)]
    [InlineData(false, null, DecisionNotAcceptableActionEnum.UseForOtherPurposes, DecisionCode.E97)]

    public void DecisionFinderTest(bool? consignmentAcceptable, DecisionDecisionEnum? decision, DecisionNotAcceptableActionEnum? notAcceptableAction, DecisionCode expectedCode)
    {
        // Arrange
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

        // Act
        var result = sut.FindDecision(notification);

        // Assert
        result.DecisionCode.Should().Be(expectedCode);
    }
}