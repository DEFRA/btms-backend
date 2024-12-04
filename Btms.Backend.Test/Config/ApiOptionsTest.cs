using Btms.Backend.Config;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Btms.Backend.Test.Config;

public class ApiOptionsTest
{
    private readonly IValidateOptions<ApiOptions> validator = new ApiOptions.Validator();
    
    [Fact]
    public void ShouldSucceedIfNoCdsProxy()
    {
        var c = new ApiOptions();

        validator.Validate("", c).Should().Be(ValidateOptionsResult.Success);
    }
    
    [Fact]
    public void ShouldFailIfCdsProxyDoesntHaveProtocol()
    {
        var c = new ApiOptions() { CdpHttpsProxy = "aaa" };
        
        validator.Validate("", c).Failed.Should().BeTrue();
    }
    
    [Fact]
    public void ShouldSetHttpsProxyIfCdsProxyPresent()
    {
        var c = new ApiOptions { CdpHttpsProxy = "https://aaa" };

        c.HttpsProxy.Should().Be("aaa");
    }
    
    [Fact]
    public void ShouldNotSetHttpsProxyIfCdsProxyPresent()
    {
        var c = new ApiOptions();

        c.HttpsProxy.Should().BeNull();
    }
}