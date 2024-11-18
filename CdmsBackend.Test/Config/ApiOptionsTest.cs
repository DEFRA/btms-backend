using CdmsBackend.Config;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CdmsBackend.Test.Config;

public class ApiOptionsTest
{
    private IValidateOptions<ApiOptions> validator = new ApiOptions.Validator();
    
    [Fact]
    public void ShouldSucceedIfNoCdsProxy()
    {
        var c = new ApiOptions() { };

        validator.Validate("", c).Should().Be(ValidateOptionsResult.Success);
    }
    
    [Fact]
    public void ShouldSucceedIfCdsProxyAndHttpsProxy()
    {
        var c = new ApiOptions() { CdpHttpsProxy = "https://aaa", HttpsProxy = "aaa", };
    
        validator.Validate("", c).Should().Be(ValidateOptionsResult.Success);
    }
    
    [Fact]
    public void ShouldFailIfCdsProxyAndNotHttpsProxy()
    {
        var c = new ApiOptions() { CdpHttpsProxy = "https://aaa" };
    
        validator.Validate("", c).Failed.Should().BeTrue();
    }
    
    [Fact]
    public void ShouldFailIfCdsProxyDoesntHaveProtocol()
    {
        var c = new ApiOptions() { CdpHttpsProxy = "aaa", HttpsProxy = "aaa", };
        validator.Validate("", c).Failed.Should().BeTrue();
    }
    
    [Fact]
    public void ShouldFailIfHttpsProxyHasProtocol()
    {
        var c = new ApiOptions() { CdpHttpsProxy = "https://aaa", HttpsProxy = "https://aaa", };
        validator.Validate("", c).Failed.Should().BeTrue();
    }
}