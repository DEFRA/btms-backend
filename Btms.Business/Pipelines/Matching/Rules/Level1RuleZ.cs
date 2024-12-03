namespace Btms.Business.Pipelines.Matching.Rules;

public class Level1RuleZ : MatchRuleBase
{
    public override async Task<PipelineResult> ProcessFilter(MatchContext context)
    {
        context.Record += "Did rule Z" + Environment.NewLine;
        
        return await Task.FromResult(new PipelineResult(false));
    }
}