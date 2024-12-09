using Btms.Model.Auditing;

namespace Btms.Model.Data;

public interface IAuditable
{
    AuditEntry GetLatestAuditEntry();
}