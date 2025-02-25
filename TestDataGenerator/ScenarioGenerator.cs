using System.Collections;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Scenarios;
using Decision = Btms.Types.Alvs.Decision;
using Finalisation = Btms.Types.Alvs.Finalisation;

namespace TestDataGenerator;

public abstract class ScenarioGenerator(ILogger logger)
{
    protected readonly ILogger Logger = logger;

    public abstract GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config);

    protected virtual List<object> ModifyMessages(
        List<object> messages)
    {
        return messages;
    }

    /// <summary>
    /// A class to hold a list of message types we support. Would be nice to use something
    /// other than object :|
    /// </summary>
    public class GeneratorResult : IEnumerable<object>
    {
        public GeneratorResult(object[] initial)
        {
            foreach (var o in initial)
            {
                if (o is ImportNotification or AlvsClearanceRequest or Decision or Finalisation
                    or SearchGmrsForDeclarationIdsResponse or Gmr)
                {
                    Messages.Add(o);
                }
                else
                {
                    throw new Exception($"Unexpected GeneratorResult type {o.GetType().Name}");
                }
            }
        }

        private List<object> Messages { get; set; } = new List<object>();

        public void Add(ImportNotification[] importNotifications)
        {
            Messages.AddRange(importNotifications);
        }

        public void Add(AlvsClearanceRequest[] clearanceRequests)
        {
            Messages.AddRange(clearanceRequests);
        }

        public void Add(Btms.Types.Alvs.Decision[] decisions)
        {
            Messages.AddRange(decisions);
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return Messages.GetEnumerator();
        }

        public int Count => Messages.Count;

        public IEnumerator GetEnumerator()
        {
            return Messages.GetEnumerator();
        }
    }
}