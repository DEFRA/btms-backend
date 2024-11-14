using CdmsBackend.Config;
using FluentAssertions;

namespace CdmsBackend.Test.Config;

public class ApiOptionsTest
{
    [Fact]
    public void ShouldSucceedIfNoCdsProxy()
    {
        var c = new ApiOptions() { };

        c.Validate().Should().BeTrue();
    }
    
    [Fact]
    public void ShouldSucceedIfCdsProxyAndHttpsProxy()
    {
        var c = new ApiOptions() { CdpHttpsProxy = "https://aaa", HttpsProxy = "https://aaa", };

        c.Validate().Should().BeTrue();
    }
    
    [Fact]
    public void ShouldFailIfCdsProxyAndNotHttpsProxy()
    {
        var c = new ApiOptions() { CdpHttpsProxy = "https://aaa" };

        c.Validate().Should().BeFalse();
    }
}