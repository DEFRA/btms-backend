using Cdms.Common.Extensions;
using Cdms.Model.Extensions;
using System.Collections.Generic;
using MongoDB.Bson;
namespace Cdms.Analytics.Extensions;

public static class AnalyticsMetricNames
{
    public const string MeterName = "Cdms.Backend.Analytics";
    public const string MetricPrefix = "cdms.service.analytics";

    public static class CommonTags
    {
        public const string Service = "cdms.service.anayltics";
        public const string ExceptionType = "cdms.exception_type";
        public const string MessageType = "cdms.message_type";
    }
}

public static class AnalyticsHelpers
{
    internal static DateTime AggregateDateCreator(this BsonValue b) => b["dateToUse"].BsonType != BsonType.Null ? b["dateToUse"].ToUniversalTime() : DateTime.MinValue;

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

    internal static readonly Comparer<ByDateTimeResult>? byDateTimeResultComparer = Comparer<ByDateTimeResult>.Create((d1, d2) => d1.Period.CompareTo(d2.Period));
    
}