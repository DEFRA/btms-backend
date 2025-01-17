using Btms.Model.Auditing;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.PreprocessingTests;

public class EnsureLinkIsRemovedWhenMrnDocumentRefChanges(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<ClearLinksDueToMrnDocumentRefChange>(output)
{
    [Fact]
    public void ShouldClearLinkingWhenMrnDocumentRefChanges()
    {
       var movement = Client.GetSingleMovement();
        
        //Assert Audit entries to ensure we first Linked and then Update received
        movement.AuditEntries
            .Count(a => a is { CreatedBy: CreatedBySystem.Btms, Status: "Linked" })
            .Should().Be(1);
        movement.AuditEntries
            .Count(a => a is { CreatedBy: CreatedBySystem.Cds, Status: "Updated" })
            .Should().Be(1);
        
        //Assert that the link was cleared after the updated mrn is processed
        movement.Relationships.Notifications.Data.Count.Should().Be(0);
    }
}