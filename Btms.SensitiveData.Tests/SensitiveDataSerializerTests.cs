using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Btms.SensitiveData.Tests;

public class SensitiveDataSerializerTests
{
    [Fact]
    public void WhenDoNotIncludeSensitiveData_ThenDataShouldBeRedacted()
    {
        // ARRANGE
        var options = new SensitiveDataOptions { Getter = _ => "TestRedacted", Include = false };
        var serializer = new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance);

        var simpleClass = new SimpleClass
        {
            SimpleStringOne = "Test String One",
            SimpleStringTwo = "Test String Two",
            SimpleStringArrayOne = ["Test String Array One Item One", "Test String Array One Item Two"],
            SimpleStringArrayTwo = ["Test String Array Two Item One", "Test String Array Two Item Two"]
        };

        var json = JsonSerializer.Serialize(simpleClass);

        // ACT
        var result = serializer.Deserialize<SimpleClass>(json);

        // ASSERT
        result.SimpleStringOne.Should().Be("TestRedacted");
        result.SimpleStringTwo.Should().Be("Test String Two");
        result.SimpleStringArrayOne[0].Should().Be("TestRedacted");
        result.SimpleStringArrayOne[1].Should().Be("TestRedacted");
        result.SimpleStringArrayTwo[0].Should().Be("Test String Array Two Item One");
        result.SimpleStringArrayTwo[1].Should().Be("Test String Array Two Item Two");
    }
    
    [Fact]
    public void WhenDoNotIncludeSensitiveData_ThenDataShouldBeRedactedOnNestedObjects()
    {
        // ARRANGE
        var options = new SensitiveDataOptions { Getter = _ => "TestRedacted", Include = false };
        var serializer = new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance);

        var simpleClass = new SimpleClass
        {
            SimpleObjectArray =
            [
                new SimpleClass
                {
                    SimpleStringOne = "Test String One",
                    SimpleStringTwo = "Test String Two",
                    SimpleStringArrayOne = ["Test String Array One Item One", "Test String Array One Item Two"],
                    SimpleStringArrayTwo = ["Test String Array Two Item One", "Test String Array Two Item Two"]
                }, new SimpleClass
                {
                    SimpleStringOne = "Test String One",
                    SimpleStringTwo = "Test String Two",
                    SimpleStringArrayOne = ["Test String Array One Item One", "Test String Array One Item Two"],
                    SimpleStringArrayTwo = ["Test String Array Two Item One", "Test String Array Two Item Two"]
                }
            ]
        };

        var json = JsonSerializer.Serialize(simpleClass, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // ACT
        var resultJson = serializer.RedactRawJson(json, typeof(SimpleClass));

        // ASSERT
        var result = JsonSerializer.Deserialize<SimpleClass>(resultJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        result.SimpleObjectArray[0].SimpleStringOne.Should().Be("TestRedacted");
        result.SimpleObjectArray[0].SimpleStringTwo.Should().Be("Test String Two");
        result.SimpleObjectArray[0].SimpleStringArrayOne[0].Should().Be("TestRedacted");
        result.SimpleObjectArray[0].SimpleStringArrayOne[1].Should().Be("TestRedacted");
        result.SimpleObjectArray[0].SimpleStringArrayTwo[0].Should().Be("Test String Array Two Item One");
        result.SimpleObjectArray[0].SimpleStringArrayTwo[1].Should().Be("Test String Array Two Item Two");
        
        result.SimpleObjectArray[1].SimpleStringOne.Should().Be("TestRedacted");
        result.SimpleObjectArray[1].SimpleStringTwo.Should().Be("Test String Two");
        result.SimpleObjectArray[1].SimpleStringArrayOne[0].Should().Be("TestRedacted");
        result.SimpleObjectArray[1].SimpleStringArrayOne[1].Should().Be("TestRedacted");
        result.SimpleObjectArray[1].SimpleStringArrayTwo[0].Should().Be("Test String Array Two Item One");
        result.SimpleObjectArray[1].SimpleStringArrayTwo[1].Should().Be("Test String Array Two Item Two");
    }

    [Fact]
    public void WhenIncludeSensitiveData_ThenDataShouldNotBeRedacted()
    {
        // ARRANGE
        var options = new SensitiveDataOptions { Getter = _ => "TestRedacted", Include = true };
        var serializer = new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance);

        var simpleClass = new SimpleClass
        {
            SimpleStringOne = "Test String One",
            SimpleStringTwo = "Test String Two",
            SimpleStringArrayOne =
                ["Test String Array One Item One", "Test String Array One Item Two"],
            SimpleStringArrayTwo = ["Test String Array Two Item One", "Test String Array Two Item Two"]
        };

        var json = JsonSerializer.Serialize(simpleClass);

        // ACT
        var result = serializer.Deserialize<SimpleClass>(json);

        // ASSERT
        result.SimpleStringOne.Should().Be("Test String One");
        result.SimpleStringTwo.Should().Be("Test String Two");
        result.SimpleStringArrayOne[0].Should().Be("Test String Array One Item One");
        result.SimpleStringArrayOne[1].Should().Be("Test String Array One Item Two");
        result.SimpleStringArrayTwo[0].Should().Be("Test String Array Two Item One");
        result.SimpleStringArrayTwo[1].Should().Be("Test String Array Two Item Two");
    }

    [Fact]
    public void WhenDoNotIncludeSensitiveData_AndRequestForRawJson_ThenDataShouldBeRedacted()
    {
        // ARRANGE
        var options = new SensitiveDataOptions { Getter = _ => "TestRedacted", Include = false };
        var serializer = new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance);

        var simpleClass = new SimpleClass
        {
            SimpleStringOne = "Test String One",
            SimpleStringTwo = "Test String Two",
            SimpleStringArrayOne =
                ["Test String Array One Item One", "Test String Array One Item Two"],
            SimpleStringArrayTwo = ["Test String Array Two Item One", "Test String Array Two Item Two"]
        };

        var json = JsonSerializer.Serialize(simpleClass, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // ACT
        var result = serializer.RedactRawJson(json, typeof(SimpleClass));

        // ASSERT
        var resultClass = JsonSerializer.Deserialize<SimpleClass>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        resultClass?.SimpleStringOne.Should().Be("TestRedacted");
        resultClass?.SimpleStringTwo.Should().Be("Test String Two");
        resultClass?.SimpleStringArrayOne[0].Should().Be("TestRedacted");
        resultClass?.SimpleStringArrayOne[1].Should().Be("TestRedacted");
        resultClass?.SimpleStringArrayTwo[0].Should().Be("Test String Array Two Item One");
        resultClass?.SimpleStringArrayTwo[1].Should().Be("Test String Array Two Item Two");
    }
}