using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Matching;
using Btms.Model;
using Btms.Model.Ipaffs;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SlimMessageBus;
using TestDataGenerator.Scenarios;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions;

public class NoMatchDecisionsTest
{
    [Fact]
    public async Task WhenClearanceRequest_HasNotMatch_ThenDecisionCodeShouldBeNoMatch()
    {
        // Arrange
        var movements = GenerateMovements();
        var publishBus = Substitute.For<IPublishBus>();

        var sut = new DecisionService(publishBus);

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

    private static List<Movement> GenerateMovements()
    {
        CrNoMatchScenarioGenerator generator =
        new CrNoMatchScenarioGenerator(NullLogger<CrNoMatchScenarioGenerator>.Instance);
        var config = ScenarioFactory.CreateScenarioConfig(generator, 1, 1);

        var generatorResult = generator.Generate(1, 1, DateTime.UtcNow, config);

        return generatorResult.Select(x =>
        {
            var internalClearanceRequest = AlvsClearanceRequestMapper.Map((AlvsClearanceRequest)x);
            return MovementPreProcessor.BuildMovement(internalClearanceRequest);
        }).ToList();
    }
}