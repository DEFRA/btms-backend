using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
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
        
        CrNoMatchScenarioGenerator generator =
            new CrNoMatchScenarioGenerator(NullLogger<CrNoMatchScenarioGenerator>.Instance);
        var config = ScenarioFactory.CreateScenarioConfig(generator, 1, 1);

        var generatorResult = generator.Generate(1, 1, DateTime.UtcNow, config);

        var movements = generatorResult.ClearanceRequests.Select(x =>
        {
            var internalClearanceRequest = AlvsClearanceRequestMapper.Map(x);
            return MovementPreProcessor.BuildMovement(internalClearanceRequest);
        }).ToList();

        var publishBus = Substitute.For<IPublishBus>();
        var decisionMessageBuilder = Substitute.For<IDecisionMessageBuilder>();

        decisionMessageBuilder.Build(Arg.Any<DecisionResult>())
            .Returns(Task.FromResult(new List<AlvsClearanceRequest>()));

        var sut = new DecisionService(decisionMessageBuilder, publishBus);
        
        // Act
        var decisionResult = await sut.Process(new DecisionContext(new List<ImportNotification>(), movements, true), CancellationToken.None);

        // Assert
        decisionResult.Should().NotBeNull();
        decisionResult.MovementDecisions.Count.Should().Be(1);
        decisionResult.MovementDecisions[0].ItemDecisions.Count.Should().Be(generatorResult.ClearanceRequests[0].Items!.Length!);
        foreach (var itemDecision in decisionResult.MovementDecisions[0].ItemDecisions)
        {
            itemDecision.GetDecisionCode().Should().Be(DecisionCode.X00);
        }
        
        await decisionMessageBuilder.Received(generatorResult.ClearanceRequests.Length).Build(Arg.Any<DecisionResult>());
        generatorResult.Should().NotBeNull();
        await Task.CompletedTask;
    }
}