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
        cr.Items![0].Documents![0].DocumentReference!.Should().Be("123");
    }
    
    [Fact]
    public void WithEntryDate_ShouldSet()
    {
        var builder = ClearanceRequestBuilder.Default();
        builder.WithEntryDate(DateTime.Today.AddDays(-5));
        
        var cr = builder.Build();
        cr.ServiceHeader!.ServiceCallTimestamp!.Should().BeAfter(DateTime.Today.AddDays(-5));
        cr.ServiceHeader!.ServiceCallTimestamp!.Should().BeBefore(DateTime.Today.AddDays(-4));
    }
}