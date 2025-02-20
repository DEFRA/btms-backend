using Btms.Model;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests;

public class MatchingIdentifierTests
{
    [Theory]
    [InlineData("CHEDP.GB.2024.1036543", "1036543")]
    [InlineData("CHEDA.GB.2024.1036543", "1036543")]
    [InlineData("CHEDD.GB.2024.1036543", "1036543")]
    [InlineData("CHEDPP.GB.2024.1036543", "1036543")]
    [InlineData("CHEDP.GB.2024.1036543r", "1036543")]
    [InlineData("CHEDA.GB.2024.1036543r", "1036543")]
    [InlineData("CHEDD.GB.2024.1036543r", "1036543")]
    [InlineData("CHEDPP.GB.2024.1036543r", "1036543")]
    [InlineData("chedp.GB.2024.1036543v", "1036543")]
    [InlineData("cheda.GB.2024.1036543v", "1036543")]
    [InlineData("chedd.GB.2024.1036543v", "1036543")]
    [InlineData("chedpp.GB.2024.1036543v", "1036543")]
    [InlineData("chedppGB.2024.1036543v", "1036543")]
    [InlineData("chedpp.GB2024.1036543v", "1036543")]
    [InlineData("chedpp.GB.20241036543v", "1036543")]
    [InlineData("chedppGB2024.1036543v", "1036543")]
    [InlineData("chedpp.GB20241036543v", "1036543")]
    [InlineData("chedppGB.20241036543v", "1036543")]
    [InlineData("GBCVD2024.1036543v", "1036543")]
    [InlineData("GBCHD2024.1036543v", "1036543")]
    [InlineData("GBCVD20241036543v", "1036543")]
    [InlineData("GBCHD20241036543v", "1036543")]
    public void ReferenceNumber_FromDocumentReference_Valid(string reference, string identifier)
    {
        MatchIdentifier.FromCds(reference).Identifier.Should().Be(identifier);
    }

    [Theory]
    [InlineData("chedppGB.20241036543i")]
    [InlineData("chedppGB.2024103654i")]
    [InlineData("chedppG.20241036543i")]
    [InlineData("cheppGB.20241036543i")]
    [InlineData("GBCHD20241036543t")]
    [InlineData("GBCHD2024103654v")]
    [InlineData("GBCH20241036543v")]
    public void ReferenceNumber_FromDocumentReference_InValid(string reference)
    {
        Action act = () => MatchIdentifier.FromCds(reference);

        act.Should().Throw<FormatException>();
    }

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