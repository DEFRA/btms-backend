using Btms.Consumers.AmazonQueues;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public class AmazonLocalstackTests
{
    [Fact]
    public async Task When_checking_if_localstack_available_Then_should_be_available_in_development()
    {
        var awsConsumers = new TestAwsConsumers();
        var awsLocalOptions = awsConsumers.Configuration
            .GetSection(AwsLocalOptions.SectionName)
            .Get<AwsLocalOptions>();

        if (awsLocalOptions?.ServiceUrl != null)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{awsLocalOptions?.ServiceUrl.TrimEnd('/')}/_localstack/health");
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}