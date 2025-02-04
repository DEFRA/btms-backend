using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class Mrn24GBD46KPSVZ3DFAR2ExceptionEndScenarioGenerator(IServiceProvider sp, ILogger<Mrn24GBD46KPSVZ3DFAR2ExceptionEndScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger, "Mrn-24GBD46KPSVZ3DFAR2")
{

    protected override List<object> ModifyMessages(List<object> messages)
    {
        foreach (var message in messages)
        {
            if (message is Decision decision)
            {
                foreach (var decisionItem in decision.Items!)
                {
                    foreach (var decisionItemCheck in decisionItem.Checks!)
                    {
                        decisionItemCheck.DecisionCode = "T03";
                    }
                }
            }
        }
        return messages;

    }
}