using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Types.Alvs;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.PreprocessingTests;

[Trait("Category", "Integration")]
public class EnsureAuditEntryIsAddedForMovementUpdatesTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<NoAuditLogForMovementUpdate>(output)
{
    [Fact]
    public void ShouldHaveCorrectDocumentReferenceFromUpdatedClearanceRequest()
    {
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

        movement.Items
            .Where(x => x.Documents != null)
            .SelectMany(x => x.Documents!)
            .Count(x => x.DocumentReference == "GBCHD2024.001239999999")
            .Should().Be(1);
    }
    
    [Fact]
    public void ShouldHaveUpdatedAuditEntry()
    {
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();
        
        movement.AuditEntries
            .Count(a => a is { CreatedBy: "Cds", Status: "Updated" })
            .Should().Be(1);
    }
}