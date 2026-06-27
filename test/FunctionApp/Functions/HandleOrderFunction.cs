using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AzuTopo.TestFunctionApp.Functions;

public class HandleOrderFunction(ServiceBusClient serviceBusClient, ILogger<HandleOrderFunction> logger)
{
    [Function("HandleOrder")]
    public async Task Run(
        [ServiceBusTrigger("orders", Connection = "ServiceBusConnectionString")] string message)
    {
        logger.LogInformation("HandleOrder triggered. Message: {Message}", message);

        await using var sender = serviceBusClient.CreateSender("notifications");
        var notification = JsonSerializer.Serialize(new
        {
            Source = "HandleOrder",
            ProcessedAt = DateTimeOffset.UtcNow,
            OriginalMessage = message,
        });
        await sender.SendMessageAsync(new ServiceBusMessage(notification));
        logger.LogInformation("HandleOrder forwarded to notifications queue");
    }
}
