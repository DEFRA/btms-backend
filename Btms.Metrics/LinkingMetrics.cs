using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Btms.Metrics;

public class LinkingMetrics
{
    readonly Histogram<double> duration;
    readonly Counter<long> total;
    readonly Counter<long> faulted;

    public LinkingMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MetricNames.MeterName);
        duration = meter.CreateHistogram<double>("btms.linking.duration", "ms", "Elapsed time spent linking, in millis");

        total = meter.CreateCounter<long>("btms.linking.total", description: "Number of links generated");
        faulted = meter.CreateCounter<long>("btms.linking.faulted", description: "Number of times linking errored");
        
    }
    public void Faulted(Exception exception)
    {
        var tagList = BuildTags();
        tagList.Add(MetricNames.CommonTags.ExceptionType, exception.GetType().Name);
        faulted.Add(1, tagList);

    }

    public void Linked<T>(long delta)
    {
        var tagList = BuildTags();
        tagList.Add("btms.linking.resource_type", typeof(T).Name);
        total.Add(delta, tagList);
    }

    public void Completed(double milliseconds)
    {
        var tagList = BuildTags();
        duration.Record(milliseconds, tagList);
    }

    private static TagList BuildTags()
    {
        return new TagList
        {
            { MetricNames.CommonTags.Service, Process.GetCurrentProcess().ProcessName }
        };
    }
}