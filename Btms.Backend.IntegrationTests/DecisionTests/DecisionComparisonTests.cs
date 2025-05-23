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

    [InlineData(typeof(Mrn24Gbdc4Tw6Duqyiar5ScenarioGenerator), DecisionStatusEnum.DocumentReferenceCaseIncorrect)]
    [InlineData(typeof(Mrn24Gbdshixsy6Rckar3ScenarioGenerator), DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs)]
    [InlineData(typeof(ChedWithAlvsX00WrongDocumentReferenceFormatScenarioGenerator), DecisionStatusEnum.DocumentReferenceFormatIncorrect, Skip = "Lim to investigate test data")]
    [InlineData(typeof(Mrn24Gbd2Uowtwym5Lar8ScenarioGenerator), DecisionStatusEnum.PartialImportNotificationsLinked)]

    [InlineData(typeof(Mrn24Gbd0Mbe1Q1Cntar7ScenarioGenerator), DecisionStatusEnum.PartialImportNotificationsLinked)]
    [InlineData(typeof(Mrn25Gb16796A6B91Ear9ScenarioGenerator), DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs)]

    //This has a combination of X00s that we've cleared, and X00s that we've generated E03 for :|
    [InlineData(typeof(Mrn24Gbc8Onyjqzt5Tar5ScenarioGenerator), DecisionStatusEnum.AlvsX00NotBtms)]

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