using Btms.Model;
using Btms.Model.Cds;
using MongoDB.Bson;
namespace Btms.Analytics.Extensions;

public class AnalyticsException(string message, Exception inner) : Exception(message, inner);

public static class AnalyticsMetricNames
{
    public const string MeterName = "Btms.Backend.Analytics";
    public const string MetricPrefix = "Btms.service.analytics";

    public static class CommonTags
    {
        public const string Service = "Btms.service.anayltics";
        public const string ExceptionType = "Btms.exception_type";
        public const string MessageType = "Btms.message_type";
    }
}

public static class AnalyticsHelpers
{
    internal static DateTime AggregateDateCreator(this BsonValue b) => b["dateToUse"].BsonType != BsonType.Null ? b["dateToUse"].ToUniversalTime() : DateTime.MinValue;

    internal static string GetLinkedName(string linked, string type)
    {
        return $"{type} {linked}";
    }
    
    internal static string GetLinkedName(bool linked, string type)
    {
        return $"{type} {GetLinkedName(linked)}";
    }
    internal static string GetLinkedName(bool linked)
    {
        return linked ? "Linked" : "Not Linked";
    }
    
    // By creating the dates we care about, we can ensure the arrays have all the dates, 
    // including any series that don't have data on a given day. We need these to be zero for the chart to render
    // correctly
    internal static DateTime[] CreateDateRange(DateTime from, DateTime to, AggregationPeriod aggregateBy) => Enumerable.Range(0, (to - from).Periods(aggregateBy)).Reverse()
        .Select(offset => from.Increment(offset, aggregateBy)) // from.AddDays(offset))
        .ToArray(); 

    internal static readonly Comparer<ByDateTimeResult>? ByDateTimeResultComparer = Comparer<ByDateTimeResult>.Create((d1, d2) => d1.Period.CompareTo(d2.Period));
    
    public static string[] GetImportNotificationSegments()
    {
        return ModelHelpers.GetChedTypes()
            .SelectMany(chedType => new[] { $"{chedType} Linked", $"{chedType} Not Linked" })
            .ToArray();
    }

    public static LinkStatusEnum[] GetMovementStatusSegments()
    {
        return Enum.GetValues<LinkStatusEnum>();
        // .Select(e => e.AsString()).ToArray();
        // return ["Linked", "Not Linked", "Investigate"];
    }
}