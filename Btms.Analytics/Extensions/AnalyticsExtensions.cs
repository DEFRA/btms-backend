using System.Collections.Immutable;
using Btms.Backend.Data;
using Btms.Model.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Btms.Analytics.Extensions;

public static class AnalyticsExtensions
{
    private static readonly bool EnableMetrics = true;
    public static IServiceCollection AddAnalyticsServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IImportNotificationsAggregationService, ImportNotificationsAggregationService>();
        services.AddScoped<IMovementsAggregationService, MovementsAggregationService>();

        // To revisit in future 
        if (EnableMetrics)
        {
            services.TryAddScoped<ImportNotificationMetrics>();    
        }
        
        return services;
    }

    public static string MetricsKey(this DatetimeSeries ds)
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

    public static DatetimeSeries AsDataset(this Dictionary<string, BsonDocument> records, DateTime[] dateRange,
        string title)
    {
        var dates = records.GetNamedSetAsDict(title);
        return new DatetimeSeries(title)
        {
            Periods = dateRange
                .Select(resultDate =>
                    new ByDateTimeResult { Period = resultDate, Value = dates.GetValueOrDefault(resultDate, 0) })
                .Order(AnalyticsHelpers.ByDateTimeResultComparer)
                .ToList()
        };
    }

    public static T[] AsOrderedArray<T, TKey>(this IEnumerable<T> en, Func<T, TKey> keySelector)
    {
        return en
            .OrderBy(keySelector)
            .ToArray();
    }

    /// <summary>
    /// Gives us an opportunity to hook into the mongo execution and
    /// grab the query as a string after it's been executed
    /// </summary>
    /// <param name="source"></param>
    /// <param name="logger"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <returns></returns>
    /// <exception cref="AnalyticsException"></exception>
    internal static IEnumerable<IGrouping<TKey, TSource>> Execute<TSource, TKey>(this IQueryable<IGrouping<TKey, TSource>> source, ILogger logger)
    {
        try
        {
            var aggregatedData = source.ToList();
            return aggregatedData;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error querying Mongo : {Message}", ex.Message);
            throw new AnalyticsException("Error querying Mongo", ex);
        }
        finally 
        {
            logger.LogExecutedMongoString(source);
        }
    }
    
    /// <summary>
    /// Gives us an opportunity to hook into the mongo execution and
    /// grab the query as a string after it's been executed
    /// </summary>
    /// <param name="source"></param>
    /// <param name="logger"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <returns></returns>
    /// <exception cref="AnalyticsException"></exception>
    internal static IEnumerable<TSource> Execute<TSource>(this IAggregateFluent<TSource> source, ILogger logger)
    {
        try
        {
            var aggregatedData = source.ToList();
            return aggregatedData;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error querying Mongo : {Message}", ex.Message);
            throw new AnalyticsException("Error querying Mongo", ex);
        }
        finally
        {
            logger.LogInformation("Query from IAggregateFluent");
            // logger.LogExecutedMongoString((IQueryable)source);
        }
    }
    
    /// <summary>
    /// Gives us an opportunity to hook into the mongo execution and
    /// grab the query as a string after it's been executed 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="logger"></param>
    /// <param name="keySelector"></param>
    /// <param name="elementSelector"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    /// <exception cref="AnalyticsException"></exception>
    public static Dictionary<TKey, TElement> ExecuteAsDictionary<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        ILogger logger,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector)
        where TKey : notnull
    {
        try
        {
            var aggregatedData = source
                .ToDictionary(keySelector, elementSelector);
            return aggregatedData;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error querying Mongo : {Message}", ex.Message);
            throw new AnalyticsException("Error querying Mongo", ex);
        }
        finally 
        {
            logger.LogExecutedMongoString((IQueryable)source);
        }
    }
    /// <summary>
    /// Gives us an opportunity to hook into the mongo execution and
    /// grab the query as a string after it's been executed 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="logger"></param>
    /// <param name="keySelector"></param>
    /// <param name="elementSelector"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    /// <exception cref="AnalyticsException"></exception>
    public static ImmutableSortedDictionary<TKey, TElement> ExecuteAsSortedDictionary<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        ILogger logger,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector)
        where TKey : notnull
    {
        try
        {
            var aggregatedData = source
                .ToImmutableSortedDictionary(keySelector, elementSelector);
            return aggregatedData;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error querying Mongo : {Message}", ex.Message);
            throw new AnalyticsException("Error querying Mongo", ex);
        }
        finally 
        {
            logger.LogExecutedMongoString((IQueryable)source);
        }
    }
    
    internal static IEnumerable<TSource> Execute<TSource>(
        this IQueryable<TSource> source, ILogger logger) 
    {
        
        try
        {
            var aggregatedData = source.ToList();
            return aggregatedData;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error querying Mongo : {Message}", ex.Message);
            throw new AnalyticsException("Error querying Mongo", ex);
        }
        finally 
        {
            logger.LogExecutedMongoString(source);
        }
    }

    /// <summary>
    /// Gets the executed query details to allow issues to be reproduced
    /// I'm sure this could be made cleaner but should do for now.
    /// </summary>
    /// <param name="logger">Where to write the log</param>
    /// <param name="source">A mongo query that has already executed</param>
    private static void LogExecutedMongoString(this ILogger logger, IQueryable source)
    {
        var stages = ((IMongoQueryProvider)source.Provider).LoggedStages;
        var query = string.Join(",", stages.Select(s => s.ToString()).ToArray());
        logger.LogInformation("[{Query}]", query);
    }
    
    public static async Task<IDataset> AsIDataset(this Task<MultiSeriesDatetimeDataset> ms)
    {
        await ms;
        return (IDataset)ms.Result;
    }

    public static async Task<IDataset> AsIDataset(this Task<MultiSeriesDataset> ms)
    {
        await ms;
        return (IDataset)ms.Result;
    }
    
    public static async Task<IDataset> AsIDataset(this Task<TabularDataset<ByNameDimensionResult>> ms)
    {
        await ms;
        return (IDataset)ms.Result;
    }

    public static async Task<IDataset> AsIDataset(this Task<SingleSeriesDataset> ms)
    {
        await ms;
        return (IDataset)ms.Result;
    }
    
    public static async Task<IDataset> AsIDataset<TSummary,TResult>(this Task<SummarisedDataset<TSummary, TResult>> ms)
        where TResult : IDimensionResult
        where TSummary : IDimensionResult
    {
        await ms;
        return (IDataset)ms.Result;
    }
}