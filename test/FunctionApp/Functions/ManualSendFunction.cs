using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzuTopo.TestFunctionApp.Functions;

public class ManualSendFunction(ServiceBusClient serviceBusClient, ILogger<ManualSendFunction> logger)
{
    [Function("ManualSend")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications")] HttpRequestData req)
    {
        var body = await req.ReadAsStringAsync();
        logger.LogInformation("ManualSend triggered. Body length: {Length}", body?.Length ?? 0);

        await using var sender = serviceBusClient.CreateSender("notifications");

        var messageId = Guid.NewGuid().ToString();
        var payload = JsonSerializer.Serialize(new { MessageId = messageId, SentAt = DateTimeOffset.UtcNow, Payload = body });
        var message = new ServiceBusMessage(payload) { MessageId = messageId };

        await sender.SendMessageAsync(message);
        logger.LogInformation("Sent message {MessageId} to notifications queue", messageId);

        var response = req.CreateResponse(HttpStatusCode.Accepted);
        await response.WriteAsJsonAsync(new { MessageId = messageId });
        return response;
    }
}
