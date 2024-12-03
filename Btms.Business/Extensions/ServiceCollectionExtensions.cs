using Btms.Business.Commands;
using Btms.Business.Pipelines;
using Btms.Business.Pipelines.Matching;
using Btms.Business.Pipelines.Matching.Rules;
using Btms.Business.Services;
using Btms.Backend.Data.Extensions;
using Btms.BlobService;
using Btms.BlobService.Extensions;
using Btms.Common.Extensions;
using Btms.Metrics.Extensions;
using Btms.SensitiveData;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.Business.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddBtmsMetrics();
            services.BtmsAddOptions<SensitiveDataOptions>(configuration, SensitiveDataOptions.SectionName);
            services.BtmsAddOptions<BusinessOptions>(configuration, BusinessOptions.SectionName);
            
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

            services.AddScoped<ILinkingService, LinkingService>();

            return services;
        }
    }
}