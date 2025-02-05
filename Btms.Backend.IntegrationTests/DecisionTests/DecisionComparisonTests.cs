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
    [InlineData(typeof(ChedAh01ScenarioGenerator2), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedAh01ScenarioGenerator3), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedAh01ScenarioGenerator4), DecisionStatusEnum.NoAlvsDecisions)]
    
    [InlineData(typeof(ChedDh01ScenarioGenerator1), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedDh01ScenarioGenerator2), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedDh01ScenarioGenerator3), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedDh01ScenarioGenerator4), DecisionStatusEnum.NoAlvsDecisions)]
    //
    [InlineData(typeof(ChedPh01ScenarioGenerator1), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedPh01ScenarioGenerator2), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedPh01ScenarioGenerator3), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedPh01ScenarioGenerator4), DecisionStatusEnum.NoAlvsDecisions)]
    
    [InlineData(typeof(ChedAh02ScenarioGenerator1), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedAh02ScenarioGenerator2), DecisionStatusEnum.NoAlvsDecisions)]
    
    [InlineData(typeof(ChedDh02ScenarioGenerator1), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedDh02ScenarioGenerator2), DecisionStatusEnum.NoAlvsDecisions)]
    
    [InlineData(typeof(ChedPh02ScenarioGenerator1), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedPh02ScenarioGenerator2), DecisionStatusEnum.NoAlvsDecisions)]
    
    [InlineData(typeof(ChedAc03ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedPc03ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedDc03ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    
    [InlineData(typeof(ChedAc05ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedAc06ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedPc06ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    
    [InlineData(typeof(ChedAn02ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedAn04ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedDn02ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedDn04ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    
    [InlineData(typeof(ChedPn02ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedPn03ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedPn04ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    [InlineData(typeof(ChedPn07ScenarioGenerator), DecisionStatusEnum.NoAlvsDecisions)]
    
    [InlineData(typeof(MissingChedScenarioGenerator), DecisionStatusEnum.NoImportNotificationsLinked)]
    [InlineData(typeof(IuuScenarioGenerator), DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs)]
    
    [InlineData(typeof(Mrn24Gbdzsrxdxtbvkar6ScenarioGenerator), DecisionStatusEnum.HasChedppChecks)]
    [InlineData(typeof(Mrn24Gbdshixsy6Rckar3ScenarioGenerator), DecisionStatusEnum.AlvsX00NotBtms)]

    [InlineData(typeof(Mrn24GBDC4TW6DUQYIAR5ScenarioGenerator), DecisionStatusEnum.AlvsX00CaseSensitivity)]
    [InlineData(typeof(Mrn24GBDSHIXSY6RCKAR3ScenarioGenerator), DecisionStatusEnum.AlvsX00WrongDocumentReferenceFormat)]

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

        TestOutputHelper.WriteLine("MRN {0}, expectedDecisionCode {1}", movement.EntryReference, decisionStatus);

        movement
            .AlvsDecisionStatus.Context.DecisionComparison?.DecisionStatus
            .Should().Be(decisionStatus);
    }
}