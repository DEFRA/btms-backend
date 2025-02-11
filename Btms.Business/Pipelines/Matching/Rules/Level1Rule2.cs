namespace Btms.Business.Pipelines.Matching.Rules;

public class Level1Rule2 : MatchRuleBase
{
    public override async Task<PipelineResult> ProcessFilter(MatchContext context)
    {
        context.Record += "Did rule two" + Environment.NewLine;

        return await Task.FromResult(new PipelineResult(false));
    }
}