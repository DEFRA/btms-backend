using MediatR.Pipeline;

namespace Cdms.Business.Pipelines.Matching;

public class MatchPostProcess : IRequestPostProcessor<MatchRequest, PipelineResult>
{
    public Task Process(MatchRequest request, PipelineResult response, CancellationToken cancellationToken)
    {
        request.Context.Record += "Did Post Processing" + Environment.NewLine;
        return Task.CompletedTask;
    }
}