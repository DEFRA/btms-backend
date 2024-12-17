using System.Collections;
using AutoFixture;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using TestDataGenerator.Scenarios;

namespace TestDataGenerator;

public abstract class ScenarioGenerator
{
    private readonly string _fullFolder =
        $"{Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)}/Scenarios/Samples";

    public abstract GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config);

    internal ImportNotificationBuilder<ImportNotification> GetNotificationBuilder(string file)
    {
        var fullPath = $"{_fullFolder}/{file}.json";
        var builder = ImportNotificationBuilder.FromFile(fullPath);

        return builder;
    }

    internal ClearanceRequestBuilder GetClearanceRequestBuilder(string file)
    {
        var fullPath = $"{_fullFolder}/{file}.json";
        var builder = new ClearanceRequestBuilder(fullPath);

        return builder;
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
                if (o is ImportNotification || o is AlvsClearanceRequest)
                {
                    Messages.Add(o);
                }
                else
                {
                    throw new Exception("Unexpected type");
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