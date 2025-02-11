using Btms.Backend.Data.InMemory;
using Btms.Business.Builders;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Business.Services.Matching;
using Btms.Model;
using Btms.Model.Ipaffs;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SlimMessageBus;
using TestDataGenerator;
using TestDataGenerator.Scenarios;
using Xunit;
using Check = Btms.Model.Cds.Check;
using Decision = Btms.Model.Ipaffs.Decision;

namespace Btms.Business.Tests.Services.Decisions;

public class NoMatchDecisionsTest
{
    [Fact]
    public async Task WhenClearanceRequest_HasNotMatch_AndNoChecks_ThenNoDecisionShouldBeGenerated()
    {
        // Arrange
        var movements = GenerateMovements(false);

        var sut = new DecisionService(NullLogger<DecisionService>.Instance,
            Array.Empty<IDecisionFinder>(),
            new MovementBuilderFactory(new DecisionStatusFinder(), NullLogger<MovementBuilder>.Instance),
            new MemoryMongoDbContext());

        var matchingResult = new MatchingResult();
        matchingResult.AddDocumentNoMatch(movements[0].Id!, movements[0].Items[0].ItemNumber!.Value, movements[0].Items[0].Documents?[0].DocumentReference!);

        // Act
        var decisionResult = await sut.Process(new DecisionContext(new List<ImportNotification>(), movements, matchingResult, "TestMessageId",true), CancellationToken.None);

        // Assert
        decisionResult.Should().NotBeNull();
        decisionResult.Decisions.Count.Should().Be(0);

        await Task.CompletedTask;
    }

    [Fact]
    public async Task WhenClearanceRequest_HasNotMatch_ThenDecisionCodeShouldBeNoMatch()
    {
        // Arrange
        var movements = GenerateMovements(true);
        movements[0].Items[0].Checks = [new Check() { CheckCode = "TEST" }];

        var sut = new DecisionService(NullLogger<DecisionService>.Instance,
            Array.Empty<IDecisionFinder>(),
            new MovementBuilderFactory(new DecisionStatusFinder(), NullLogger<MovementBuilder>.Instance),
            new MemoryMongoDbContext());

        var matchingResult = new MatchingResult();
        matchingResult.AddDocumentNoMatch(movements[0].Id!, movements[0].Items[0].ItemNumber!.Value, movements[0].Items[0].Documents?[0].DocumentReference!);

        // Act
        var decisionResult = await sut.Process(new DecisionContext(new List<ImportNotification>(), movements, matchingResult, "TestMessageId", true), CancellationToken.None);

        // Assert
        decisionResult.Should().NotBeNull();
        decisionResult.Decisions.Count.Should().Be(1);
        decisionResult.Decisions[0].DecisionCode.Should().Be(DecisionCode.X00);

        await Task.CompletedTask;
    }

    // CrNoMatchNoChecksScenarioGenerator
    // CrNoMatchScenarioGenerator
    private static List<Movement> GenerateMovements(bool hasChecks)
    {
        ScenarioGenerator generator = hasChecks
            ? new CrNoMatchScenarioGenerator(NullLogger<CrNoMatchScenarioGenerator>.Instance)
            : new CrNoMatchNoChecksScenarioGenerator(NullLogger<CrNoMatchNoChecksScenarioGenerator>.Instance);

        var config = ScenarioFactory.CreateScenarioConfig(generator, 1, 1);

        var movementBuilderFactory = new MovementBuilderFactory(new DecisionStatusFinder(), NullLogger<MovementBuilder>.Instance);
        var generatorResult = generator
            .Generate(1, 1, DateTime.UtcNow, config)
            .First(x => x is AlvsClearanceRequest);

        var internalClearanceRequest = AlvsClearanceRequestMapper.Map((AlvsClearanceRequest)generatorResult);
        var movement = movementBuilderFactory
            .From(internalClearanceRequest)
            .Build();

        return [movement];
    }
}