using System.Text.Json.Nodes;
using Btms.SensitiveData;
using Btms.Types.Ipaffs;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Btms.Backend.IntegrationTests;

public class SensitiveDataTests
{
    [Fact]
    public void WhenIncludeSensitiveData_RedactedShouldBeSameAsJson()
    {
        string json =
            File.ReadAllText(Path.GetFullPath("..\\..\\..\\Fixtures\\SmokeTest\\IPAFFS\\CHEDA\\CHEDA_GB_2024_1041389-ee0e6fcf-52a4-45ea-8830-d4553ee70361.json"));
                
        SensitiveDataOptions options = new SensitiveDataOptions { Getter = s => "TestRedacted", Include = true };
        var serializer = new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance);

        var result = serializer.RedactRawJson(json, typeof(ImportNotification));

        JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(result)).Should().BeTrue();

    }

    [Fact]
    public void WhenIncludeSensitiveData_RedactedShouldBeDifferentJson()
    {
        string json =
            File.ReadAllText(Path.GetFullPath("..\\..\\..\\Fixtures\\SmokeTest\\IPAFFS\\CHEDA\\CHEDA_GB_2024_1041389-ee0e6fcf-52a4-45ea-8830-d4553ee70361.json"));

        SensitiveDataOptions options = new SensitiveDataOptions { Getter = s => "TestRedacted", Include = false };
        var serializer = new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance);

        var result = serializer.RedactRawJson(json, typeof(ImportNotification));

        JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(result)).Should().BeFalse();
        result.Should().Contain("TestRedacted");

    }
}