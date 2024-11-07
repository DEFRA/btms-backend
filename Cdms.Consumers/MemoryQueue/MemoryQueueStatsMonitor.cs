using System.Collections.Concurrent;

namespace Cdms.Consumers.MemoryQueue;

public class MemoryQueueStatsMonitor : IMemoryQueueStatsMonitor
{
    private ConcurrentDictionary<string, QueueStats> queueStatsByPath = new ConcurrentDictionary<string, QueueStats>();
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
    }

    public IDictionary<string, QueueStats> GetAll()
    {
        return queueStatsByPath;
    }

    public QueueStats Get(string name)
    {
        return queueStatsByPath.GetValueOrDefault(name);
    }
}