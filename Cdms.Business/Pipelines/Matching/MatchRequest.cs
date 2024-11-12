namespace Cdms.Business.Pipelines.Matching;

public record MatchRequest(MatchContext Context) : PipelineRequest<MatchContext>(Context);