using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

// ReSharper disable once InconsistentNaming
public class ChedPPDecisionFinderTests
{
    [Theory]
    [InlineData(null, ImportNotificationTypeEnum.Cveda, false)]
    [InlineData(null, ImportNotificationTypeEnum.Ced, false)]
    [InlineData(null, ImportNotificationTypeEnum.Cvedp, false)]
    [InlineData(null, ImportNotificationTypeEnum.Chedpp, true)]
    [InlineData(false, ImportNotificationTypeEnum.Chedpp, true)]
    [InlineData(true, ImportNotificationTypeEnum.Chedpp, false)]
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
        var sut = new ChedPPDecisionFinder();

        var result = sut.CanFindDecision(notification);

        result.Should().Be(expectedResult);
    }
    
    [Fact]
    public async Task FindDecisionTest()
    {
        var sut = new ChedPPDecisionFinder();
        

        await Task.CompletedTask;
    }
}