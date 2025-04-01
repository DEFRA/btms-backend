using Serilog;
using Serilog.Configuration;

namespace Btms.Consumers.Serilog;

public static class BtmsClientInfoLoggerConfigurationExtensions
{
    public static LoggerConfiguration WithConsumerCorrelationId(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        string headerName = "x-correlation-id",
        bool addValueIfHeaderAbsence = false)
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        return enrichmentConfiguration.With(new ConsumerCorrelationIdEnricher(headerName, addValueIfHeaderAbsence));
    }
}