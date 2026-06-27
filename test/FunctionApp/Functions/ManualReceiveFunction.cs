using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzuTopo.TestFunctionApp.Functions;

public class ManualReceiveFunction(ServiceBusClient serviceBusClient, ILogger<ManualReceiveFunction> logger)
{
    [Function("ManualReceive")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo timer)
    {
        await using var receiver = serviceBusClient.CreateReceiver("notifications");

        var messages = await receiver.ReceiveMessagesAsync(maxMessages: 10, maxWaitTime: TimeSpan.FromSeconds(5));
        foreach (var message in messages)
        {
            logger.LogInformation("ManualReceive got message {MessageId}: {Body}", message.MessageId, message.Body.ToString());
            await receiver.CompleteMessageAsync(message);
        }

        logger.LogInformation("ManualReceive processed {Count} messages", messages.Count);
    }
}
