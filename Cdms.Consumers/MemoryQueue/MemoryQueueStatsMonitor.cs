using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Cdms.Consumers.MemoryQueue;

public class MemoryQueueStatsMonitor : IMemoryQueueStatsMonitor
{
    private readonly ConcurrentDictionary<string, QueueStats> queueStatsByPath = new ConcurrentDictionary<string, QueueStats>();
    readonly UpDownCounter<long> queueCount;

    public MemoryQueueStatsMonitor(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Cdms");
        queueCount = meter.CreateUpDownCounter<long>("messaging.cdms.memory.queue", "ea", "Number of messages in queue");
    }
    public void Enqueue(string queue)
    {
        queueStatsByPath.AddOrUpdate(queue,
            s =>
            {
                var value = new QueueStats(queue);
                value.Enqueue();
                return value;

            }, (s, stats) =>
            {
                stats.Enqueue();
                return stats;
            });

        var tagList = new TagList
        {
            { "messaging.cdms.service", Process.GetCurrentProcess().ProcessName },
            { "messaging.cdms.queue", queue },
        };
        queueCount.Add(1, tagList);
    }

    public void Dequeue(string queue)
    {
        queueStatsByPath.AddOrUpdate(queue,
            s =>
            {
                var value = new QueueStats(queue);
                value.Dequeue();
                return value;

            }, (s, stats) =>
            {
                stats.Dequeue();
                return stats;
            });

        var tagList = new TagList
        {
            { "messaging.cdms.service", Process.GetCurrentProcess().ProcessName },
            { "messaging.cdms.queue", queue },
        };
        queueCount.Add(-1, tagList);
    }

    public IDictionary<string, QueueStats> GetAll()
    {
        return queueStatsByPath;
    }

    public QueueStats Get(string name)
    {
        return queueStatsByPath.GetValueOrDefault(name, new QueueStats(name));
    }
}