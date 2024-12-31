using Btms.Backend.IntegrationTests.Helpers;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.MatchingTests;

[Trait("Category", "Integration")]
public class UnmatchedChedTests(
    InMemoryScenarioApplicationFactory<AllChedsNoMatchScenarioGenerator> factory,
    ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "MatchingTests"),
        IClassFixture<InMemoryScenarioApplicationFactory<AllChedsNoMatchScenarioGenerator>>
{
    [Fact]
    public void ChedsWithNoCR_ShouldNotMatch()
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
        

        // // Act
        var chedAImportNotification = Client.AsJsonApiClient().GetById(chedA.ReferenceNumber!, "api/import-notifications");
        var chedDImportNotification = Client.AsJsonApiClient().GetById(chedD.ReferenceNumber!, "api/import-notifications");
        var chedPImportNotification = Client.AsJsonApiClient().GetById(chedP.ReferenceNumber!, "api/import-notifications");
        var chedPPImportNotification = Client.AsJsonApiClient().GetById(chedPP.ReferenceNumber!, "api/import-notifications");

        // Assert
        (chedAImportNotification.Data.Relationships!["movements"]!.Meta?["matched"]!).ToString()!.Should().Be("False");
        (chedDImportNotification.Data.Relationships!["movements"]!.Meta?["matched"]!).ToString()!.Should().Be("False");
        (chedPImportNotification.Data.Relationships!["movements"]!.Meta?["matched"]!).ToString()!.Should().Be("False");
        (chedPPImportNotification.Data.Relationships!["movements"]!.Meta?["matched"]!).ToString()!.Should().Be("False");
    }
}