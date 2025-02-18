using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Types.Alvs;
using FluentAssertions;

namespace Btms.Types.Tests;

public class EpochDateTimeJsonConverterTests
{
    private readonly DateTime dt = new DateTime(2024,1,1, 12, 0, 0, DateTimeKind.Utc);
    [Fact]
    public void CorrectlySerialises()
    {
        var header = new Header() { ArrivalDateTime = dt };
        var s = header.ToJsonString();
        s.Should().Be("{\"arrivalDateTime\":1704110400000}");
    }
    
    [Fact]
    public void CorrectlyDeserialisesTimeWithMilliseconds()
    {
        var s = "{ \"arrivalDateTime\":\"1704110400000\"}";
        var header = JsonSerializer.Deserialize<Header>(s)!;
        header.ArrivalDateTime.Should().Be(dt);
        header.ArrivalDateTime!.Value.Kind.Should().Be(DateTimeKind.Utc);

    }
    
    [Fact]
    public void CorrectlyDeserialises()
    {
        var s = "{ \"arrivalDateTime\":\"1704110400\"}";
        var header = JsonSerializer.Deserialize<Header>(s)!;
        header.ArrivalDateTime.Should().Be(dt);
        header.ArrivalDateTime!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }
}