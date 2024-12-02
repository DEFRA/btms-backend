using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Cdms.Metrics;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cdms.Emf
{

    public static class EmfExportExtensions
    {
        public static IApplicationBuilder UseEmfExporter(this IApplicationBuilder builder)
        {
            var config = builder.ApplicationServices.GetRequiredService<IConfiguration>();

            bool enabled = config.GetValue<bool>("AWS_EMF_ENABLED", true);

            if (enabled)
            {
                var ns = config.GetValue<string>("AWS_EMF_NAMESPACE");
            EmfExporter.Init(builder.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(EmfExporter)), ns!);
            }
            return builder;
        }
    }
    public static class EmfExporter
    {
        private static readonly MeterListener meterListener = new();
        private static ILogger log = null!;
        private static string? awsNamespace;
        public static void Init(ILogger logger, string? awsNamespace)
        {
            log = logger;
            EmfExporter.awsNamespace = awsNamespace;
            meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name is MetricNames.MeterName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };

            meterListener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
            meterListener.SetMeasurementEventCallback<long>(OnMeasurementRecorded);
            meterListener.SetMeasurementEventCallback<double>(OnMeasurementRecorded);
            meterListener.Start();
        }

        static void OnMeasurementRecorded<T>(
            Instrument instrument,
            T measurement,
            ReadOnlySpan<KeyValuePair<string, object?>> tags,
            object? state)
        {
            try
            {
                using (var metricsLogger = new MetricsLogger())
                {
                    metricsLogger.SetNamespace(awsNamespace);
                    var dimensionSet = new DimensionSet();
                    foreach (var tag in tags)
                    {
                        dimensionSet.AddDimension(tag.Key, tag.Value?.ToString());
                    }

                    // If the request contains a w3c trace id, let's embed it in the logs
                    // Otherwise we'll include the TraceIdentifier which is the connectionId:requestCount
                    // identifier.
                    // https://www.w3.org/TR/trace-context/#traceparent-header
                    if (!string.IsNullOrEmpty(Activity.Current?.Id))
                    {
                        metricsLogger.PutProperty("TraceId", Activity.Current.TraceStateString);
                    }

                    if (!string.IsNullOrEmpty(Activity.Current?.TraceStateString))
                    {
                        metricsLogger.PutProperty("TraceState", Activity.Current.TraceStateString);
                    }
                    metricsLogger.SetDimensions(dimensionSet);
                    var name = instrument.Name.Dehumanize().Camelize();
                    metricsLogger.PutMetric(name, Convert.ToDouble(measurement), instrument.Unit == "ea" ? Unit.COUNT : Unit.MILLISECONDS);
                    metricsLogger.Flush();
                }

            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to push EMF metric");
            }

        }
    }
}
