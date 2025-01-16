using Btms.Backend.IntegrationTests.Helpers;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.MatchingTests;

[Trait("Category", "Integration")]
public class UnmatchedChedTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<AllChedsNoMatchScenarioGenerator>(output)
{
    [Fact]
    public void ChedsWithNoCR_ShouldNotMatch()
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
        

        // // Act
        var chedAImportNotification = Client.AsJsonApiClient().GetById(chedA.ReferenceNumber!, Path.Join("api", "import-notifications")).GetResourceObject<Model.Ipaffs.ImportNotification>();
        var chedDImportNotification = Client.AsJsonApiClient().GetById(chedD.ReferenceNumber!, Path.Join("api", "import-notifications")).GetResourceObject<Model.Ipaffs.ImportNotification>();
        var chedPImportNotification = Client.AsJsonApiClient().GetById(chedP.ReferenceNumber!, Path.Join("api", "import-notifications")).GetResourceObject<Model.Ipaffs.ImportNotification>();
        var chedPPImportNotification = Client.AsJsonApiClient().GetById(chedPP.ReferenceNumber!, Path.Join("api", "import-notifications")).GetResourceObject<Model.Ipaffs.ImportNotification>();

        
            
        // Assert
        chedAImportNotification.Relationships.Movements.Links.Should().BeNull();
        chedDImportNotification.Relationships.Movements.Links.Should().BeNull();
        chedPImportNotification.Relationships.Movements.Links.Should().BeNull();
        chedPPImportNotification.Relationships.Movements.Links.Should().BeNull();
    }
}