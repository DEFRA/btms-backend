using System.Diagnostics.CodeAnalysis;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Items = Btms.Types.Alvs.Items;
using LinkStatus = Btms.Model.Cds.LinkStatus;

namespace Btms.Business.Extensions;

public static class MovementExtensions
{

    public static string[] UniqueDocumentReferences(this Movement movement)
    {
        return movement.Items
            .SelectMany(i => i.Documents ?? [])
            .Select(d => d.DocumentReference!)
            .ToArray();
    }

    public static List<string> UniqueNotificationRelationshipIds(this Movement movement)
    {
        return movement.Relationships.Notifications.Data
            .Select(n => MatchIdentifier.FromNotification(n.Id!).Identifier)
            .Distinct() // We may end up with multiple relationships for the same ID if multiple items link to it?
            .ToList();
    }

    public static List<string> UniqueDocumentReferenceIds(this List<Btms.Model.Cds.Items> items)
    {
        var result = new List<string>();
        foreach (var item in items)
        {
            result.AddRange(item.GetIdentifiers());
        }

        return result.Distinct().ToList();
    }

    public static List<string> UniqueDocumentReferenceIds(this Movement movement)
    {
        return movement.Items
            .UniqueDocumentReferenceIds();
    }

    [SuppressMessage("SonarLint", "S3358",
         Justification =
             "This is a linq expression tree, unsure how to make it independent expressions")]
    public static MovementStatus GetMovementStatus(ImportNotificationTypeEnum[] chedTypes, List<string> documentReferenceIds, List<string> notificationRelationshipIds)
    {
        return new MovementStatus()
        {
            ChedTypes = chedTypes,
            LinkStatus = documentReferenceIds.Count == 0
                ? LinkStatus.NoLinks
                : notificationRelationshipIds.Count == documentReferenceIds.Count &&
                  notificationRelationshipIds.All(documentReferenceIds.Contains)
                    ? LinkStatus.AllLinked
                    : notificationRelationshipIds.Count == 0
                        ? LinkStatus.MissingLinks
                        : notificationRelationshipIds.Count < documentReferenceIds.Count
                            ? LinkStatus.PartiallyLinked
                            : LinkStatus.Investigate
        };
    }

    public static Dictionary<(int, string), string?> GetCheckDictionary(this CdsDecision decision)
    {
        return decision
            .Items!
            .SelectMany(i => i.Checks!.Select(c => new { Item = i, Check = c }))
            .ToDictionary(ic => (ic.Item.ItemNumber, ic.Check.CheckCode!), ic => ic.Check.DecisionCode);
    }

    public static List<ItemCheck> GetItemChecks(this AlvsDecision alvsDecision, IReadOnlyDictionary<(int, string), string?> compareTo)
    {
        return alvsDecision.Decision
            .Items!.SelectMany(i => i.Checks!.Select(c => new { Item = i, Check = c }))
            .Select(ic =>
            {
                var decisionCode =
                    compareTo.GetValueOrDefault((ic.Item.ItemNumber, ic.Check.CheckCode!), null);
                return new ItemCheck()
                {
                    ItemNumber = ic.Item!.ItemNumber,
                    CheckCode = ic.Check!.CheckCode!,
                    AlvsDecisionCode = ic.Check!.DecisionCode!,
                    BtmsDecisionCode = decisionCode
                };
            })
            .ToList();
    }
}