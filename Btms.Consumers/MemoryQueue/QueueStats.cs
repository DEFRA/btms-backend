namespace Btms.Consumers.MemoryQueue;

public class QueueStats(string name)
{
    private int count;
    public string Name { get; } = name;

    public int Count => count;

    public void Enqueue()
    {
        Interlocked.Increment(ref count);
    }

    public void Dequeue()
    {
        Interlocked.Decrement(ref count);
    }
}