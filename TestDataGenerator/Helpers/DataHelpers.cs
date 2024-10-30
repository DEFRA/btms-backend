using Cdms.Types.Alvs;
using Cdms.Types.Ipaffs;

namespace TestDataGenerator.Helpers;

public static class DataHelpers
{
    internal static string RootBlobPath()
    {
        // TODO {notification.LastUpdated} should be a date so we can use it here...
        return "GENERATED/";
    }
    internal static string BlobPath(this ImportNotification notification)
    {
        // TODO {notification.LastUpdated} should be a date so we can use it here...
        return $"{RootBlobPath()}IPAFFS/CHEDA/2024/10/01/{notification.ReferenceNumber!.Replace(".","_")}-{Guid.NewGuid()}.json";
    }

    internal static string BlobPath(this AlvsClearanceRequest clearanceRequest)
    {
        var dateString = clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.ToString("yyyy/dd/MM");
        
        return $"{RootBlobPath()}ALVS/{dateString}/{clearanceRequest.Header!.EntryReference!.Replace(".","")}-{Guid.NewGuid()}.json";
    }
}