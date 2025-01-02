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
        var chedAImportNotification = Client.AsJsonApiClient().GetById(chedA.ReferenceNumber!, "api/import-notifications").GetResourceObject<Model.Ipaffs.ImportNotification>();
        var chedDImportNotification = Client.AsJsonApiClient().GetById(chedD.ReferenceNumber!, "api/import-notifications").GetResourceObject<Model.Ipaffs.ImportNotification>();
        var chedPImportNotification = Client.AsJsonApiClient().GetById(chedP.ReferenceNumber!, "api/import-notifications").GetResourceObject<Model.Ipaffs.ImportNotification>();
        var chedPPImportNotification = Client.AsJsonApiClient().GetById(chedPP.ReferenceNumber!, "api/import-notifications").GetResourceObject<Model.Ipaffs.ImportNotification>();

        
            
        // Assert
        chedAImportNotification.Relationships.Movements.Links.Should().BeNull();
        chedDImportNotification.Relationships.Movements.Links.Should().BeNull();
        chedPImportNotification.Relationships.Movements.Links.Should().BeNull();
        chedPPImportNotification.Relationships.Movements.Links.Should().BeNull();
    }
}