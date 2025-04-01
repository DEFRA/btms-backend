using Serilog.Core;
using Serilog.Events;
using Btms.Consumers.ConsumerContextAccessor.ContextAccessor;

namespace Btms.Consumers.Serilog
{
    public class ConsumerCorrelationIdEnricher : ILogEventEnricher
    {
        private const string CorrelationIdItemKey = "Serilog_CorrelationId";
        private const string PropertyName = "CorrelationId";
        private readonly string _headerKey;
        private readonly bool _addValueIfHeaderAbsence;
        private readonly IConsumerContextAccessor _contextAccessor;

        public ConsumerCorrelationIdEnricher(string headerKey, bool addValueIfHeaderAbsence)
            : this(headerKey, addValueIfHeaderAbsence, new ContextAccessor.ConsumerContextAccessor())
        {
        }

        internal ConsumerCorrelationIdEnricher(string headerKey, bool addValueIfHeaderAbsence, IConsumerContextAccessor contextAccessor)
        {
            _headerKey = headerKey;
            _addValueIfHeaderAbsence = addValueIfHeaderAbsence;
            _contextAccessor = contextAccessor;
        }

        /// <inheritdoc/>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var context = _contextAccessor.ConsumerContext;
            if (context == null)
            {
                return;
            }

            if (context.Properties.TryGetValue(CorrelationIdItemKey, out var value) && value is LogEventProperty logEventProperty)
            {
                logEvent.AddPropertyIfAbsent(logEventProperty);
                return;
            }

            context.Headers.TryGetValue(_headerKey, out object? headerValue);
            var requestHeader = headerValue?.ToString();

            string? correlationId;

            if (!string.IsNullOrWhiteSpace(requestHeader))
            {
                correlationId = requestHeader;
            }
            else if (_addValueIfHeaderAbsence)
            {
                correlationId = Guid.NewGuid().ToString();
            }
            else
            {
                correlationId = null;
            }

            var correlationIdProperty = new LogEventProperty(PropertyName, new ScalarValue(correlationId));
            logEvent.AddOrUpdateProperty(correlationIdProperty);

            context.Properties.Add(CorrelationIdItemKey, correlationIdProperty);
        }
    }
}
