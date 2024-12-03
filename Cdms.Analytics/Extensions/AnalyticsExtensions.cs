using Cdms.Backend.Data;
using Cdms.Model.Data;
using Cdms.Model.Ipaffs;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cdms.Analytics.Extensions;

public static class AnalyticsExtensions
{
    private static readonly bool enableMetrics = true;
    public static IServiceCollection AddAnalyticsServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IImportNotificationsAggregationService, ImportNotificationsAggregationService>();
        services.AddScoped<IMovementsAggregationService, MovementsAggregationService>();

        // To revisit in future 
        if (enableMetrics)
        {
            services.TryAddScoped<ImportNotificationMetrics>();    
        }
        
        return services;
    }

    public static string MetricsKey(this Dataset ds)
    {
        return ds.Name.Replace(" ", "-").ToLower();
    }

    public static int Periods(this TimeSpan t, AggregationPeriod aggregateBy) => Convert.ToInt32(aggregateBy == AggregationPeriod.Hour ? t.TotalHours : t.TotalDays);
    
    public static DateTime Increment(this DateTime d, int offset, AggregationPeriod aggregateBy) => aggregateBy == AggregationPeriod.Hour ? d.AddHours(offset) : d.AddDays(offset);

    public static Dictionary<string, BsonDocument> GetAggregatedRecordsDictionary<T>(
        this IMongoCollectionSet<T> collection,
        FilterDefinition<T> filter,
        ProjectionDefinition<T> projection,
        ProjectionDefinition<BsonDocument> group,
        ProjectionDefinition<BsonDocument> datasetGroup,
        Func<BsonDocument, string> createDatasetName) where T : IDataEntity
    {
        return collection
            .Aggregate()
            .Match(filter)
            .Project(projection)
            .Group(group)
            .Group(datasetGroup)
            .ToList()
            .ToDictionary(createDatasetName, b => b);
    }

    public static Dictionary<DateTime, int> GetNamedSetAsDict(this Dictionary<string, BsonDocument> records, string title)
    {
        return records
            .TryGetValue(title, out var b)
            ? b["dates"].AsBsonArray
                .ToDictionary(AnalyticsHelpers.AggregateDateCreator, d => d["count"].AsInt32)
            : [];
    }

    public static Dataset AsDataset(this Dictionary<string, BsonDocument> records, DateTime[] dateRange, string title)
    {
        var dates = records.GetNamedSetAsDict(title);
        return new Dataset(title)
        {
            Periods = dateRange
                .Select(resultDate =>
                    new ByDateTimeResult()
                    {
                        Period = resultDate, Value = dates.GetValueOrDefault(resultDate, 0)
                    })
                .Order(AnalyticsHelpers.byDateTimeResultComparer)
                .ToList()
        };
    }

    public static Dataset[] AsOrderedArray(this IEnumerable<Dataset> en)
    {
        return en
            .OrderBy(d => d.Name)
            .ToArray();
    }

}