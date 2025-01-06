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
    
    [Fact(Skip = "The document ref isn't being updated.")]
    // [Fact]
    public void ShouldHaveCorrectDocumentReferenceFromUpdatedClearanceRequest()
    {
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();
        
        movement.Items.First().Documents!.First().DocumentReference.Should().Be("GBCHD2024.001239999999");
    }
}