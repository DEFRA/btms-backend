using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class DecisionCategoryTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(ChedAh01ScenarioGenerator1), null, null)]
    [InlineData(typeof(CrNoMatchSingleItemWithDecisionScenarioGenerator), null, null)]
    [InlineData(typeof(CrNoMatchNoDecisionScenarioGenerator), null, null)]
    [InlineData(typeof(DeletedNotificationTestsScenarioGenerator), DecisionCategoryEnum.IpaffsDeletedChed, "24GBDPN9J48XRW5AR0")]
    [InlineData(typeof(ChedWithAlvsX00WrongDocumentReferenceFormatScenarioGenerator), DecisionCategoryEnum.DocumentReferenceFieldIncorrect, null)]
    [InlineData(typeof(CancelledNotificationSingleChedMrnTestsScenarioGenerator), DecisionCategoryEnum.IpaffsCancelledChed, null)]

    public void ShouldHaveCorrectDecisionCode(Type generatorType, DecisionCategoryEnum? decisionReason, string? mrnToAssertOn)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Reason : {1}", generatorType!.FullName, decisionReason);
        EnsureEnvironmentInitialised(generatorType);
        CheckDecisionStatus(decisionReason, mrnToAssertOn);
    }

    private void CheckDecisionStatus(DecisionCategoryEnum? decisionReason, string? mrnToAssertOn)
    {
        var movement = mrnToAssertOn.HasValue() ?
            Client
                .GetMovementByMrn(mrnToAssertOn)
                .GetResourceObject<Movement>() :
            Client
                .GetSingleMovement();

        TestOutputHelper.WriteLine("MRN {0}, NonComparableDecisionReasonEnum {1}", movement.EntryReference, decisionReason);

        movement
            .Status.NonComparableDecisionReason
            .Should().Be(decisionReason);
    }
}