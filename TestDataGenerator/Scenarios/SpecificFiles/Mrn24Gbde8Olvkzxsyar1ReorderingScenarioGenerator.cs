using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class Mrn24Gbde8Olvkzxsyar1ImportNotificationsAtEndScenarioGenerator(IServiceProvider sp, ILogger<Mrn24Gbde8Olvkzxsyar1ImportNotificationsAtEndScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger, "Mrn-24GBDE8OLVKZXSYAR1")
{
    private sealed class PutImportNotificationsAtEndComparer : IComparer<object>
    {
        public int Compare(object? x, object? y)
        {
            if (x is ImportNotification && y is ImportNotification)
                return x!.CreatedDate().CompareTo(y!.CreatedDate());
            else if (y is ImportNotification)
                return -1;
            else
                return x!.CreatedDate().CompareTo(y!.CreatedDate());
        }
    }

    protected override List<object> ModifyMessages(List<object> messages)
    {
        return messages
            .Order(new PutImportNotificationsAtEndComparer())
            .ToList();
    }
}