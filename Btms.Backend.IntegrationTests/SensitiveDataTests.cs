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
    private const string FilePath =
        "../../../../TestDataGenerator/Scenarios/Samples/SmokeTest/IPAFFS/CHEDA/CHEDA_GB_2024_1041389-ee0e6fcf-52a4-45ea-8830-d4553ee70361.json";
    
    [Fact]
    public void WhenIncludeSensitiveData_RedactedShouldBeSameAsJson()
    {
        var json = File.ReadAllText(FilePath);
        var options = new SensitiveDataOptions { Getter = _ => "TestRedacted", Include = true };
        var serializer = new SensitiveDataSerializer(Options.Create(options),
            NullLogger<SensitiveDataSerializer>.Instance, new SensitiveFieldsProvider());

        var result = serializer.RedactRawJson(json, typeof(ImportNotification));

        JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(result)).Should().BeTrue();
    }

    [Fact]
    public void WhenIncludeSensitiveData_RedactedShouldBeDifferentJson()
    {
        var json = File.ReadAllText(FilePath);
        var options = new SensitiveDataOptions { Getter = _ => "TestRedacted", Include = false };
        var serializer = new SensitiveDataSerializer(Options.Create(options),
            NullLogger<SensitiveDataSerializer>.Instance, new SensitiveFieldsProvider());

        var result = serializer.RedactRawJson(json, typeof(ImportNotification));

        JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(result)).Should().BeFalse();
        result.Should().Contain("TestRedacted");
    }

    [Fact]
    public void WhenIncludeSensitiveData_DataShouldBeRedacted()
    {
        var json = File.ReadAllText(FilePath);
        var options = new SensitiveDataOptions { Getter = _ => "TestRedacted", Include = false };
        var serializer = new SensitiveDataSerializer(Options.Create(options),
            NullLogger<SensitiveDataSerializer>.Instance, new SensitiveFieldsProvider());

        var result = serializer.Deserialize<ImportNotification>(json);

        result.PartOne?.Consignee?.Address?.AddressLine1.Should().Contain("TestRedacted");
    }
}