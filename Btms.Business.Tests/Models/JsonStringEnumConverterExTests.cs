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
        new JsonStringEnumConverterEx<LinkStatus>()
            .GetValue(LinkStatus.Investigate)
            .Should().Be(LinkStatus.Investigate.ToString());
    }
}