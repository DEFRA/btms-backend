using Cdms.Model;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Cdms.Business.Tests;

public class MatchingIdentifierTests(ITestOutputHelper output)
{
    [Fact]
    public void ReferenceNumber_FromIpaffs_NoSplitIndicator()
    {
        var referenceNumber =
            MatchIdentifier.FromNotification("CHEDP.GB.2024.1036543");

        referenceNumber.Identifier.Should().Be("1036543");
    }

    [Fact]
    public void ReferenceNumber_FromIpaffs_HasSplitIndicator()
    {
        var referenceNumber =
            MatchIdentifier.FromNotification("CHEDP.GB.2024.1036543V");

        referenceNumber.Identifier.Should().Be("1036543");
    }

    [Fact]
    public void ReferenceNumber_FromCds_NoSplitIndicator()
    {
        var referenceNumber = MatchIdentifier.FromCds("GBCHD2024.1036543");

        referenceNumber.Identifier.Should().Be("1036543");
    }

    [Fact]
    public void ReferenceNumber_FromCdss_HasSplitIndicator()
    {
        var referenceNumber = MatchIdentifier.FromCds("GBCHD2024.1036543V");

        referenceNumber.Identifier.Should().Be("1036543");
    }
}