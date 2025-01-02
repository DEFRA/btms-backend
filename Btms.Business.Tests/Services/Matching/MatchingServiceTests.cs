using Btms.Business.Builders;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Matching;
using Btms.Model;
using Btms.Model.Ipaffs;
using Btms.Types.Alvs;
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
        var movementBuilder = new MovementBuilder(NullLogger<MovementBuilder>.Instance);
        var config = ScenarioFactory.CreateScenarioConfig(generator, 1, 1);

        var generatorResult = generator
            .Generate(1, 1, DateTime.UtcNow, config)
            .First(x => x is AlvsClearanceRequest);

        var internalClearanceRequest = AlvsClearanceRequestMapper.Map((AlvsClearanceRequest)generatorResult);
        var movement = movementBuilder
            .From(internalClearanceRequest)
            .Build();

        return [movement];
    }

    private static (List<ImportNotification> Notifications, List<Movement> Movements) GenerateSimpleMatch()
    {
        ChedASimpleMatchScenarioGenerator generator =
            new ChedASimpleMatchScenarioGenerator(NullLogger<ChedASimpleMatchScenarioGenerator>.Instance);
        var config = ScenarioFactory.CreateScenarioConfig(generator, 1, 1);
        var movementBuilder = new MovementBuilder(NullLogger<MovementBuilder>.Instance);

        var generatorResult = generator.Generate(1, 1, DateTime.UtcNow, config);

        var messages = generatorResult.Aggregate(
            new { Notifications = new List<ImportNotification>(), Movements = new List<Movement>(), }, (memo, x) =>
            {
                switch (x)
                {
                    case null:
                        throw new ArgumentNullException();

                    case Btms.Types.Ipaffs.ImportNotification n:
                        var internalNotification = ImportNotificationMapper.Map(n);
                        memo.Notifications.Add(internalNotification);
                        break;
                    case AlvsClearanceRequest cr:

                        var internalClearanceRequest = AlvsClearanceRequestMapper.Map(cr);
                        memo.Movements.Add(movementBuilder.From(internalClearanceRequest).Build());
                        break;
                    default:
                        throw new ArgumentException($"Unexpected type {x.GetType().Name}");
                }

                return memo;
            });

        return new ValueTuple<List<ImportNotification>, List<Movement>>(messages.Notifications, messages.Movements);
    }

    

}