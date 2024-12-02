using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Cdms.SensitiveData.Tests;

public class SensitiveDataSerializerTests
{
    [Fact]
    public void WhenDoNotIncludeSensitiveData_ThenDataShouldBeRedacted()
    {
        // ARRANGE
        SensitiveDataOptions options = new SensitiveDataOptions { Getter = s => "TestRedacted", Include = false };
        var serializer = new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance);

        var simpleClass = new SimpleClass()
        {
            SimpleStringOne = "Test String One",
            SimpleStringTwo = "Test String Two",
            SimpleStringArrayOne =
                new[] { "Test String Array One Item One", "Test String Array One Item Two" },
            SimpleStringArrayTwo = new[] { "Test String Array Two Item One", "Test String Array Two Item Two" }
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
    public void WhenIncludeSensitiveData_ThenDataShouldNotBeRedacted()
    {
        // ARRANGE
        SensitiveDataOptions options = new SensitiveDataOptions { Getter = s => "TestRedacted", Include = true };
        var serializer = new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance);

        var simpleClass = new SimpleClass()
        {
            SimpleStringOne = "Test String One",
            SimpleStringTwo = "Test String Two",
            SimpleStringArrayOne =
                new[] { "Test String Array One Item One", "Test String Array One Item Two" },
            SimpleStringArrayTwo = new[] { "Test String Array Two Item One", "Test String Array Two Item Two" }
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
}