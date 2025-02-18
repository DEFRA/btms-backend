using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Model.Ipaffs;
using FluentAssertions;

namespace Btms.Types.Tests;

public class UnknownTimeZoneDateTimeJsonConverterTests
{
    [Fact]
    public void CorrectlySerialises()
    {
        var d = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);
        var applicant = new Applicant() { SampledOn = d };
        var s = applicant.ToJsonString();
        s.Should().NotBeNull();
    }
    
    [Fact]
    public void CorrectlyDeserialisesTimeWithMilliseconds()
    {
        var s = "{ \"overriddenOn\":\"2024-10-31T22:00:05.02\"}";
        var o = JsonSerializer.Deserialize<Btms.Types.Ipaffs.InspectionOverride>(s);
        o.OverriddenOn.Should().NotBeNull();
    }
    
    [Fact]
    public void CorrectlyDeserialises()
    {
        var s = "{ \"overriddenOn\":\"2024-12-01T12:00\"}";
        var o = JsonSerializer.Deserialize<Btms.Types.Ipaffs.InspectionOverride>(s);
        o.OverriddenOn.Should().NotBeNull();
    }
    
    [Fact]
    public void ThrowsExceptionDuringDeserialisationForUtcDate()
    {
        var s = "{ \"overriddenOn\":\"2024-10-31T22:00:05.02Z\"}";
        
        Action act = () => JsonSerializer.Deserialize<Btms.Types.Ipaffs.InspectionOverride>(s);
        act.Should().Throw<FormatException>()
            .WithMessage($"Invalid Value in OverriddenOn, value=2024-10-31T22:00:05.02Z. Unknown TimeZone dates must be DateTimeKind.Unspecified, not Utc");
    }
}