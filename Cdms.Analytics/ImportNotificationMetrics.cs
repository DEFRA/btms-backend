

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Cdms.Common;
using Cdms.Metrics;
using Cdms.Analytics.Extensions;
using Cdms.Common.Extensions;
using Cdms.Model;
using Cdms.Model.Extensions;
using Cdms.Model.Ipaffs;
using Microsoft.Extensions.Logging;

namespace Cdms.Analytics;
/// <summary>
/// Started to look at providing metrics from mongo into the cloudwatch cdp metrics stack
/// a few question marks about how CDP would deal with this if we're running more than one service
/// so parking for now.
/// </summary>
public class ImportNotificationMetrics
{
    private readonly IImportNotificationsAggregationService _importService;
    private readonly Dictionary<string, Instrument> _metrics = new Dictionary<string, Instrument>();
    private readonly ILogger<ImportNotificationMetrics> _logger;
    
    private void Add(Instrument i)
    {
        _metrics.Add(i.Name, i);
    }
    
    public ImportNotificationMetrics(IMeterFactory meterFactory, IImportNotificationsAggregationService importService, ILogger<ImportNotificationMetrics> logger)
    {
        _importService = importService;
        _logger = logger;
        
        var meter = meterFactory.Create(AnalyticsMetricNames.MeterName);
        
        foreach (var chedType in ModelHelpers.GetChedTypes())
        {
            Add(meter.CreateGauge<int>($"{AnalyticsMetricNames.MetricPrefix}.import-notifications.{chedType.ToLower()}-linked.count", "ea",$"Number of {chedType} Linked Import Notifications")); 
            Add(meter.CreateGauge<int>($"{AnalyticsMetricNames.MetricPrefix}.import-notifications.{chedType.ToLower()}-not-linked.count", "ea",$"Number of {chedType} Not Linked Import Notifications"));  
        }
    }

    public async Task RecordCurrentState()
    {
        var metrics = await _importService.ByCreated(DateTime.Today, DateTime.Now.NextHour());
        
        foreach (var dataset in metrics)
        {
            var key = $"{AnalyticsMetricNames.MetricPrefix}.import-notifications.{dataset.MetricsKey()}.count";
            if (_metrics.TryGetValue(key, out var instrument))
            {
                if (instrument is Gauge<int> g)
                {
                    g.Record(dataset.Periods[0].Value);
                }
                else
                {
                    _logger.LogWarning("Unexpected type of instrument {type} for metric {name}", instrument.GetType(), key);
                }
            }
            else
            {
                _logger.LogWarning("No instrument present for metric {name}", key);
            }
        }
    }
}