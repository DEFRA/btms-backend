using System.ComponentModel.DataAnnotations;
using Btms.Azure;

namespace Btms.Consumers;

public class ConsumerOptions
{
    public const string SectionName = nameof(ConsumerOptions);

    public Dictionary<string, int> InMemoryInstances = [];

    public int InMemoryNotifications = 2;
    // public int InMemoryNotifications = 2; 

    public int GetInMemoryInstances(string topic, int defaultInstances = 2)
    {
        return InMemoryInstances.GetValueOrDefault("NOTIFICATIONS", defaultInstances);
    }
    // public int GetInstanceCount(string bus, string topic, int defaultInstances = 2)
    // {
    //     if (!Instances.TryGetValue(bus, out var instance)) return defaultInstances;
    //     
    //     return instance.GetValueOrDefault(topic, defaultInstances);
    // }
}