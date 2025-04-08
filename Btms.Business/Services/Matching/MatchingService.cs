using System.Diagnostics;
using Btms.Model;
using Btms.Model.Cds;

namespace Btms.Business.Services.Matching;

public class MatchingService : IMatchingService
{
    public Task<MatchingResult> Process(MatchingContext matchingContext, CancellationToken cancellationToken)
    {
        var matchingResult = new MatchingResult();
        foreach (var movement in matchingContext.Movements)
        {
            foreach (var item in movement.Items)
            {
                if (item.Documents == null) continue;

                var groupedDocuments = item
                    .Documents
                    .GroupBy(d => new { d.DocumentReference, d.DocumentCode})
                    .Select(d => d.Key);

                foreach (var documentGroup in groupedDocuments)
                {
                    ProcessDocument(matchingContext, documentGroup.DocumentReference, documentGroup.DocumentCode, movement, item, matchingResult);
                }
            }
        }

        return Task.FromResult(matchingResult);
    }

    private static void ProcessDocument(MatchingContext matchingContext, string? documentGroup, string? documentCode, Movement movement,
        Items item, MatchingResult matchingResult)
    {
        Debug.Assert(documentGroup != null);
        Debug.Assert(movement.Id != null, "movement.Id != null");
        Debug.Assert(item.ItemNumber != null, "item.ItemNumber != null");

        if (MatchIdentifier.TryFromCds(documentGroup!, documentCode!, out var identifier))
        {
            var notification = matchingContext.Notifications.Find(x =>
                x._MatchReference == identifier.Identifier);

            if (notification is null)
            {
                matchingResult.AddDocumentNoMatch(movement.Id, item.ItemNumber.Value, documentGroup);
            }
            else
            {
                Debug.Assert(notification?.Id != null, "notification.Id != null");
                matchingResult.AddMatch(notification.Id, movement.Id, item.ItemNumber.Value, documentGroup);
            }
        }
    }
}