using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace TestDataGenerator.Tests;

public class ClearanceRequestBuilderTests
{
    [Fact]
    public void WithReferenceNumber_WithChedA_ShouldCreateCorrectReference()
    {
        var builder = ClearanceRequestBuilder.Default();
        builder.WithReferenceNumberOneToOne("CHEDA.GB.2024.123");
        var cr = builder.Build();
        cr.Items![0].Documents![0].DocumentReference!.Should().Be("GBCHD2024.123");
    }

    [Fact]
    public void WithEntryDate_ShouldSet()
    {
        var date = DateTime.Today.AddDays(-5);
        var builder = ClearanceRequestBuilder.Default();
        builder.WithCreationDate(date);

        var cr = builder.Build();
        cr.ServiceHeader!.ServiceCallTimestamp.ToDate().Should().Be(date.ToDate());
    }
}