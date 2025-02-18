using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Types.Alvs;
using FluentAssertions;

namespace Btms.Types.Tests;

public class EpochDateTimeJsonConverterTests
{
    [Fact]
    public void CorrectlySerialises()
    {
        var d = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);
        var header = new Header() { ArrivalDateTime = d };
        var s = header.ToJsonString();
        s.Should().NotBeNull();
    }
    
    [Fact]
    public void CorrectlyDeserialisesTimeWithMilliseconds()
    {
        var s = "{ \"arrivalDateTime\":\"1739877116000\"}";
        var o = JsonSerializer.Deserialize<Header>(s);
        o.ArrivalDateTime.Should().NotBeNull();
    }
    
    [Fact]
    public void CorrectlyDeserialises()
    {
        var s = "{ \"arrivalDateTime\":\"1739877116\"}";
        var o = JsonSerializer.Deserialize<Header>(s);
        o.ArrivalDateTime.Should().NotBeNull();
    }
}