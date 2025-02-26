using Btms.BlobService;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Btms.BlobService.Extensions;

using TestDataGenerator.Extensions;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.SpecificFiles;
using Xunit;

namespace TestDataGenerator.Tests;

public class SpecificFilesScenarioGeneratorTests
{
    [Fact]
    public void GeneratedMessagesShouldBeInCorrectOrder()
    {
        var (configuration, _) = BuilderExtensions.GetConfig("Scenarios/Samples");

        var sp = new ServiceCollection()
            .AddBlobStorage(configuration)
            .AddSingleton<CachingBlobService>()
            .ConfigureTestDataGenerationServices()
            .BuildServiceProvider();

        var scenario = new DuplicateMovementItemsCdms211(sp, NullLogger<DuplicateMovementItemsCdms211>.Instance);

        var config = new ScenarioConfig()
        {
            Name = "Test",
            ArrivalDateRange = 1,
            Count = 1,
            Generator = scenario,
            CreationDateRange = 1
        };

        var messages = scenario.Generate(1, 1, DateTime.Today, config);

        // TODO the scenario 
        messages.Count.Should().Be(5);
        messages.Select(m => m.CreatedDate())
            .Should().BeInAscendingOrder();
    }
}