using System.Diagnostics;
using System.Diagnostics.Metrics;
using SlimMessageBus.Host;

namespace Cdms.Consumers.Metrics;

public class InMemoryQueueMetrics
{
    readonly Histogram<int> timeInQueue;
    readonly Counter<long> incomingCountMetric;
    readonly Counter<long> outgoingCountMetric;
    private long queueCount;
    private readonly int noOfQueues;
    public InMemoryQueueMetrics(IMeterFactory meterFactory, IMasterMessageBus messageBusProvider)
    {
        var meter = meterFactory.Create("Cdms");
        timeInQueue = meter.CreateHistogram<int>("messaging.memory.time_queued", "ms", "Elapsed time spent consuming a message, in millis");
        meter.CreateObservableUpDownCounter("messaging.memory.active", () => queueCount, description: "Number of messages in queue");
        incomingCountMetric = meter.CreateCounter<long>("messaging.memory.incoming", description: "Number of messages incoming");
        outgoingCountMetric = meter.CreateCounter<long>("messaging.memory.outgoing", description: "Number of messages outgoing");
        meter.CreateObservableCounter("messaging.memory.queues", () => noOfQueues, description: "Number of queues");
        noOfQueues = messageBusProvider.Settings.Children.First(x => x.Name == "InMemory").Consumers.Select(x => x.Path).Distinct().Count();
    }
    public void Incoming(string queueName)
    {
        var tagList = new TagList
        {
            { "service", Process.GetCurrentProcess().ProcessName },
            { "messaging.queue_name", queueName },
        };


        Interlocked.Increment(ref queueCount);
        incomingCountMetric.Add(1, tagList);
    }

    public void TimeSpentInQueue(int milliseconds, string queueName)
    {
        var tagList = new TagList
        {
            { "service", Process.GetCurrentProcess().ProcessName },
            { "messaging.queue_name", queueName },
        };

        timeInQueue.Record(milliseconds, tagList);
    }

    public void Outgoing(string queueName)
    {
        var tagList = new TagList
        {
            { "service", Process.GetCurrentProcess().ProcessName },
            { "messaging.queue_name", queueName },
        };

        outgoingCountMetric.Add(1, tagList);
    }

    public void Completed()
    {
        Interlocked.Decrement(ref queueCount);
    }
}