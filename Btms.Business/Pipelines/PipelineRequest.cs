using MediatR;

namespace Btms.Business.Pipelines;

public record PipelineRequest<TContext>(TContext Context) : IRequest<PipelineResult>;