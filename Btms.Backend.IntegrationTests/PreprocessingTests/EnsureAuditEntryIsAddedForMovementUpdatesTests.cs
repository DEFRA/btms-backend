using FluentAssertions;
using Btms.Model.Auditing;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
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
        var movement = Client
            .GetSingleMovement();

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
        var movement = Client
            .GetSingleMovement();

        movement.AuditEntries
            .Count(a => a is { CreatedBy: CreatedBySystem.Cds, Status: "Updated" })
            .Should().Be(1);
    }
}