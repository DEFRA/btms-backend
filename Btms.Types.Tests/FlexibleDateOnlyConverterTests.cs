using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Types.Ipaffs;
using FluentAssertions;

namespace Btms.Types.Tests;

public class FlexibleDateOnlyConverterTests
{
    [Fact]
    public void ShouldDeserialiseDatesWithoutTimes()
    {
        var s = "{ \"documentIssueDate\":\"2024-12-01\"}";
        var o = JsonSerializer.Deserialize<AccompanyingDocument>(s);
        o.DocumentIssueDate.Should().NotBeNull();
    }
    
    [Fact]
    public void ShouldDeserialiseDatesWithTimes()
    {   
        var s = "{ \"documentIssueDate\":\"2024-12-01T12:00\"}";
        var o = JsonSerializer.Deserialize<AccompanyingDocument>(s);
        o.DocumentIssueDate.Should().NotBeNull();
    }
    
    [Fact]
    public void ShouldSerialiseCorrectly()
    {
        var d = new AccompanyingDocument() { DocumentIssueDate = new DateOnly(2024, 12, 01) };

        var s = d.ToJsonString();
        s.Should().Contain("2024-12-01\"");
    }
}