using Btms.Model;
using Btms.Model.Auditing;

namespace TestGenerator.IntegrationTesting.Backend.Extensions;

public static class MovementExtensions
{
    public static AuditEntry SingleBtmsDecisionAuditEntry(this Movement movement)
    {
        return movement
            .AuditEntries
            .Single(a => a is { CreatedBy: "Btms", Status: "Decision" });
    }
}