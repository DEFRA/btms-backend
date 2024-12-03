using System.Diagnostics;

namespace Btms.Metrics;

public static class BtmsDiagnostics
{
    public static readonly string ActivitySourceName = MetricNames.MeterName;
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}