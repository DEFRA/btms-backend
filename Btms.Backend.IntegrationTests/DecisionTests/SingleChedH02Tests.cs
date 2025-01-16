using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class SingleChedH02Tests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<AllChedsH02DecisionGenerator>(output)
{
    [Fact]
    public void SingleChed_ShouldHaveH02CheckValues()
    {
        // Arrange
        var loadedData = LoadedData;

        var chedA = (ImportNotification)loadedData.Single(d =>
            d.Message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Cveda }).Message;
        var chedD = (ImportNotification)loadedData.Single(d =>
            d.Message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Ced }).Message;
        var chedP = (ImportNotification)loadedData.Single(d =>
            d.Message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Cvedp }).Message;
        var chedPP = (ImportNotification)loadedData.Single(d =>
            d.Message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Chedpp }).Message;

        var chedAClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.Message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedA.ReferenceNumber!.Split(".")
                        .Last()))
            .Message;
        var chedDClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.Message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedD.ReferenceNumber!.Split(".")
                        .Last()))
            .Message;
        var chedPClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.Message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedP.ReferenceNumber!.Split(".")
                        .Last()))
            .Message;
        var chedPPClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.Message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedPP.ReferenceNumber!.Split(".")
                        .Last()))
            .Message;

        // Act
        var chedAMovement = Client.AsJsonApiClient()
            .GetById(chedAClearanceRequest!.Header!.EntryReference!, Path.Join("api", "movements")).GetResourceObject<Movement>();
        var chedDMovement = Client.AsJsonApiClient()
            .GetById(chedDClearanceRequest!.Header!.EntryReference!, Path.Join("api", "movements")).GetResourceObject<Movement>();
        var chedPMovement = Client.AsJsonApiClient()
            .GetById(chedPClearanceRequest!.Header!.EntryReference!, Path.Join("api", "movements")).GetResourceObject<Movement>();
        var chedPPMovement = Client.AsJsonApiClient()
            .GetById(chedPPClearanceRequest!.Header!.EntryReference!, Path.Join("api", "movements")).GetResourceObject<Movement>();

        // Assert
        string decisionCode = "";
        chedAMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                return decisionCode.Equals("H02");
            }).Should().BeTrue("Expected H02. Actually {0}", decisionCode);
        chedDMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                return decisionCode.Equals("H02");
            }).Should().BeTrue("Expected H02. Actually {0}", decisionCode);
        chedPMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                return decisionCode.Equals("H02");
            }).Should().BeTrue("Expected H02. Actually {0}", decisionCode);
        chedPPMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                return decisionCode.Equals("H02");
            }).Should().BeTrue("Expected H02. Actually {0}", decisionCode);
    }
}

[Trait("Category", "Integration")]
public class SingleChedDecisionTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<AllChedsNonHoldDecisionGenerator>(output)
{
    [Fact]
    public void SingleChed_ShouldHaveH02CheckValues()
    {
        // Arrange
        var loadedData = LoadedData;

        var chedA = (ImportNotification)loadedData.Single(d =>
            d.Message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Cveda }).Message;
        var chedD = (ImportNotification)loadedData.Single(d =>
            d.Message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Ced }).Message;
        var chedP = (ImportNotification)loadedData.Single(d =>
            d.Message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Cvedp }).Message;
        var chedPP = (ImportNotification)loadedData.Single(d =>
            d.Message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Chedpp }).Message;

        var chedAClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.Message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedA.ReferenceNumber!.Split(".")
                        .Last()))
            .Message;
        var chedDClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.Message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedD.ReferenceNumber!.Split(".")
                        .Last()))
            .Message;
        var chedPClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.Message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedP.ReferenceNumber!.Split(".")
                        .Last()))
            .Message;

        // Act
        var chedAMovement = Client.AsJsonApiClient()
            .GetById(chedAClearanceRequest!.Header!.EntryReference!, Path.Join("api", "movements")).GetResourceObject<Movement>();
        var chedDMovement = Client.AsJsonApiClient()
            .GetById(chedDClearanceRequest!.Header!.EntryReference!, Path.Join("api", "movements")).GetResourceObject<Movement>();
        var chedPMovement = Client.AsJsonApiClient()
            .GetById(chedPClearanceRequest!.Header!.EntryReference!, Path.Join("api", "movements")).GetResourceObject<Movement>();

        // Assert
        string decisionCode = "";
        chedAMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                return decisionCode.Equals("C03");
            }).Should().BeTrue("Expected C03. Actually {0}", decisionCode);
        chedDMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                return decisionCode.Equals("C03");
            }).Should().BeTrue("Expected C03. Actually {0}", decisionCode);
        chedPMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                return decisionCode.Equals("C03");
            }).Should().BeTrue("Expected C03. Actually {0}", decisionCode);
    }
}

[Trait("Category", "Integration")]
public class MultiChedDecisionTest(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<MultiChedPMatchScenarioGenerator>(output)
{
    [Fact]
    public void MultiChed_ShouldHaveH01CheckValues()
    {
        string decisionCode = "";
        var expectedDecision = "H02";
        var movements = Client.AsJsonApiClient().Get(Path.Join("api", "movements")).GetResourceObjects<Movement>().Single().Decisions
            .OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                
                return decisionCode.Equals(expectedDecision);
            }).Should().BeTrue($"Expected {expectedDecision}. Actually {{0}}", decisionCode);
        ;
    }
}