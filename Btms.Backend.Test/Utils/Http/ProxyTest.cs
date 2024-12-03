using Amazon.Runtime.Internal.Util;
using Btms.Backend.Utils.Http;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;

namespace Btms.Backend.Test.Utils.Http;

public class ProxyTest
{

    private readonly string proxyUri = "http://user:password@localhost:8080";
    private readonly string localProxy = "http://localhost:8080/";
    private readonly string localhost = "http://localhost/";

    [Fact]
    public void ExtractProxyCredentials()
    {
        var proxy = new System.Net.WebProxy
        {
            BypassProxyOnLocal = true
        };

        Proxy.ConfigureProxy(proxy, proxyUri, NullLogger.Instance);

        var credentials = proxy.Credentials?.GetCredential(new System.Uri(proxyUri), "Basic");

        credentials?.UserName.Should().Be("user");
        credentials?.Password.Should().Be("password");
    }

    [Fact]
    public void ExtractProxyEmptyCredentials()
    {
        var noPasswordUri = "http://user@localhost:8080";

        var proxy = new System.Net.WebProxy
        {
            BypassProxyOnLocal = true
        };

        Proxy.ConfigureProxy(proxy, noPasswordUri, NullLogger.Instance);

        proxy.Credentials.Should().BeNull();
    }

    [Fact]
    public void ExtractProxyUri()
    {

        var proxy = new System.Net.WebProxy
        {
            BypassProxyOnLocal = true
        };

        Proxy.ConfigureProxy(proxy, proxyUri, NullLogger.Instance);
        proxy.Address.Should().NotBeNull();
        proxy.Address?.AbsoluteUri.Should().Be(localProxy);
    }

    [Fact]
    public void CreateProxyFromUri()
    {

        var proxy = Proxy.CreateProxy(proxyUri, NullLogger.Instance) as System.Net.WebProxy;
        proxy?.Address.Should().NotBeNull();
        proxy?.Address?.AbsoluteUri.Should().Be(localProxy);
    }

    [Fact]
    public void CreateNoProxyFromEmptyUri()
    {
        var proxy = Proxy.CreateProxy(null, NullLogger.Instance) as System.Net.WebProxy;


        proxy?.Address.Should().BeNull();
    }

    [Fact]
    public void ProxyShouldBypassLocal()
    {

        var proxy = Proxy.CreateProxy(proxyUri, NullLogger.Instance);

        proxy.IsBypassed(new Uri(localhost)).Should().BeTrue();
        proxy.IsBypassed(new Uri("https://defra.gov.uk")).Should().BeFalse();
    }

    [Fact]
    public void HandlerShouldHaveProxy()
    {
        var proxy = Proxy.CreateProxy(proxyUri, NullLogger.Instance);
        var handler = Proxy.CreateHttpClientHandler(proxy, proxyUri);

        handler.Proxy.Should().NotBeNull();
        handler.UseProxy.Should().BeTrue();
        handler.Proxy?.Credentials.Should().NotBeNull();
        handler.Proxy?.GetProxy(new Uri(localhost)).Should().NotBeNull();
        handler.Proxy?.GetProxy(new Uri("http://google.com")).Should().NotBeNull();
        handler.Proxy?.GetProxy(new Uri(localhost))?.AbsoluteUri.Should().Be(localhost);
        handler.Proxy?.GetProxy(new Uri("http://google.com"))?.AbsoluteUri.Should().Be(localProxy);
    }


}
