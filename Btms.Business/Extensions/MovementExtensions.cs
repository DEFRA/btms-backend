using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Items = Btms.Types.Alvs.Items;

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
    
    private static string TrimUniqueNumber(this string id)
    {
        return id.Substring(id.LastIndexOf(".") + 1);
    }
    public static List<string> UniqueNotificationRelationshipIds(this Movement movement)
    {
        return movement.Relationships.Notifications.Data
            .Select(n => n.Id!.TrimUniqueNumber())
            .Distinct() // We may end up with multiple relationships for the same ID if multiple items link to it?
            .ToList();
    }
    
    public static bool IsChed(this Document doc)
    {
        return doc.DocumentReference != null &&
               doc.DocumentReference.ToUpper().StartsWith("GBCHD");
    }
    
    public static List<string> UniqueDocumentReferenceIdsThatShouldLink(this List<Btms.Model.Cds.Items> items)
    {
        return items
            .SelectMany(i => i.Documents ?? [])
            // Only CHED document refs should result in links
            .Where(d => d.IsChed())
            .Select(d => d.DocumentReference!.TrimUniqueNumber())
            .Distinct()
            .ToList();
    }
    
    public static List<string> UniqueDocumentReferenceIdsThatShouldLink(this Movement movement)
    {
        return movement.Items
            .UniqueDocumentReferenceIdsThatShouldLink();
    }

    public static MovementStatus GetMovementStatus(ImportNotificationTypeEnum[] chedTypes, List<string> documentReferenceIds, List<string> notificationRelationshipIds)
    {
        return new MovementStatus()
        {
            ChedTypes = chedTypes,
            LinkStatus = documentReferenceIds.Count == 0
                ? LinkStatusEnum.NoLinks
                : notificationRelationshipIds.Count() == documentReferenceIds.Count() &&
                  notificationRelationshipIds.All(documentReferenceIds.Contains)
                    ? LinkStatusEnum.AllLinked
                    : notificationRelationshipIds.Count == 0 && documentReferenceIds.Count != 0
                        ? LinkStatusEnum.MissingLinks
                        : notificationRelationshipIds.Count < documentReferenceIds.Count()
                            ? LinkStatusEnum.PartiallyLinked
                            : LinkStatusEnum.Investigate
        };
    }
}