using MediatR;

namespace Cdms.Business.Pipelines;

public abstract class PipelineBase<TContext, TRequest> : IPipelineBehavior<TRequest, PipelineResult>
    where TRequest : PipelineRequest<TContext>
{
    public abstract Task<PipelineResult> ProcessFilter(TContext context);
    
    public async Task<PipelineResult> Handle(
        TRequest request,
        RequestHandlerDelegate<PipelineResult> next,
        CancellationToken cancellationToken)
    {
        var currentProgress = await ProcessFilter(request.Context);
        
        if (currentProgress.ExitPipeline)
        {
            return currentProgress;
        }
        
        return await next();
    }
}