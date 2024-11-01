using Cdms.Types.Alvs;
using Cdms.Types.Ipaffs;

namespace TestDataGenerator.Helpers;

public static class DataHelpers
{
    internal static string BlobPath(this ImportNotification notification, string rootPath)
    {
        var dateString = notification.LastUpdated!.Value.ToString("yyyy/MM/dd");

        return $"{rootPath}/IPAFFS/CHEDA/{dateString}/{notification.ReferenceNumber!.Replace(".","_")}-{Guid.NewGuid()}.json";
    }

    internal static string BlobPath(this AlvsClearanceRequest clearanceRequest, string rootPath)
    {
        var dateString = clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.ToString("yyyy/MM/dd");
        
        return $"{rootPath}/ALVS/{dateString}/{clearanceRequest.Header!.EntryReference!.Replace(".","")}-{Guid.NewGuid()}.json";
    }
}