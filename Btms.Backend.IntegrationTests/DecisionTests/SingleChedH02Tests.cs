using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class SingleChedH02Tests(
    InMemoryScenarioApplicationFactory<AllChedsH02DecisionGenerator> factory,
    ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "DecisionTests"),
        IClassFixture<InMemoryScenarioApplicationFactory<AllChedsH02DecisionGenerator>>
{
    [Fact]
    public void SingleChed_ShouldHaveH02CheckValues()
    {
        // Arrange
        var loadedData = factory.LoadedData;

        var chedA = (ImportNotification)loadedData.Single(d =>
            d.message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Cveda }).message;
        var chedD = (ImportNotification)loadedData.Single(d =>
            d.message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Ced }).message;
        var chedP = (ImportNotification)loadedData.Single(d =>
            d.message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Cvedp }).message;
        var chedPP = (ImportNotification)loadedData.Single(d =>
            d.message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Chedpp }).message;

        var chedAClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedA.ReferenceNumber!.Split(".")
                        .Last()))
            .message;
        var chedDClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedD.ReferenceNumber!.Split(".")
                        .Last()))
            .message;
        var chedPClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedP.ReferenceNumber!.Split(".")
                        .Last()))
            .message;
        var chedPPClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedPP.ReferenceNumber!.Split(".")
                        .Last()))
            .message;

        // Act
        var chedAMovement = Client.AsJsonApiClient()
            .GetById(chedAClearanceRequest!.Header!.EntryReference!, "api/movements").GetResourceObject<Movement>();
        var chedDMovement = Client.AsJsonApiClient()
            .GetById(chedDClearanceRequest!.Header!.EntryReference!, "api/movements").GetResourceObject<Movement>();
        var chedPMovement = Client.AsJsonApiClient()
            .GetById(chedPClearanceRequest!.Header!.EntryReference!, "api/movements").GetResourceObject<Movement>();
        var chedPPMovement = Client.AsJsonApiClient()
            .GetById(chedPPClearanceRequest!.Header!.EntryReference!, "api/movements").GetResourceObject<Movement>();

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
public class SingleChedDecisionTests(
    InMemoryScenarioApplicationFactory<AllChedsNonHoldDecisionGenerator> factory,
    ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "DecisionTests"),
        IClassFixture<InMemoryScenarioApplicationFactory<AllChedsNonHoldDecisionGenerator>>
{
    [Fact]
    public void SingleChed_ShouldHaveH02CheckValues()
    {
        // Arrange
        var loadedData = factory.LoadedData;

        var chedA = (ImportNotification)loadedData.Single(d =>
            d.message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Cveda }).message;
        var chedD = (ImportNotification)loadedData.Single(d =>
            d.message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Ced }).message;
        var chedP = (ImportNotification)loadedData.Single(d =>
            d.message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Cvedp }).message;
        var chedPP = (ImportNotification)loadedData.Single(d =>
            d.message is ImportNotification { ImportNotificationType: ImportNotificationTypeEnum.Chedpp }).message;

        var chedAClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedA.ReferenceNumber!.Split(".")
                        .Last()))
            .message;
        var chedDClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedD.ReferenceNumber!.Split(".")
                        .Last()))
            .message;
        var chedPClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedP.ReferenceNumber!.Split(".")
                        .Last()))
            .message;
        var chedPPClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d.message is AlvsClearanceRequest clearanceRequest && clearanceRequest.Header!.EntryReference!.Contains(
                    chedPP.ReferenceNumber!.Split(".")
                        .Last()))
            .message;

        // Act
        var chedAMovement = Client.AsJsonApiClient()
            .GetById(chedAClearanceRequest!.Header!.EntryReference!, "api/movements").GetResourceObject<Movement>();
        var chedDMovement = Client.AsJsonApiClient()
            .GetById(chedDClearanceRequest!.Header!.EntryReference!, "api/movements").GetResourceObject<Movement>();
        var chedPMovement = Client.AsJsonApiClient()
            .GetById(chedPClearanceRequest!.Header!.EntryReference!, "api/movements").GetResourceObject<Movement>();
        var chedPPMovement = Client.AsJsonApiClient()
            .GetById(chedPPClearanceRequest!.Header!.EntryReference!, "api/movements").GetResourceObject<Movement>();

        // Assert
        string decisionCode = "";
        chedAMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                return decisionCode.Equals("E03");
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
        chedPPMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decisionCode = i.Checks!.First().DecisionCode!;
                return decisionCode.Equals("C03");
            }).Should().BeTrue("Expected C03. Actually {0}", decisionCode);
    }
}

