using Btms.Business.Commands;
using Btms.Business.Pipelines;
using Btms.Business.Pipelines.Matching;
using Btms.Business.Pipelines.Matching.Rules;
using Btms.Backend.Data.Extensions;
using Btms.BlobService;
using Btms.BlobService.Extensions;
using Btms.Business.Builders;
using Btms.Common.Extensions;
using Btms.Metrics.Extensions;
using Btms.SensitiveData;
using Btms.Types.Ipaffs;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Business.Services.Linking;
using Btms.Business.Services.Matching;
using Btms.Business.Services.Validating;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Validation;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Validation;
using Btms.Types.Gvms;
using Btms.Validation;
using Btms.Validation.Extensions;
using Finalisation = Btms.Types.Alvs.Finalisation;

namespace Btms.Business.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBtmsValidation(setup =>
        {
            setup.AddValidator<Finalisation, FinalisationValidator>();
            setup.AddValidator<AlvsClearanceRequest, AlvsClearanceRequestValidator>();
            setup.AddValidator<BtmsValidationPair<CdsClearanceRequest, Movement>, CdsClearanceRequestValidator>();
            setup.AddValidator<BtmsValidationPair<CdsFinalisation, Movement>, CdsFinalisationValidator>();
        });
        services.AddBtmsMetrics();
        services.BtmsAddOptions<SensitiveDataOptions>(configuration, SensitiveDataOptions.SectionName);
        services.BtmsAddOptions<BusinessOptions>(configuration, BusinessOptions.SectionName);

        services.AddMongoDbContext(configuration);
        services.AddBlobStorage(configuration);
        services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();
        services.AddSingleton<ISensitiveDataSerializer, SensitiveDataSerializer>();
        services.AddSingleton<ISensitiveFieldsProvider, SensitiveFieldsProvider>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(ServiceCollectionExtensions)));

        // hard code list for now, get via config -> reflection later
        List<Type> rules =
        [
            typeof(Level1Rule8),
            typeof(Level1Rule4),
            typeof(Level1Rule2),
            typeof(Level1Rule1),
            typeof(Level1RuleZ)
        ];

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
        services.AddScoped<ILinker<Model.Ipaffs.ImportNotification, Model.Gvms.Gmr>, ImportNotificationGmrLinker>();
        services.AddScoped<ILinker<Model.Gvms.Gmr, Model.Ipaffs.ImportNotification>, ImportNotificationGmrLinker>();
        services.Decorate(typeof(ILinker<,>), typeof(LoggingLinker<,>));

        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IDecisionService, DecisionService>();
        services.AddScoped<IMatchingService, MatchingService>();

        services.AddScoped<IDecisionFinder, ChedADecisionFinder>();
        services.AddScoped<IDecisionFinder, ChedDDecisionFinder>();
        services.AddScoped<IDecisionFinder, ChedPDecisionFinder>();
        services.AddScoped<IDecisionFinder, ChedPPDecisionFinder>();
        services.AddScoped<IDecisionFinder, IuuDecisionFinder>();

        services.AddTransient<DecisionStatusFinder>();
        services.AddTransient<MovementBuilderFactory>();
        services.AddScoped<IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification>, ImportNotificationPreProcessor>();
        services.AddScoped<IPreProcessor<AlvsClearanceRequest, Model.Movement>, MovementPreProcessor>();
        services.AddScoped<IPreProcessor<Gmr, Model.Gvms.Gmr>, GmrPreProcessor>();

        return services;
    }
}