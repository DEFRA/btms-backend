namespace Btms.Consumers.MemoryQueue;

public interface IMemoryQueueStatsMonitor
{
    void Enqueue(string queue);

    void Dequeue(string queue);

    IDictionary<string, QueueStats> GetAll();

    QueueStats Get(string name);
}