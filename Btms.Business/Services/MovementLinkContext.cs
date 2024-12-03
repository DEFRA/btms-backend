using Btms.Model;

namespace Btms.Business.Services;

public record MovementLinkContext(Movement ReceivedMovement, Movement? ExistingMovement) : LinkContext
{
    public override string GetIdentifiers()
    {
        return string.Join(',', ReceivedMovement._MatchReferences);
    }
}