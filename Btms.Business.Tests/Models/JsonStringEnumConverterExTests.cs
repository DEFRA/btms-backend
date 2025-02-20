using Btms.Common.Enum;
using Btms.Model;
using Btms.Model.Cds;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Models;

public class JsonStringEnumConverterExTests
{
    [Fact]
    public void ShouldCreateLookupForLinkStatusEnum()
    {
        new JsonStringEnumConverterEx<LinkStatusEnum>()
            .GetValue(LinkStatusEnum.Investigate)
            .Should().Be(LinkStatusEnum.Investigate.ToString());
    }
}