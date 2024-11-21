using Cdms.Backend.Data.Extensions;
using Cdms.BlobService;
using Cdms.BlobService.Extensions;
using Cdms.Business.Commands;
using Cdms.Business.Pipelines;
using Cdms.Business.Pipelines.Matching;
using Cdms.Business.Pipelines.Matching.Rules;
using Cdms.Common.Extensions;
using Cdms.SensitiveData;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cdms.Business.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.CdmsAddOptions<SensitiveDataOptions>(configuration, SensitiveDataOptions.SectionName);
            services.CdmsAddOptions<BusinessOptions>(configuration, BusinessOptions.SectionName);
            
            services.AddMongoDbContext(configuration);
            services.AddBlobStorage(configuration);
            services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();
            services.AddSingleton<ISensitiveDataSerializer, SensitiveDataSerializer>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SyncNotificationsCommand>());

            // hard code list for now, get via config -> reflection later
            List<Type> rules = new List<Type>
            {
                typeof(Level1Rule8),
                typeof(Level1Rule4),
                typeof(Level1Rule2),
                typeof(Level1Rule1),
                typeof(Level1RuleZ)
            };

            // Add matching pipelines
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(PipelineResult).Assembly);
                cfg.AddRequestPreProcessor<MatchPreProcess>();
                cfg.AddRequestPostProcessor<MatchPostProcess>();
                
                foreach (var rule in rules)
                {
                    
                    cfg.AddBehavior(typeof(IPipelineBehavior<MatchRequest, PipelineResult>), rule);
                }

                cfg.AddBehavior<IPipelineBehavior<MatchRequest, PipelineResult>, MatchTerminatePipeline>();
            });

            services.AddSingleton<SyncMetrics>();

            return services;
        }
    }
}