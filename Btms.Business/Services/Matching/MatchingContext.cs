using System.Diagnostics;
using Btms.Model;
using Btms.Model.Alvs;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Matching;

public class MatchingContext(
    List<ImportNotification> notifications,
    List<Movement> movements)
{
    public List<ImportNotification> Notifications { get; } = notifications;
    public List<Movement> Movements { get; } = movements;

    public Dictionary<int, List<Document>> GetUnlinkedIdentifiers()
    {
        var notificationIdentifiers = Notifications.Select(x => x._MatchReference).ToList();

        var itemReferences = new Dictionary<int, List<Document>>();
        foreach (var movementItem in Movements.SelectMany(movement => movement.Items))
        {
            Debug.Assert(movementItem.ItemNumber != null, "movementItem.ItemNumber != null");
            Debug.Assert(movementItem.Documents != null, "movementItem.Documents != null");
            ////itemReferences.Add(movementItem.ItemNumber.Value,
            ////    movementItem.Documents.Select(x =>
            ////        x.DocumentReference != null ? MatchIdentifier.FromCds(x.DocumentReference).Identifier : null));
             itemReferences.Add(movementItem.ItemNumber.Value, movementItem.Documents.ToList());
        }

        return itemReferences
            .ToDictionary(itemReference => itemReference.Key,
                itemReference => itemReference.Value.Where(x => !notificationIdentifiers.Contains(MatchIdentifier.FromCds(x.DocumentReference!).Identifier)).ToList())!;

    }
}