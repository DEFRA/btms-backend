using System.Diagnostics;

namespace Cdms.Metrics;

public static class CdmsDiagnostics
{
    public static readonly string ActivitySourceName = MetricNames.MeterName;
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}