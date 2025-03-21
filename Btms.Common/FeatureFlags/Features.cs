using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Btms.Common.FeatureFlags
{

    public static class FeatureExtensions
    {
        public static IServiceCollection RunIfFeatureEnabled(this IServiceCollection services, string feature,
            Action action)
        {
            if (services.BuildServiceProvider().GetRequiredService<IFeatureManager>().IsEnabledAsync(feature)
                .GetAwaiter().GetResult())
            {
                action();
            }

            return services;
        }
    }
    public static class Features
    {
        public const string SyncPerformanceEnhancements = nameof(SyncPerformanceEnhancements);
        public const string Validation = nameof(Validation);

        public static class HealthChecks
        {
            public static class Sqs
            {
                public const string ClearanceRequests = $"{nameof(HealthChecks)}_{nameof(Sqs)}_{nameof(ClearanceRequests)}";
                public const string Decisions = $"{nameof(HealthChecks)}_{nameof(Sqs)}_{nameof(Decisions)}";
                public const string Finalisations = $"{nameof(HealthChecks)}_{nameof(Sqs)}_{nameof(Finalisations)}";
            }

            public static class Asb
            {
                public const string Alvs = $"{nameof(HealthChecks)}_{nameof(Asb)}_{nameof(Alvs)}";
                public const string Ipaffs = $"{nameof(HealthChecks)}_{nameof(Asb)}_{nameof(Ipaffs)}";
                public const string Gmr = $"{nameof(HealthChecks)}_{nameof(Asb)}_{nameof(Gmr)}";
            }
        }

        public static class Consumers
        {
            public static class Sqs
            {
                public const string ClearanceRequests = $"{nameof(Consumers)}_{nameof(Sqs)}_{nameof(ClearanceRequests)}";
                public const string Decisions = $"{nameof(Consumers)}_{nameof(Sqs)}_{nameof(Decisions)}";
                public const string Finalisations = $"{nameof(Consumers)}_{nameof(Sqs)}_{nameof(Finalisations)}";
            }

            public static class Asb
            {
                public const string Alvs = $"{nameof(Consumers)}_{nameof(Asb)}_{nameof(Alvs)}";
                public const string Ipaffs = $"{nameof(Consumers)}_{nameof(Asb)}_{nameof(Ipaffs)}";
                public const string Gmr = $"{nameof(Consumers)}_{nameof(Asb)}_{nameof(Gmr)}";
            }
        }
    }
}