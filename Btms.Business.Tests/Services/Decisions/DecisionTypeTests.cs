using Btms.Business.Services.Decisions;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions;

public class DecisionTypeTests
{
    [Theory]
    [InlineData(DecisionType.Iuu, "H224", true)]
    [InlineData(DecisionType.Iuu, "C673", true)]
    [InlineData(DecisionType.Iuu, "A111", false)]
    [InlineData(DecisionType.Iuu, "X999", false)]
    [InlineData(DecisionType.Ched, "H224", false)]
    [InlineData(DecisionType.Ched, "C673", false)]
    [InlineData(DecisionType.Ched, "A111", true)]
    [InlineData(DecisionType.Ched, "X999", true)]
    [InlineData(DecisionType.None, "H224", false)]
    [InlineData(DecisionType.None, "C673", false)]
    [InlineData(DecisionType.None, "A111", true)]
    [InlineData(DecisionType.None, "X999", true)]
    public void When_checking(DecisionType decisionType, string? checkCode, bool expectedResult)
    {
        decisionType.ForCheckCode(checkCode).Should().Be(expectedResult);
    }
}