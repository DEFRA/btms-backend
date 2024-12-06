using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Types.Ipaffs;
using FluentAssertions;

namespace Btms.Backend.Test.JsonCoverters;

public class KeyDataPairsToDictionaryStringObjectJsonConverterTests
{
    public class TestClass
    {
        [JsonPropertyName("keyDataPair")]
        [JsonConverter(typeof(KeyDataPairsToDictionaryStringObjectJsonConverter))]
        public IDictionary<string, object>? KeyDataPairs { get; set; }
    }

    [Fact]
    public void GivenDataIsNotPresentInJson_ThenItShouldBeDeserializedAsNull()
    {
        var json =
            "{\"keyDataPair\": [\r\n                        {\r\n                            \"key\": \"netweight\"\r\n                        },\r\n                        {\r\n                            \"key\": \"number_package\",\r\n                            \"data\": \"0\"\r\n                        },\r\n                        {\r\n                            \"key\": \"type_package\",\r\n                            \"data\": \"Case\"\r\n                        }\r\n                    ]}";

        var result = JsonSerializer.Deserialize<TestClass>(json);

        result.Should().NotBeNull();
        result?.KeyDataPairs?.Count.Should().Be(3);
        result?.KeyDataPairs?["netweight"].Should().BeNull();
        result?.KeyDataPairs?["number_package"].Should().Be(0);
        result?.KeyDataPairs?["type_package"].Should().Be("Case");

    }

    [Fact]
    public void GivenValidJson_ThenEverythingShouldBeDeserialized()
    {
        var json =
            "{\r\n\"keyDataPair\": [\r\n                        {\r\n                            \"key\": \"number_package\",\r\n                            \"data\": \"0\"\r\n                        },\r\n                        {\r\n                            \"key\": \"type_package\",\r\n                            \"data\": \"Case\"\r\n                        }\r\n                    ]\r\n}";

        var result = JsonSerializer.Deserialize<TestClass>(json);

        result.Should().NotBeNull();
        result?.KeyDataPairs?.Count.Should().Be(2);
        result?.KeyDataPairs?["number_package"].Should().Be(0);
        result?.KeyDataPairs?["type_package"].Should().Be("Case");

    }
}