using System.Net;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using HealthChecks.AzureServiceBus;
using System;

namespace Btms.Backend.Asb;

public class BtmsServiceBusClientProvider(IWebProxy proxy) : ServiceBusClientProvider
{
    public override ServiceBusClient CreateClient(string? connectionString)
    {
        var clientOptions = new ServiceBusClientOptions()
        {
            WebProxy = proxy,
            TransportType = ServiceBusTransportType.AmqpWebSockets,
        };
        return new ServiceBusClient(connectionString, clientOptions);
    }


}