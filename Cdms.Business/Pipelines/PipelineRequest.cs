using MediatR;

namespace Cdms.Business.Pipelines;

public record PipelineRequest<TContext>(TContext Context) : IRequest<PipelineResult>;