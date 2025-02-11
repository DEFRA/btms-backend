namespace Btms.Business.Services.Matching;

public record MatchingResult
{
    private readonly List<Match> _matches = [];
    private readonly List<DocumentNoMatch> _noMatches = [];
    public void AddMatch(string notificationId, string movementId, int itemNumber, string documentReference)
    {
        _matches.Add(new Match(notificationId, movementId, itemNumber, documentReference));
    }

    public void AddDocumentNoMatch(string movementId, int itemNumber, string documentReference)
    {
        _noMatches.Add(new DocumentNoMatch(movementId, itemNumber, documentReference));
    }

    public IReadOnlyList<Match> Matches => _matches.AsReadOnly();


    public IReadOnlyList<DocumentNoMatch> NoMatches => _noMatches.AsReadOnly();
}

public record Match(string NotificationId, string MovementId, int ItemNumber, string DocumentReference);

public record DocumentNoMatch(string MovementId, int ItemNumber, string DocumentReference);