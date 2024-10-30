using Cdms.Types.Ipaffs;
using FluentAssertions;
using Xunit;

namespace TestDataGenerator.Tests;

public class ClearanceRequestBuilderTests
{
    [Fact]
    public void WithReferenceNumber_WithChedA_ShouldCreateCorrectReference()
    {
        var builder = ClearanceRequestBuilder.Default();
        builder.WithFirstReferenceNumber("123");
        var cr = builder.Build();
        cr.Header!.EntryReference.Should().Be("123");
    }
}