using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cdms.Emf
{

    public static class EmfExportExtentions
    {
        public static IApplicationBuilder UseEmfExporter(this IApplicationBuilder builder)
        {
            EmfExporter.Init(builder.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(EmfExporter)));
            return builder;
        }
    }
    public static class EmfExporter
    {
        private static readonly MeterListener meterListener = new();
        private static ILogger log = null!;
        public static void Init(ILogger logger)
        {
            log = logger;
            
            meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name is "Cdms")
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
