using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Matching;
using Btms.Model;
using Btms.Model.Ipaffs;
using Btms.Types.Alvs.Mapping;
using Btms.Types.Ipaffs.Mapping;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TestDataGenerator.Scenarios;
using Xunit;

namespace Btms.Business.Tests.Services.Matching;

public class MatchingServiceTests
{
    [Fact]
    public async Task SimpleNoMatchTest()
    {
        // Arrange
        var movements = GenerateMovements();
        var sut = new MatchingService();
        var context = new MatchingContext(new List<ImportNotification>(), movements);

        // Act
        var matchResult = await sut.Process(context, CancellationToken.None);

        // Assert
        matchResult.NoMatches.Count.Should().Be(movements[0].Items.Count);
    }

    [Fact]
    public async Task SimpleMatchTest()
    {
        // Arrange
        var testData = GenerateSimpleMatch();
        var sut = new MatchingService();
        var context = new MatchingContext(testData.Notifications, testData.Movements);

        // Act
        var matchResult = await sut.Process(context, CancellationToken.None);

        // Assert
        matchResult.NoMatches.Count.Should().Be(0);
        matchResult.Matches.Count.Should().Be(1);
    }

    private static List<Movement> GenerateMovements()
    {
        CrNoMatchScenarioGenerator generator =
            new CrNoMatchScenarioGenerator(NullLogger<CrNoMatchScenarioGenerator>.Instance);
        var config = ScenarioFactory.CreateScenarioConfig(generator, 1, 1);

        var generatorResult = generator.Generate(1, 1, DateTime.UtcNow, config);

        return generatorResult.ClearanceRequests.Select(x =>
        {
            var internalClearanceRequest = AlvsClearanceRequestMapper.Map(x);
            return MovementPreProcessor.BuildMovement(internalClearanceRequest);
        }).ToList();
    }

    private static (List<ImportNotification> Notifications, List<Movement> Movements) GenerateSimpleMatch()
    {
        ChedASimpleMatchScenarioGenerator generator =
            new ChedASimpleMatchScenarioGenerator(NullLogger<ChedASimpleMatchScenarioGenerator>.Instance);
        var config = ScenarioFactory.CreateScenarioConfig(generator, 1, 1);

        var generatorResult = generator.Generate(1, 1, DateTime.UtcNow, config);

        var movements = generatorResult.ClearanceRequests.Select(x =>
        {
            var internalClearanceRequest = AlvsClearanceRequestMapper.Map(x);
            return MovementPreProcessor.BuildMovement(internalClearanceRequest);
        }).ToList();

        var notifications = generatorResult.ImportNotifications.Select(ImportNotificationMapper.Map).ToList();

        return new ValueTuple<List<ImportNotification>, List<Movement>>(notifications, movements);
    }

    

}