using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Btms.Consumers;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Backend.IntegrationTests.Helpers;

public static class AzureServiceBusHelper
{
    private const string ConnectionString =
        "Endpoint=sb://127.0.0.1:5672;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

    public static Task PublishClearanceRequest(AlvsClearanceRequest request)
    {
        return PublishMessage(request, "dev_alvs_topic_vnet", AlvsMessageTypes.ALVSClearanceRequest);
    }

    public static Task PublishDecision(Decision request)
    {
        return PublishMessage(request, "dev_alvs_topic_vnet", AlvsMessageTypes.ALVSDecisionNotification);
    }

    public static Task PublishNotification(ImportNotification request)
    {
        return PublishMessage(request, "dev_notification_topic_vnet");
    }

    public static Task PublishGmr(Gmr request)
    {
        return PublishMessage(request, "dev_gmr_topic_vnet");
    }

    private static async Task PublishMessage<T>(T request, string topicName, string? messageType = null)
    {
        await using var client = new ServiceBusClient(ConnectionString);

        var sender = client.CreateSender(topicName);
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)))
        {
            ContentType = "application/json"
        };

        if (messageType is not null)
        {
            message.ApplicationProperties.Add("messageType", messageType);
        }

        await sender.SendMessageAsync(message);
    }
}