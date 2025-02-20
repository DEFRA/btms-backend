using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Model.Gvms;
using FluentAssertions;

namespace Btms.Types.Tests;

public class UnknownTimeZoneDateTimeJsonConverterTests
{
    private readonly DateTime dt = new DateTime(2024,1,1, 12, 0, 0, DateTimeKind.Utc);
    
    [Fact]
    public void CorrectlySerialises()
    {
        var d = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        var applicant = new ActualCrossing() { ArrivesAt = d };
        var s = applicant.ToJsonString();
        s.Should().Be("{\"arrivesAt\":\"2024-01-01T12:00:00\"}");
    }
    
    [Fact]
    public void CorrectlyDeserialisesTimeWithMilliseconds()
    {
        var s = "{ \"arrivesAt\":\"2024-01-01T12:00:00.00\"}";
        var inspectionOverride = JsonSerializer.Deserialize<ActualCrossing>(s)!;
        inspectionOverride.ArrivesAt.Should().Be(dt);
        inspectionOverride.ArrivesAt!.Value.Kind.Should().Be(DateTimeKind.Unspecified);
    }
    
    [Fact]
    public void CorrectlyDeserialises()
    {
        var s = "{ \"arrivesAt\":\"2024-01-01T12:00\"}";
        var inspectionOverride = JsonSerializer.Deserialize<ActualCrossing>(s)!;
        inspectionOverride.ArrivesAt.Should().Be(dt);
        inspectionOverride.ArrivesAt!.Value.Kind.Should().Be(DateTimeKind.Unspecified);
    }
    
    [Fact]
    public void ThrowsExceptionDuringDeserialisationForUtcDate()
    {
        var s = "{ \"arrivesAt\":\"2024-01-01T12:00:00.00Z\"}";
        
        Action act = () => JsonSerializer.Deserialize<ActualCrossing>(s);
        act.Should().Throw<FormatException>()
            .WithMessage($"Invalid Value in {nameof(ActualCrossing.ArrivesAt)}, value=2024-01-01T12:00:00.00Z. Unknown TimeZone dates must be DateTimeKind.Unspecified, not Utc");
    }
}