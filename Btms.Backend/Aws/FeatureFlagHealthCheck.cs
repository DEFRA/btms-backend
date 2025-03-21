using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;

namespace Btms.Backend.Aws;

public class FeatureFlagHealthCheck(IFeatureManager featureManager, string feature, IHealthCheck innerHeathCheck) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (await featureManager.IsEnabledAsync(feature))
        {
            return await innerHeathCheck.CheckHealthAsync(context, cancellationToken);
        }

        return HealthCheckResult.Healthy("Health Check disabled",
            new Dictionary<string, object>() { { "featureFlag", feature } });
    }
}