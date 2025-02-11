using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Btms.Metrics;

public class ValidationMetrics
{
    private readonly Counter<long> _total;
    private readonly Counter<long> _failed;

    public ValidationMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MetricNames.MeterName);

        _total = meter.CreateCounter<long>("btms.validation.total", description: "Number of times validation occurred");
        _failed = meter.CreateCounter<long>("btms.validation.failed", description: "Number of times validation failed");

    }
    public void Failed(Exception exception)
    {
        var tagList = BuildTags();
        tagList.Add(MetricNames.CommonTags.ExceptionType, exception.GetType().Name);
        _failed.Add(1, tagList);
    }

    public void Validated()
    {
        var tagList = BuildTags();
        _total.Add(1, tagList);
    }

    private static TagList BuildTags()
    {
        return new TagList
        {
            { MetricNames.CommonTags.Service, Process.GetCurrentProcess().ProcessName }
        };
    }
}