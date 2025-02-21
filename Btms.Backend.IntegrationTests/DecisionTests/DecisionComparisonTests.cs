using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class DecisionComparisonTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(ChedAh01ScenarioGenerator1), DecisionStatusEnum.NoAlvsDecisions)]

    [InlineData(typeof(MissingChedScenarioGenerator), DecisionStatusEnum.NoImportNotificationsLinked)]
    [InlineData(typeof(IuuScenarioGenerator), DecisionStatusEnum.DocumentReferenceFormatIncorrect, Skip = "Lim to investigate test data")]

    [InlineData(typeof(Mrn24Gbdzsrxdxtbvkar6ScenarioGenerator), DecisionStatusEnum.HasChedppChecks, Skip = "Lim to investigate test data")]

    // Failing due to CDMS-319 & CDMS-314
    [InlineData(typeof(Mrn24Gbdc4Tw6Duqyiar5ScenarioGenerator), DecisionStatusEnum.DocumentReferenceCaseIncorrect)]
    [InlineData(typeof(Mrn24Gbdshixsy6Rckar3ScenarioGenerator), DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs)]
    [InlineData(typeof(ChedWithAlvsX00WrongDocumentReferenceFormatScenarioGenerator), DecisionStatusEnum.DocumentReferenceFormatIncorrect, Skip = "Lim to investigate test data")]
    [InlineData(typeof(Mrn24Gbd2Uowtwym5Lar8ScenarioGenerator), DecisionStatusEnum.PartialImportNotificationsLinked)]

    [InlineData(typeof(Mrn24Gbd0Mbe1Q1Cntar7ScenarioGenerator), DecisionStatusEnum.PartialImportNotificationsLinked)]
    public void ShouldHaveCorrectDecisionCode(Type generatorType, DecisionStatusEnum decisionStatus)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Status : {1}", generatorType!.FullName, decisionStatus);
        EnsureEnvironmentInitialised(generatorType);
        CheckDecisionStatus(decisionStatus);
    }

    private void CheckDecisionStatus(DecisionStatusEnum decisionStatus)
    {
        var movement =
            Client
                .GetSingleMovement();

        TestOutputHelper.WriteLine("MRN {0}, expectedDecisionStatus {1}", movement.EntryReference, decisionStatus);

        movement
            .AlvsDecisionStatus.Context.DecisionComparison?.DecisionStatus
            .Should().Be(decisionStatus);

        movement.Decisions.Last().Items.Length.Should().Be(movement.Items.Count);
    }
}