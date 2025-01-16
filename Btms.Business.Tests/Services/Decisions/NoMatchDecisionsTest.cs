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
        var publishBus = Substitute.For<IPublishBus>();

        var sut = new DecisionService(NullLogger<DecisionService>.Instance, publishBus, Array.Empty<IDecisionFinder>());

        var matchingResult = new MatchingResult();
        matchingResult.AddDocumentNoMatch(movements[0].Id!, movements[0].Items[0].ItemNumber!.Value, movements[0].Items[0].Documents?[0].DocumentReference!);

        // Act
        var decisionResult = await sut.Process(new DecisionContext(new List<ImportNotification>(), movements, matchingResult, true), CancellationToken.None);

        // Assert
        decisionResult.Should().NotBeNull();
        decisionResult.Decisions.Count.Should().Be(0);
        await publishBus.Received(0).Publish(Arg.Any<Decision>(), Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>(), Arg.Any<CancellationToken>());
        await Task.CompletedTask;
    }

    [Fact]
    public async Task WhenClearanceRequest_HasNotMatch_ThenDecisionCodeShouldBeNoMatch()
    {
        // Arrange
        var movements = GenerateMovements(true);
        movements[0].Items[0].Checks = [new Check() { CheckCode = "TEST" }];
        var publishBus = Substitute.For<IPublishBus>();

        var sut = new DecisionService(NullLogger<DecisionService>.Instance, publishBus, Array.Empty<IDecisionFinder>());

        var matchingResult = new MatchingResult();
        matchingResult.AddDocumentNoMatch(movements[0].Id!, movements[0].Items[0].ItemNumber!.Value, movements[0].Items[0].Documents?[0].DocumentReference!);
        
        // Act
        var decisionResult = await sut.Process(new DecisionContext(new List<ImportNotification>(), movements, matchingResult, true), CancellationToken.None);

        // Assert
        decisionResult.Should().NotBeNull();
        decisionResult.Decisions.Count.Should().Be(1);
        decisionResult.Decisions[0].DecisionCode.Should().Be(DecisionCode.X00);
        
        await publishBus.Received().Publish(Arg.Any<Types.Alvs.Decision>(), Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>(), Arg.Any<CancellationToken>());
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
        
        var movementBuilderFactory = new MovementBuilderFactory(NullLogger<MovementBuilder>.Instance);
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