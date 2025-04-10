using Btms.Model;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests;

public class MatchingIdentifierTests
{
    [Theory]
    [InlineData("CHEDP.GB.2024.1036543", "C640", "1036543")]
    [InlineData("CHEDA.GB.2024.1036543", "C640", "1036543")]
    [InlineData("CHEDD.GB.2024.1036543", "C640", "1036543")]
    [InlineData("CHEDPP.GB.2024.1036543", "C640", "1036543")]
    [InlineData("CHEDP.GB.2024.1036543r", "C640", "1036543")]
    [InlineData("CHEDA.GB.2024.1036543r", "C640", "1036543")]
    [InlineData("CHEDD.GB.2024.1036543r", "C640", "1036543")]
    [InlineData("CHEDPP.GB.2024.1036543r", "C640", "1036543")]
    [InlineData("chedp.GB.2024.1036543v", "C640", "1036543")]
    [InlineData("cheda.GB.2024.1036543v", "C640", "1036543")]
    [InlineData("chedd.GB.2024.1036543v", "C640", "1036543")]
    [InlineData("chedpp.GB.2024.1036543v", "C640", "1036543")]
    [InlineData("chedppGB.2024.1036543v", "C640", "1036543")]
    [InlineData("chedpp.GB2024.1036543v", "C640", "1036543")]
    [InlineData("chedpp.GB.20241036543v", "C640", "1036543")]
    [InlineData("chedppGB2024.1036543v", "C640", "1036543")]
    [InlineData("chedpp.GB20241036543v", "C640", "1036543")]
    [InlineData("chedppGB.20241036543v", "C640", "1036543")]
    [InlineData("GBCVD2024.1036543v", "C640", "1036543")]
    [InlineData("GBCHD2024.1036543v", "C640", "1036543")]
    [InlineData("GBCVD20241036543v", "C640", "1036543")]
    [InlineData("GBCHD20241036543v", "C640", "1036543")]
    [InlineData("gbchd20241036543v", "C640", "1036543")]
    [InlineData("gbchd20241036543V", "C640", "1036543")]
    [InlineData("GBchd20241036543V", "C640", "1036543")]
    [InlineData("GBCHD241036543v", "C640", "1036543")]
    [InlineData("GBCHD.241036543v", "C640", "1036543")]
    [InlineData("GBCHD24.1036543v", "C640", "1036543")]
    [InlineData("GBCHD.24.1036543v", "C640", "1036543")]
    [InlineData("DHBGC.24.1036543v", "C640", "1036543")]
    [InlineData("GBGBC.2024.1036543v", "C640", "1036543")]
    [InlineData("GBGBC.2024.12345678v", "9115", "2345678")]
    [InlineData("GBCHD2025..5401250", "C640", "5401250")]
    [InlineData("GBCHD2025. 5455685", "C640", "5455685")]
    [InlineData("GBCVD.2024..5325881", "C640", "5325881")]
    [InlineData("GBCHD2025.55800.83", "9115", "5580083")]
    [InlineData("GBCHD2025 5503131", "9115", "5503131")]
    [InlineData("GBCVD.2024..5325881", "9115", "5325881")]
    public void ReferenceNumber_FromDocumentReference_Valid(string reference, string documentCode, string identifier)
    {
        MatchIdentifier.FromCds(reference, documentCode).Identifier.Should().Be(identifier);
    }

    [Theory]
    [InlineData("chedppGB.20241036543i", "C940")]
    [InlineData("chedppGB.2024103654i", "C940")]
    [InlineData("chedppG.20241036543i", "C940")]
    [InlineData("cheppGB.20241036543i", "C940")]
    [InlineData("GBCHD20241036543t", "C940")]
    [InlineData("GBCHD2024103654v", "C940")]
    [InlineData("GBCH20241036543v", "NAB2")]
    public void ReferenceNumber_FromDocumentReference_InValid(string reference, string documentCode)
    {
        Action act = () => MatchIdentifier.FromCds(reference, documentCode);

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
        var referenceNumber = MatchIdentifier.FromCds("GBCHD2024.1036543", "C640");

        referenceNumber.Identifier.Should().Be("1036543");
    }

    [Fact]
    public void ReferenceNumber_FromCdss_HasSplitIndicator()
    {
        var referenceNumber = MatchIdentifier.FromCds("GBCHD2024.1036543V", "C640");

        referenceNumber.Identifier.Should().Be("1036543");
    }
}