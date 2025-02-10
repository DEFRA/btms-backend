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
    [InlineData(ImportNotificationTypeEnum.Cveda, false, "H219")]
    [InlineData(ImportNotificationTypeEnum.Ced, false, "H219")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, false, "H219")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, true, "H219")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, true, "H218")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, true, "H220")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, false, null)]
    public void CanFindDecisionTest(ImportNotificationTypeEnum? importNotificationType, bool expectedResult, string? checkCode)
    {
        var notification = new ImportNotification
        {
            ImportNotificationType = importNotificationType,
        };
        var sut = new ChedPPDecisionFinder();

        var result = sut.CanFindDecision(notification, checkCode);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(ImportNotificationStatusEnum.Amend, DecisionCode.E99)]
    [InlineData(ImportNotificationStatusEnum.Cancelled, DecisionCode.E99)]
    [InlineData(ImportNotificationStatusEnum.Deleted, DecisionCode.E99)]
    [InlineData(ImportNotificationStatusEnum.Draft, DecisionCode.E99)]
    [InlineData(ImportNotificationStatusEnum.InProgress, DecisionCode.H02)]
    [InlineData(ImportNotificationStatusEnum.Submitted, DecisionCode.H02)]
    [InlineData(ImportNotificationStatusEnum.Modify, DecisionCode.E99)]
    [InlineData(ImportNotificationStatusEnum.PartiallyRejected, DecisionCode.H01)]
    [InlineData(ImportNotificationStatusEnum.Rejected, DecisionCode.N02)]
    [InlineData(ImportNotificationStatusEnum.Replaced, DecisionCode.E99)]
    [InlineData(ImportNotificationStatusEnum.SplitConsignment, DecisionCode.E99)]
    public void DecisionFinderTest(ImportNotificationStatusEnum status, DecisionCode expectedCode)
    {
        var notification = new ImportNotification
        {
            Status = status
        };
        var sut = new ChedPPDecisionFinder();

        var result = sut.FindDecision(notification, null);

        result.DecisionCode.Should().Be(expectedCode);
        result.CheckCode.Should().BeNull();
    }
}