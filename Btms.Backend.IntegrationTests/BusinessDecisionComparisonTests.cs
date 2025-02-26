using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class BusinessDecisionComparisonTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{

    // // Destroyed
    // Mrn24Gbdej9V2Od0Bhar0DestroyedScenarioGenerator
    //         
    // // Manual Action
    // Mrn24Gbeds4W7Dfrlmar0ManualActionScenarioGenerator
    //         
    // // Cleared
    // Mrn24Gbd2Uowtwym5Lar8ScenarioGenerator
    //         
    // // No Finalisation
    // Mrn24Gbdshixsy6Rckar3ScenarioGenerator

    [Theory]
    [InlineData(typeof(ChedAh01ScenarioGenerator1), BusinessDecisionStatusEnum.AnythingElse)]
    [InlineData(typeof(Mrn24Gbdc4Tw6Duqyiar5ScenarioGenerator), BusinessDecisionStatusEnum.AlvsNotRefusedBtmsRefused)]
    [InlineData(typeof(Mrn24Gbdshixsy6Rckar3ScenarioGenerator), BusinessDecisionStatusEnum.MatchComplete)]
    [InlineData(typeof(ChedWithAlvsX00WrongDocumentReferenceFormatScenarioGenerator), BusinessDecisionStatusEnum.AlvsReleaseBtmsNotReleased)]
    [InlineData(typeof(Mrn24Gbdej9V2Od0Bhar0DestroyedScenarioGenerator), BusinessDecisionStatusEnum.CancelledOrDestroyed)]
    [InlineData(typeof(Mrn24Gbeds4W7Dfrlmar0ManualActionScenarioGenerator), BusinessDecisionStatusEnum.ManualReleases)]
    public void ShouldHaveCorrectDecisionCode(Type generatorType, BusinessDecisionStatusEnum decisionStatus)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Status : {1}", generatorType!.FullName, decisionStatus);
        EnsureEnvironmentInitialised(generatorType);
        CheckDecisionStatus(decisionStatus);
    }

    private void CheckDecisionStatus(BusinessDecisionStatusEnum decisionStatus)
    {
        var movement =
            Client
                .GetSingleMovement();

        TestOutputHelper.WriteLine("MRN {0}, expectedDecisionStatus {1}", movement.EntryReference, decisionStatus);

        movement
            .Status.BusinessDecisionStatus
            .Should().Be(decisionStatus);
    }
}