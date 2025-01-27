using System.Diagnostics;
using Btms.Business.Extensions;
using Btms.Model;

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
                    .Where(d => d.IsChed())
                    .GroupBy(d => d.DocumentReference)
                    .Select(d => d.Key);

                foreach (var documentGroup in groupedDocuments)
                {
                    Debug.Assert(documentGroup != null);
                    Debug.Assert(movement.Id != null, "movement.Id != null");
                    Debug.Assert(item.ItemNumber != null, "item.ItemNumber != null");

                    var notification = matchingContext.Notifications.Find(x =>
                        x._MatchReference == MatchIdentifier.FromCds(documentGroup).Identifier);

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

        return Task.FromResult(matchingResult);
    }
}