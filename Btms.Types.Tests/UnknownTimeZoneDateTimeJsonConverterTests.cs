using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Model.Ipaffs;
using FluentAssertions;

namespace Btms.Types.Tests;

public class UnknownTimeZoneDateTimeJsonConverterTests
{
    private readonly DateTime dt = new DateTime(2024,1,1, 12, 0, 0, DateTimeKind.Utc);
    
    [Fact]
    public void CorrectlySerialises()
    {
        var d = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        var applicant = new Applicant() { SampledOn = d };
        var s = applicant.ToJsonString();
        s.Should().Be("{\"sampledOn\":\"2024-01-01T12:00:00\"}");
    }
    
    [Fact]
    public void CorrectlyDeserialisesTimeWithMilliseconds()
    {
        var s = "{ \"overriddenOn\":\"2024-01-01T12:00:00.00\"}";
        var inspectionOverride = JsonSerializer.Deserialize<Btms.Types.Ipaffs.InspectionOverride>(s)!;
        inspectionOverride.OverriddenOn.Should().Be(dt);
        inspectionOverride.OverriddenOn!.Value.Kind.Should().Be(DateTimeKind.Unspecified);
    }
    
    [Fact]
    public void CorrectlyDeserialises()
    {
        var s = "{ \"overriddenOn\":\"2024-01-01T12:00\"}";
        var inspectionOverride = JsonSerializer.Deserialize<Btms.Types.Ipaffs.InspectionOverride>(s)!;
        inspectionOverride.OverriddenOn.Should().Be(dt);
        inspectionOverride.OverriddenOn!.Value.Kind.Should().Be(DateTimeKind.Unspecified);
    }
    
    [Fact]
    public void ThrowsExceptionDuringDeserialisationForUtcDate()
    {
        var s = "{ \"overriddenOn\":\"2024-01-01T12:00:00.00Z\"}";
        
        Action act = () => JsonSerializer.Deserialize<Btms.Types.Ipaffs.InspectionOverride>(s);
        act.Should().Throw<FormatException>()
            .WithMessage($"Invalid Value in OverriddenOn, value=2024-01-01T12:00:00.00Z. Unknown TimeZone dates must be DateTimeKind.Unspecified, not Utc");
    }
}