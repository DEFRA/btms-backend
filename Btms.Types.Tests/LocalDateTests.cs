using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using FluentAssertions;

namespace Btms.Types.Tests;

public class LocalDateTests
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
    public void ThrowsExceptionForUtcDate()
    {
        var d = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
        var applicant = new Applicant() { SampledOn = d };
        
        Action act = () => applicant.ToJsonString();
        act.Should().Throw<FormatException>()
            .WithMessage($"Local dates must be DateTimeKind.Unspecified, not Utc");
    }
}