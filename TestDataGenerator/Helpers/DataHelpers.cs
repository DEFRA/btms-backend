using Btms.Common.Extensions;
using Btms.Model;
using Btms.Types.Ipaffs.V1.Extensions;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using Decision = Btms.Types.Alvs.Decision;
using Finalisation = Btms.Types.Alvs.Finalisation;

namespace TestDataGenerator.Helpers;

public static class DataHelpers
{
    internal static string BlobPath(this object resource, string rootPath)
    {
        switch (resource)
        {
            case null:
                throw new ArgumentNullException();
            case ImportNotification n:
                return n.BlobPath(rootPath);
            case AlvsClearanceRequest cr:
                return cr.BlobPath(rootPath);
            case Decision d:
                return d.BlobPath(rootPath);
            case Finalisation f:
                return f.BlobPath(rootPath);
            default:
                throw new InvalidDataException($"Unexpected type {resource.GetType().Name}");
        }
    }

    private static string BlobPath(this ImportNotification notification, string rootPath)
    {
        var dateString = notification.LastUpdated!.Value.ToString("yyyy/MM/dd");

        return $"{rootPath}/IPAFFS/{notification.ImportNotificationType!.Value.AsString()}/{dateString}/{notification.ReferenceNumber!.Replace(".", "_")}-{Guid.NewGuid()}.json";
    }

    private static string DateRef(this DateTime created)
    {
        return created.ToString("MMdd");
    }

    private static string BlobPath(this AlvsClearanceRequest clearanceRequest, string rootPath)
    {
        var dateString = clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.ToString("yyyy/MM/dd");
        var subPath = "ALVS";
        
        return
            $"{rootPath}/{subPath}/{dateString}/{clearanceRequest.Header!.EntryReference!.Replace(".", "")}-{Guid.NewGuid()}.json";
    }

    private static string BlobPath(this Decision decision, string rootPath)
    {
        var dateString = decision.ServiceHeader!.ServiceCallTimestamp!.Value.ToString("yyyy/MM/dd");
        var subPath = "DECISIONS";
        
        return
            $"{rootPath}/{subPath}/{dateString}/{decision.Header!.EntryReference!.Replace(".", "")}-{Guid.NewGuid()}.json";
    }

    private static string BlobPath(this Finalisation finalisation, string rootPath)
    {
        var dateString = finalisation.ServiceHeader!.ServiceCallTimestamp!.Value.ToString("yyyy/MM/dd");
        var subPath = "FINALISATION";
        
        return
            $"{rootPath}/{subPath}/{dateString}/{finalisation.Header!.EntryReference!.Replace(".", "")}-{Guid.NewGuid()}.json";
    }

    internal static string AsCdsEntryReference(this MatchIdentifier identifier)
    {
        return $"23GB9999{identifier.Identifier}";
    }

    internal static string AsCdsDeclarationUcr(this MatchIdentifier identifier)
    {
        return $"UCGB9999{identifier.Identifier}";
    }

    internal static string AsCdsMasterUcr(this MatchIdentifier identifier)
    {
        return $"MUB9999{identifier.Identifier}";
    }
    
    internal static string GenerateReferenceNumber(ImportNotificationTypeEnum chedType, int scenario,
        DateTime created, int item)
    {
        var prefix = chedType.AsString();

        if (item > 999999) throw new ArgumentException("Currently only deals with max 100,000 items");

        var formatHundredThousands = "000000";

        return $"{prefix}.GB.{created.Year}.{scenario.ToString("00")}{created.DateRef()}{(item + 1).ToString(formatHundredThousands)}";
    }
}