using MediatR.Pipeline;

namespace Cdms.Business.Pipelines.Matching;

public class MatchPreProcess : IRequestPreProcessor<MatchRequest>
{
    public Task Process(MatchRequest request, CancellationToken cancellationToken)
    {
        request.Context.Record += $"Did pre-processing with initial request [{request.Context.MatchReference}]{Environment.NewLine}";
        return Task.CompletedTask;
    }
}