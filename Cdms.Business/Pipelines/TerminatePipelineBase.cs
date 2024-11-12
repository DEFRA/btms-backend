using MediatR;

namespace Cdms.Business.Pipelines;

public abstract class TerminatePipelineBase<TRequest> : IPipelineBehavior<TRequest, PipelineResult>
    where TRequest : notnull
{
    public async Task<PipelineResult> Handle(
        TRequest request,
        RequestHandlerDelegate<PipelineResult> next,
        CancellationToken cancellationToken)
    {
        var result = new PipelineResult(false);
        return await Task.FromResult(result);
    }
}