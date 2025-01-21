using Btms.Types.Alvs;

namespace TestDataGenerator.Extensions;

public static class DecisionBuilderExtensions
{
    public static DecisionBuilder<Decision> WithTunaChecks(this DecisionBuilder<Decision> builder)
    {
        return builder
            .WithItemAndCheck(1, "H222", "H01")
            .WithItemAndCheck(1, "H224", "C07");
    }
    
    public static DecisionBuilder<Decision> WithClearanceRequestDecisions(this DecisionBuilder<Decision> builder, AlvsClearanceRequest clearanceRequest, string decisionCode = "H01")
    {
        return builder
            .Do(d =>
            {
                d.Items = clearanceRequest
                    .Items?
                    .Select(i => new Items()
                    {
                        ItemNumber = i.ItemNumber,
                        Documents = i.Documents,
                        Checks = i.Checks?
                            .Select(c => new Check() { CheckCode = c.CheckCode, DecisionCode = decisionCode }).ToArray()
                    })
                    .ToArray();
            });
    }
}