using Btms.Analytics;
using Btms.Analytics.Extensions;
using Btms.Common.Extensions;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Driver.Linq;

namespace Btms.Backend.Config;


[ModelBinder(BinderType = typeof(DateRangeBinder))]
public class DateRange {
    [FromQuery(Name = "dateFrom")] 
    public DateTime? From { get; set; }
    [FromQuery(Name = "dateTo")] 
    public DateTime? To { get; set; }

    public static DateRange Default()
    {
        return new DateRange() { From = DateTime.Now, To = DateTime.Now };
    }
    public static bool TryParse(string query, out DateRange dateRange)
    {
        dateRange = new DateRange() { From = DateTime.Now, To = DateTime.Now };
        return true;
    }
}

public class DateRangeBinderProvider : IModelBinderProvider
{
    public DateRangeBinderProvider()
    {
        Console.WriteLine("Testing12");
    }
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(DateRange))
        {
            return new BinderTypeModelBinder(typeof(DateRangeBinder));
        }

        return new BinderTypeModelBinder(context.Metadata.ModelType);
        // ;
        // return null!; //new BinderTypeModelBinder();
    }
}

public class DateRangeBinder: IModelBinder
{
    public DateRangeBinder()
    {
        Console.WriteLine("Testing12");
    }
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var dateFrom = bindingContext.ValueProvider.GetValue("dateFrom").FirstValue;
        var dateTo = bindingContext.ValueProvider.GetValue("dateTo").FirstValue;
            
        var dateRange = new DateRange()
        {
            From = DateTime.Now, To = DateTime.Now
        };
        // var name = bindingContext.ValueProvider.GetValue("name").FirstValue;
        // var beginName = bindingContext.ValueProvider.GetValue("name:contains").FirstValue;
        // var exactName = bindingContext.ValueProvider.GetValue("name:exact").FirstValue;
        // if (name.ToLower().Contains("tom")) {
        //     model1.name = name;
        // }
        // if (beginName.ToLower().StartsWith("tom")) {
        //     model1.beginName = beginName;
        // }
        // if (exactName.Contains("Tom")) {
        //     model1.exactName = exactName;
        // }
        bindingContext.Result = ModelBindingResult.Success(dateRange);
        return Task.CompletedTask;
    }
}
    
public static class AnalyticsDashboards
{
    public static async Task<IDictionary<string, IDataset>> GetCharts(
        ILogger logger,
        IImportNotificationsAggregationService importService,
        IMovementsAggregationService movementsService,
        string[] chartsToRender,
        ImportNotificationTypeEnum[] chedTypes,
        string? country,
        DateRange dateRange,
        // DateTime? dateFrom,
        // DateTime? dateTo,
        bool finalisedOnly
        )
    {
        var charts = new Dictionary<string, Func<Task<IDataset>>>
        {
            {
                "importNotificationLinkingByCreated",
                () => importService.ByCreated(DateTime.Today.MonthAgo(), DateTime.Today).AsIDataset()
            },
            {
                "importNotificationLinkingByArrival",
                () => importService.ByArrival(DateTime.Today.MonthAgo(), DateTime.Today.MonthLater()).AsIDataset()
            },
            {
                "last7DaysImportNotificationsLinkingStatus",
                () => importService.ByStatus(DateTime.Today.WeekAgo(), DateTime.Now).AsIDataset()
            },
            {
                "last24HoursImportNotificationsLinkingStatus",
                () => importService.ByStatus(DateTime.Now.Yesterday(), DateTime.Now).AsIDataset()
            },
            {
                "last24HoursImportNotificationsLinkingByCreated",
                () => importService
                    .ByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour)
                    .AsIDataset()
            },
            {
                "lastMonthImportNotificationsByTypeAndStatus",
                () => importService.ByStatus(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "allImportNotificationsByTypeAndStatus",
                () => importService.ByStatus().AsIDataset()
            },
            {
                "last24HoursMovementsLinkingByCreated",
                () => movementsService.ByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour).AsIDataset()
            },
            {
                "movementsLinkingByCreated",
                () => movementsService.ByCreated(DateTime.Today.MonthAgo(), DateTime.Today).AsIDataset()
            },
            {
                "allMovementsByStatus",
                () => movementsService.ByStatus().AsIDataset()
            },
            {
                "lastMonthMovementsByStatus",
                () => movementsService.ByStatus(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "lastMonthMovementsByItemCount",
                () => movementsService.ByItemCount(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "uniqueDocumentReferenceCount",
                () => movementsService.ByUniqueDocumentReferenceCount(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now).AsIDataset()
            },
            {
                "uniqueDocumentReferenceByMovementCount",
                () => movementsService.UniqueDocumentReferenceByMovementCount(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now).AsIDataset()
            },
            {
                "lastMonthMovementsByUniqueDocumentReferenceCount",
                () => movementsService.ByUniqueDocumentReferenceCount(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "movementsByUniqueDocumentReferenceCount",
                () => movementsService.ByUniqueDocumentReferenceCount(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now).AsIDataset()
            },
            {
                "lastMonthUniqueDocumentReferenceByMovementCount",
                () => movementsService.UniqueDocumentReferenceByMovementCount(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "lastMonthImportNotificationsByCommodityCount",
                () => importService.ByCommodityCount(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "decisionsByDecisionCode",
                () => movementsService.ByDecision(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "movementsBySegment",
                () => movementsService.BySegment(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "importNotificationsByVersion",
                () => importService.ByMaxVersion(dateRange.From ?? DateTime.Today.AddMonths(-3), dateRange.To ?? DateTime.Today, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "movementsByMaxEntryVersion",
                () => movementsService.ByMaxVersion(dateRange.From ?? DateTime.Today.AddMonths(-3), dateRange.To ?? DateTime.Today, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "movementsByMaxDecisionNumber",
                () => movementsService.ByMaxDecisionNumber(dateRange.From ?? DateTime.Today.AddMonths(-3), dateRange.To ?? DateTime.Today, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "movementsExceptions",
                () => movementsService.ExceptionSummary(dateRange.From ?? DateTime.Today.AddMonths(-3), dateRange.To ?? DateTime.Today, finalisedOnly, chedTypes, country).AsIDataset()
            }
        };
        
        var chartsToReturn = chartsToRender.Length == 0
            ? charts
            : charts.Where(keyValuePair => chartsToRender.Contains(keyValuePair.Key));
            
        var taskList = chartsToReturn.Select(r => new KeyValuePair<string, Task<IDataset>>(key:r.Key, value: r.Value()));
        
        // TODO - have just noticed this executes each chart twice
        // once during Task.WhenAll and again on the following line - revisit
        await Task.WhenAll(taskList.Select(r => r.Value));

        var output = taskList
            .ToDictionary(t => t.Key, t => t.Value.Result);
        
        logger.LogInformation("Results found {0} Datasets, {1}", output.Count, output.Keys);

        return output;
        
    }
}