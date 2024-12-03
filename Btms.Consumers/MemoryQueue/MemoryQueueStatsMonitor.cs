using System.Collections.Concurrent;

namespace Btms.Consumers.MemoryQueue;

public class MemoryQueueStatsMonitor : IMemoryQueueStatsMonitor
{
    private readonly ConcurrentDictionary<string, QueueStats> queueStatsByPath = new();
    public void Enqueue(string queue)
    {
        var value = new QueueStats(queue);
        queueStatsByPath.AddOrUpdate(queue,
            _ =>
            {
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
        var value = new QueueStats(queue);
        queueStatsByPath.AddOrUpdate(queue,
            _ =>
            {
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
        return queueStatsByPath.GetValueOrDefault(name, new QueueStats(name));
    }
}