using Btms.Common.Extensions;
using Btms.Replication.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.Replication.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.BtmsAddOptions<ReplicationOptions>(configuration, ReplicationOptions.SectionName);
        services.AddSingleton<ReplicationTargetBlobService, ReplicationTargetBlobService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(ServiceCollectionExtensions)));

        return services;
    }
}