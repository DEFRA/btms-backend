using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;

namespace Btms.Business.Extensions;

public static class MovementExtensions
{
    public static bool AreNumbersComplete<T>(this IEnumerable<T> source, Func<T, int?> getNumbers)
    {
        var numbers = source
            .Select(getNumbers)
            .Where(n => n.HasValue())
            .Order()
            .ToList();

        if (numbers.Distinct().Count() != numbers.Count())
        {
            //Contains duplicates
            return false;
        }
        else if (numbers.Count() != numbers.Last())
        {
            //Some missing
            // should be contiguous

            return false;
        }

        return true;
    }
    
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
    
    public static List<string> UniqueDocumentReferenceIdsThatShouldLink(this Movement movement)
    {
        return movement.Items
            .SelectMany(i => i.Documents ?? [])
            // Only CHED document refs should result in links
            .Where(d => d.DocumentReference != null && 
                        d.DocumentReference.StartsWith("GBCHD"))
            .Select(d => d.DocumentReference!.TrimUniqueNumber())
            .Distinct()
            .ToList();
    }
}