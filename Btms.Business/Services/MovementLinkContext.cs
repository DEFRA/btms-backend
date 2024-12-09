using Btms.Model;
using Btms.Model.ChangeLog;

namespace Btms.Business.Services;

public record MovementLinkContext(Movement PersistedMovement, ChangeSet? ChangeSet) : LinkContext
{
    public override string GetIdentifiers()
    {
        return string.Join(',', PersistedMovement._MatchReferences);
    }
}