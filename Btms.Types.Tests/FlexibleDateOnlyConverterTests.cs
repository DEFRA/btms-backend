using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Types.Ipaffs;
using FluentAssertions;

namespace Btms.Types.Tests;

public class FlexibleDateOnlyConverterTests
{
    private readonly DateOnly dt = new DateOnly(2024, 12, 01);
    
    [Fact]
    public void ShouldDeserialiseDatesWithoutTimes()
    {
        var s = "{ \"documentIssueDate\":\"2024-12-01\"}";
        var accompanyingDocument = JsonSerializer.Deserialize<AccompanyingDocument>(s)!;
        accompanyingDocument.DocumentIssueDate.Should().Be(dt);
    }
    
    [Fact]
    public void ShouldDeserialiseDatesWithTimes()
    {   
        var s = "{ \"documentIssueDate\":\"2024-12-01T12:00\"}";
        var accompanyingDocument = JsonSerializer.Deserialize<AccompanyingDocument>(s)!;
        accompanyingDocument.DocumentIssueDate.Should().Be(dt);
    }
    
    [Fact]
    public void ShouldSerialiseCorrectly()
    {
        var d = new AccompanyingDocument() { DocumentIssueDate = dt };

        var s = d.ToJsonString();
        s.Should().Contain("2024-12-01\"");
    }
}